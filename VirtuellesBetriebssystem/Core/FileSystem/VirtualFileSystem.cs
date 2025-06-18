using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Implementation des virtuellen Dateisystems
/// </summary>
public class VirtualFileSystem : IVirtualFileSystem
{
    private readonly string _currentUser = "user";
    
    /// <summary>
    /// Gibt das Root-Verzeichnis zurück
    /// </summary>
    public VirtualDirectory Root { get; }
    
    /// <summary>
    /// Aktuelles Arbeitsverzeichnis
    /// </summary>
    public VirtualDirectory CurrentDirectory { get; private set; }
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich das Dateisystem ändert
    /// </summary>
    public event EventHandler<FileSystemEventArgs> FileSystemChanged;
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    public VirtualFileSystem()
    {
        // Root-Verzeichnis erstellen
        Root = new VirtualDirectory("/", null, "root");
        CurrentDirectory = Root;
        
        // Grundlegende Verzeichnisstruktur erstellen
        CreateBasicFileSystem();
    }
    
    /// <summary>
    /// Erstellt eine neue Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <param name="content">Inhalt der Datei</param>
    /// <returns>Die erstellte Datei</returns>
    public VirtualFile CreateFile(string path, string content = "")
    {
        path = NormalizePath(path);
        
        string directoryPath = Path.GetDirectoryName(path).Replace("\\", "/");
        string fileName = Path.GetFileName(path);
        
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("Ungültiger Dateiname.");
            
        var directory = GetDirectory(directoryPath);
        if (directory == null)
            throw new DirectoryNotFoundException($"Verzeichnis '{directoryPath}' nicht gefunden.");
            
        if (!directory.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{directoryPath}'.");
            
        if (directory.GetFile(fileName) != null)
            throw new IOException($"Datei '{fileName}' existiert bereits.");
            
        var file = new VirtualFile(fileName, directory, content, _currentUser);
        directory.AddFile(file);
        
        // Event auslösen
        FireFileSystemChanged(path, FileSystemChangeType.Created, FileSystemElementType.File);
        
        return file;
    }
    
    /// <summary>
    /// Erstellt ein neues Verzeichnis
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Das erstellte Verzeichnis</returns>
    public VirtualDirectory CreateDirectory(string path)
    {
        path = NormalizePath(path);
        
        if (path == "/" || path == "")
            return Root;
            
        // Prüfen, ob das Verzeichnis bereits existiert
        var existingDir = GetDirectory(path);
        if (existingDir != null)
            return existingDir;
            
        string parentPath = Path.GetDirectoryName(path).Replace("\\", "/");
        if (string.IsNullOrEmpty(parentPath))
            parentPath = "/";
            
        string dirName = Path.GetFileName(path);
        
        if (string.IsNullOrEmpty(dirName))
            throw new ArgumentException("Ungültiger Verzeichnisname.");
            
        var parentDir = GetDirectory(parentPath);
        if (parentDir == null)
        {
            // Versuchen, übergeordnetes Verzeichnis rekursiv zu erstellen
            parentDir = CreateDirectory(parentPath);
        }
        
        if (!parentDir.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{parentPath}'.");
            
        if (parentDir.GetSubdirectory(dirName) != null)
            throw new IOException($"Verzeichnis '{dirName}' existiert bereits.");
            
        var directory = new VirtualDirectory(dirName, parentDir, _currentUser);
        parentDir.AddSubdirectory(directory);
        
        // Event auslösen
        FireFileSystemChanged(path, FileSystemChangeType.Created, FileSystemElementType.Directory);
        
        return directory;
    }
    
    /// <summary>
    /// Löscht eine Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>True, wenn die Datei gelöscht wurde, False sonst</returns>
    public bool DeleteFile(string path)
    {
        path = NormalizePath(path);
        
        string directoryPath = Path.GetDirectoryName(path).Replace("\\", "/");
        string fileName = Path.GetFileName(path);
        
        var directory = GetDirectory(directoryPath);
        if (directory == null)
            return false;
            
        if (!directory.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{directoryPath}'.");
            
        var file = directory.GetFile(fileName);
        if (file == null)
            return false;
            
        // Prüfen, ob die Datei vom aktuellen Benutzer gelöscht werden darf
        if (file.Owner != _currentUser && _currentUser != "root")
            throw new UnauthorizedAccessException($"Keine Berechtigung zum Löschen der Datei '{path}'.");
            
        bool result = directory.RemoveFile(fileName);
        
        if (result)
        {
            // Event auslösen
            FireFileSystemChanged(path, FileSystemChangeType.Deleted, FileSystemElementType.File);
        }
        
        return result;
    }
    
    /// <summary>
    /// Löscht ein Verzeichnis
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <param name="recursive">Ob rekursiv gelöscht werden soll</param>
    /// <returns>True, wenn das Verzeichnis gelöscht wurde, False sonst</returns>
    public bool DeleteDirectory(string path, bool recursive = false)
    {
        path = NormalizePath(path);
        
        if (path == "/" || path == "")
            throw new InvalidOperationException("Das Root-Verzeichnis kann nicht gelöscht werden.");
            
        string parentPath = Path.GetDirectoryName(path).Replace("\\", "/");
        if (string.IsNullOrEmpty(parentPath))
            parentPath = "/";
            
        string dirName = Path.GetFileName(path);
        
        var parentDir = GetDirectory(parentPath);
        if (parentDir == null)
            return false;
            
        if (!parentDir.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{parentPath}'.");
            
        var directory = parentDir.GetSubdirectory(dirName);
        if (directory == null)
            return false;
            
        // Prüfen, ob das Verzeichnis vom aktuellen Benutzer gelöscht werden darf
        if (directory.Owner != _currentUser && _currentUser != "root")
            throw new UnauthorizedAccessException($"Keine Berechtigung zum Löschen des Verzeichnisses '{path}'.");
            
        // Prüfen, ob das Verzeichnis leer ist oder rekursiv gelöscht werden soll
        if (!recursive && (directory.GetFiles().Any() || directory.GetSubdirectories().Any()))
            throw new IOException($"Verzeichnis '{path}' ist nicht leer. Verwenden Sie den Parameter 'recursive'.");
            
        bool result = parentDir.RemoveSubdirectory(dirName);
        
        if (result)
        {
            // Event auslösen
            FireFileSystemChanged(path, FileSystemChangeType.Deleted, FileSystemElementType.Directory);
        }
        
        return result;
    }
    
    /// <summary>
    /// Prüft, ob eine Datei existiert
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>True, wenn die Datei existiert, False sonst</returns>
    public bool FileExists(string path)
    {
        return GetFile(path) != null;
    }
    
    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>True, wenn das Verzeichnis existiert, False sonst</returns>
    public bool DirectoryExists(string path)
    {
        return GetDirectory(path) != null;
    }
    
    /// <summary>
    /// Liest den Inhalt einer Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>Inhalt der Datei</returns>
    public string ReadFile(string path)
    {
        path = NormalizePath(path);
        
        var file = GetFile(path);
        if (file == null)
            throw new FileNotFoundException($"Datei '{path}' nicht gefunden.");
            
        if (!file.CanRead(_currentUser))
            throw new UnauthorizedAccessException($"Keine Leserechte für die Datei '{path}'.");
            
        file.MarkAsAccessed();
        return file.Content;
    }
    
    /// <summary>
    /// Schreibt Inhalt in eine Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <param name="content">Zu schreibender Inhalt</param>
    public void WriteFile(string path, string content)
    {
        path = NormalizePath(path);
        
        var file = GetFile(path);
        if (file == null)
        {
            // Datei existiert nicht, erstellen
            CreateFile(path, content);
            return;
        }
        
        if (!file.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte für die Datei '{path}'.");
            
        if (file.IsReadOnly)
            throw new UnauthorizedAccessException($"Datei '{path}' ist schreibgeschützt.");
            
        file.UpdateContent(content);
        
        // Event auslösen
        FireFileSystemChanged(path, FileSystemChangeType.Modified, FileSystemElementType.File);
    }
    
    /// <summary>
    /// Verschiebt eine Datei
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn die Datei verschoben wurde, False sonst</returns>
    public bool MoveFile(string sourcePath, string destinationPath)
    {
        sourcePath = NormalizePath(sourcePath);
        destinationPath = NormalizePath(destinationPath);
        
        // Kopieren und dann löschen
        if (CopyFile(sourcePath, destinationPath))
        {
            if (DeleteFile(sourcePath))
            {
                // Event auslösen
                FireFileSystemChanged(sourcePath, FileSystemChangeType.Moved, FileSystemElementType.File);
                return true;
            }
            
            // Wenn das Löschen fehlschlägt, auch das Ziel löschen
            DeleteFile(destinationPath);
        }
        
        return false;
    }
    
    /// <summary>
    /// Verschiebt ein Verzeichnis
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn das Verzeichnis verschoben wurde, False sonst</returns>
    public bool MoveDirectory(string sourcePath, string destinationPath)
    {
        sourcePath = NormalizePath(sourcePath);
        destinationPath = NormalizePath(destinationPath);
        
        if (sourcePath == "/" || destinationPath == "/")
            throw new InvalidOperationException("Das Root-Verzeichnis kann nicht verschoben werden.");
            
        var sourceDir = GetDirectory(sourcePath);
        if (sourceDir == null)
            return false;
            
        var sourceParentPath = Path.GetDirectoryName(sourcePath).Replace("\\", "/");
        if (string.IsNullOrEmpty(sourceParentPath))
            sourceParentPath = "/";
            
        var sourceParent = GetDirectory(sourceParentPath);
        if (sourceParent == null)
            return false;
            
        if (!sourceParent.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{sourceParentPath}'.");
            
        // Prüfen, ob das Zielverzeichnis bereits existiert
        if (DirectoryExists(destinationPath))
            throw new IOException($"Verzeichnis '{destinationPath}' existiert bereits.");
            
        var destParentPath = Path.GetDirectoryName(destinationPath).Replace("\\", "/");
        if (string.IsNullOrEmpty(destParentPath))
            destParentPath = "/";
            
        var destParent = GetDirectory(destParentPath);
        if (destParent == null)
        {
            // Zielverzeichnis erstellen
            destParent = CreateDirectory(destParentPath);
        }
        
        if (!destParent.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{destParentPath}'.");
            
        var destName = Path.GetFileName(destinationPath);
        
        // Neues Zielverzeichnis erstellen
        var destDir = new VirtualDirectory(destName, destParent, sourceDir.Owner, sourceDir.Permissions);
        destParent.AddSubdirectory(destDir);
        
        // Alle Dateien kopieren
        foreach (var file in sourceDir.GetFiles())
        {
            var newFile = new VirtualFile(file.Name, destDir, file.Content, file.Owner, file.Permissions);
            destDir.AddFile(newFile);
        }
        
        // Unterverzeichnisse rekursiv kopieren
        foreach (var subdir in sourceDir.GetSubdirectories())
        {
            var newSourcePath = CombinePath(sourcePath, subdir.Name);
            var newDestPath = CombinePath(destinationPath, subdir.Name);
            MoveDirectory(newSourcePath, newDestPath);
        }
        
        // Quellverzeichnis löschen
        bool result = sourceParent.RemoveSubdirectory(sourceDir.Name);
        
        if (result)
        {
            // Event auslösen
            FireFileSystemChanged(sourcePath, FileSystemChangeType.Moved, FileSystemElementType.Directory);
        }
        
        return result;
    }
    
    /// <summary>
    /// Kopiert eine Datei
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn die Datei kopiert wurde, False sonst</returns>
    public bool CopyFile(string sourcePath, string destinationPath)
    {
        sourcePath = NormalizePath(sourcePath);
        destinationPath = NormalizePath(destinationPath);
        
        var sourceFile = GetFile(sourcePath);
        if (sourceFile == null)
            return false;
            
        if (!sourceFile.CanRead(_currentUser))
            throw new UnauthorizedAccessException($"Keine Leserechte für die Datei '{sourcePath}'.");
            
        var destDirPath = Path.GetDirectoryName(destinationPath).Replace("\\", "/");
        if (string.IsNullOrEmpty(destDirPath))
            destDirPath = "/";
            
        var destDir = GetDirectory(destDirPath);
        if (destDir == null)
        {
            // Zielverzeichnis erstellen
            destDir = CreateDirectory(destDirPath);
        }
        
        if (!destDir.CanWrite(_currentUser))
            throw new UnauthorizedAccessException($"Keine Schreibrechte im Verzeichnis '{destDirPath}'.");
            
        var destFileName = Path.GetFileName(destinationPath);
        
        // Prüfen, ob die Zieldatei bereits existiert
        var existingDestFile = destDir.GetFile(destFileName);
        if (existingDestFile != null)
        {
            if (!existingDestFile.CanWrite(_currentUser))
                throw new UnauthorizedAccessException($"Keine Schreibrechte für die Datei '{destinationPath}'.");
                
            if (existingDestFile.IsReadOnly)
                throw new UnauthorizedAccessException($"Datei '{destinationPath}' ist schreibgeschützt.");
                
            // Vorhandene Datei überschreiben
            existingDestFile.UpdateContent(sourceFile.Content);
        }
        else
        {
            // Neue Datei erstellen
            var newFile = new VirtualFile(destFileName, destDir, sourceFile.Content, _currentUser);
            destDir.AddFile(newFile);
        }
        
        // Event auslösen
        FireFileSystemChanged(destinationPath, FileSystemChangeType.Created, FileSystemElementType.File);
        
        return true;
    }
    
    /// <summary>
    /// Listet alle Dateien in einem Verzeichnis auf
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Liste der Dateien</returns>
    public IEnumerable<VirtualFile> GetFiles(string path)
    {
        path = NormalizePath(path);
        
        var directory = GetDirectory(path);
        if (directory == null)
            throw new DirectoryNotFoundException($"Verzeichnis '{path}' nicht gefunden.");
            
        if (!directory.CanRead(_currentUser))
            throw new UnauthorizedAccessException($"Keine Leserechte für das Verzeichnis '{path}'.");
            
        return directory.GetFiles();
    }
    
    /// <summary>
    /// Listet alle Verzeichnisse in einem Verzeichnis auf
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Liste der Verzeichnisse</returns>
    public IEnumerable<VirtualDirectory> GetDirectories(string path)
    {
        path = NormalizePath(path);
        
        var directory = GetDirectory(path);
        if (directory == null)
            throw new DirectoryNotFoundException($"Verzeichnis '{path}' nicht gefunden.");
            
        if (!directory.CanRead(_currentUser))
            throw new UnauthorizedAccessException($"Keine Leserechte für das Verzeichnis '{path}'.");
            
        return directory.GetSubdirectories();
    }
    
    /// <summary>
    /// Holt eine Datei anhand ihres Pfades
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>Die Datei oder null, wenn nicht gefunden</returns>
    public VirtualFile GetFile(string path)
    {
        path = NormalizePath(path);
        
        if (path == "/" || path == "")
            return null;
            
        string directoryPath = Path.GetDirectoryName(path).Replace("\\", "/");
        if (string.IsNullOrEmpty(directoryPath))
            directoryPath = "/";
            
        string fileName = Path.GetFileName(path);
        
        var directory = GetDirectory(directoryPath);
        if (directory == null)
            return null;
            
        return directory.GetFile(fileName);
    }
    
    /// <summary>
    /// Holt ein Verzeichnis anhand seines Pfades
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Das Verzeichnis oder null, wenn nicht gefunden</returns>
    public VirtualDirectory GetDirectory(string path)
    {
        path = NormalizePath(path);
        
        if (path == "/" || path == "")
            return Root;
            
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = Root;
        
        foreach (var segment in segments)
        {
            current = current.GetSubdirectory(segment);
            if (current == null)
                return null;
        }
        
        return current;
    }
    
    /// <summary>
    /// Normalisiert einen Pfad
    /// </summary>
    /// <param name="path">Der zu normalisierende Pfad</param>
    /// <returns>Der normalisierte Pfad</returns>
    public string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "/";
            
        // Windows-Pfadtrenner durch Unix-Pfadtrenner ersetzen
        path = path.Replace("\\", "/");
        
        // Relativen Pfad zum aktuellen Verzeichnis umwandeln
        if (!path.StartsWith("/"))
        {
            path = CombinePath(CurrentDirectory.FullPath, path);
        }
        
        // Doppelte Schrägstriche entfernen
        while (path.Contains("//"))
        {
            path = path.Replace("//", "/");
        }
        
        // Pfad mit '.' und '..' auflösen
        var segments = new List<string>();
        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;
                
            if (segment == "..")
            {
                if (segments.Count > 0)
                    segments.RemoveAt(segments.Count - 1);
            }
            else
            {
                segments.Add(segment);
            }
        }
        
        return "/" + string.Join("/", segments);
    }
    
    /// <summary>
    /// Kombiniert Pfadsegmente
    /// </summary>
    /// <param name="paths">Die zu kombinierenden Pfadsegmente</param>
    /// <returns>Der kombinierte Pfad</returns>
    public string CombinePath(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return "/";
            
        var result = string.Join("/", paths);
        
        // Windows-Pfadtrenner durch Unix-Pfadtrenner ersetzen
        result = result.Replace("\\", "/");
        
        // Doppelte Schrägstriche entfernen
        while (result.Contains("//"))
        {
            result = result.Replace("//", "/");
        }
        
        return result;
    }
    
    /// <summary>
    /// Wechselt das aktuelle Arbeitsverzeichnis
    /// </summary>
    /// <param name="path">Pfad des neuen Arbeitsverzeichnisses</param>
    /// <returns>True, wenn der Wechsel erfolgreich war, False sonst</returns>
    public bool ChangeDirectory(string path)
    {
        path = NormalizePath(path);
        
        var directory = GetDirectory(path);
        if (directory == null)
            return false;
            
        if (!directory.CanExecute(_currentUser))
            throw new UnauthorizedAccessException($"Keine Ausführrechte für das Verzeichnis '{path}'.");
            
        CurrentDirectory = directory;
        return true;
    }
    
    /// <summary>
    /// Erstellt die grundlegende Verzeichnisstruktur des Dateisystems
    /// </summary>
    private void CreateBasicFileSystem()
    {
        // Hauptverzeichnisse
        var bin = new VirtualDirectory("bin", Root, "root");
        var etc = new VirtualDirectory("etc", Root, "root");
        var home = new VirtualDirectory("home", Root, "root");
        var usr = new VirtualDirectory("usr", Root, "root");
        var var = new VirtualDirectory("var", Root, "root");
        var tmp = new VirtualDirectory("tmp", Root, "root", FileSystemPermissions.All);
        
        Root.AddSubdirectory(bin);
        Root.AddSubdirectory(etc);
        Root.AddSubdirectory(home);
        Root.AddSubdirectory(usr);
        Root.AddSubdirectory(var);
        Root.AddSubdirectory(tmp);
        
        // Benutzerverzeichnis
        var userHome = new VirtualDirectory("user", home, "user");
        home.AddSubdirectory(userHome);
        
        // Beispiel-Dateien
        var welcomeFile = new VirtualFile("welcome.txt", userHome, 
            "Willkommen im virtuellen Betriebssystem-Simulator!\n\n" +
            "Dies ist eine vollständig simulierte Umgebung, in der Sie mit einem virtuellen Dateisystem, " +
            "Terminal und anderen OS-Komponenten interagieren können.\n\n" +
            "Viel Spaß beim Erkunden!", "user");
        userHome.AddFile(welcomeFile);
        
        var readmeFile = new VirtualFile("README.md", Root, 
            "# Virtuelles Betriebssystem\n\n" +
            "Dieses Projekt simuliert ein Betriebssystem innerhalb einer Anwendung.\n\n" +
            "## Komponenten\n\n" +
            "- Terminal\n" +
            "- Dateisystem\n" +
            "- Prozessverwaltung\n" +
            "- Texteditor\n" +
            "- Dateibrowser\n", "root");
        Root.AddFile(readmeFile);
    }
    
    /// <summary>
    /// Löst das FileSystemChanged-Event aus
    /// </summary>
    /// <param name="path">Pfad des Elements</param>
    /// <param name="changeType">Art der Änderung</param>
    /// <param name="elementType">Typ des Elements</param>
    private void FireFileSystemChanged(string path, FileSystemChangeType changeType, FileSystemElementType elementType)
    {
        FileSystemChanged?.Invoke(this, new FileSystemEventArgs(path, changeType, elementType));
    }
    
    /// <summary>
    /// Speichert das Dateisystem in einer Datei
    /// </summary>
    /// <param name="filePath">Pfad der Datei</param>
    public async Task SaveToFileAsync(string filePath)
    {
        // ToDo: Implementieren der Speicherfunktion
        // Hierzu müsste eine serialisierbare Kopie des Dateisystems erstellt werden
    }
    
    /// <summary>
    /// Lädt das Dateisystem aus einer Datei
    /// </summary>
    /// <param name="filePath">Pfad der Datei</param>
    public async Task LoadFromFileAsync(string filePath)
    {
        // ToDo: Implementieren der Ladefunktion
    }
}