using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Repräsentiert ein virtuelles Verzeichnis im simulierten Dateisystem
/// </summary>
public class VirtualDirectory
{
    // Basisdaten
    public string Name { get; set; }
    public string FullPath { get; set; }
    public VirtualDirectory Parent { get; set; }
    
    // Inhalte
    private readonly List<VirtualFile> _files = new List<VirtualFile>();
    private readonly List<VirtualDirectory> _subdirectories = new List<VirtualDirectory>();
    
    // Metadaten
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public string Owner { get; set; }
    public FileSystemPermissions Permissions { get; set; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="name">Name des Verzeichnisses</param>
    /// <param name="parent">Übergeordnetes Verzeichnis</param>
    /// <param name="owner">Besitzer des Verzeichnisses</param>
    /// <param name="permissions">Berechtigungen des Verzeichnisses</param>
    public VirtualDirectory(string name, VirtualDirectory parent = null, string owner = "user", FileSystemPermissions permissions = FileSystemPermissions.OwnerAll | FileSystemPermissions.GroupReadExecute | FileSystemPermissions.OthersReadExecute)
    {
        Name = name;
        Parent = parent;
        
        // Pfad berechnen
        if (parent == null)
        {
            FullPath = name == "/" ? "/" : "/" + name;
        }
        else
        {
            FullPath = System.IO.Path.Combine(parent.FullPath, name).Replace("\\", "/");
        }
        
        // Metadaten setzen
        var now = DateTime.Now;
        CreationTime = now;
        LastAccessTime = now;
        LastWriteTime = now;
        Owner = owner;
        Permissions = permissions;
    }
    
    /// <summary>
    /// Fügt eine Datei hinzu
    /// </summary>
    /// <param name="file">Die hinzuzufügende Datei</param>
    public void AddFile(VirtualFile file)
    {
        if (_files.Any(f => f.Name == file.Name))
            throw new InvalidOperationException($"Eine Datei mit dem Namen '{file.Name}' existiert bereits.");
            
        _files.Add(file);
        LastWriteTime = DateTime.Now;
    }
    
    /// <summary>
    /// Entfernt eine Datei
    /// </summary>
    /// <param name="fileName">Name der zu entfernenden Datei</param>
    /// <returns>True, wenn die Datei entfernt wurde, False sonst</returns>
    public bool RemoveFile(string fileName)
    {
        var file = _files.FirstOrDefault(f => f.Name == fileName);
        if (file != null)
        {
            _files.Remove(file);
            LastWriteTime = DateTime.Now;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Fügt ein Unterverzeichnis hinzu
    /// </summary>
    /// <param name="directory">Das hinzuzufügende Verzeichnis</param>
    public void AddSubdirectory(VirtualDirectory directory)
    {
        if (_subdirectories.Any(d => d.Name == directory.Name))
            throw new InvalidOperationException($"Ein Verzeichnis mit dem Namen '{directory.Name}' existiert bereits.");
            
        _subdirectories.Add(directory);
        LastWriteTime = DateTime.Now;
    }
    
    /// <summary>
    /// Entfernt ein Unterverzeichnis
    /// </summary>
    /// <param name="directoryName">Name des zu entfernenden Verzeichnisses</param>
    /// <returns>True, wenn das Verzeichnis entfernt wurde, False sonst</returns>
    public bool RemoveSubdirectory(string directoryName)
    {
        var directory = _subdirectories.FirstOrDefault(d => d.Name == directoryName);
        if (directory != null)
        {
            _subdirectories.Remove(directory);
            LastWriteTime = DateTime.Now;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Holt eine Datei nach Namen
    /// </summary>
    /// <param name="fileName">Name der Datei</param>
    /// <returns>Die Datei oder null, wenn nicht gefunden</returns>
    public VirtualFile GetFile(string fileName)
    {
        return _files.FirstOrDefault(f => f.Name == fileName);
    }
    
    /// <summary>
    /// Holt ein Unterverzeichnis nach Namen
    /// </summary>
    /// <param name="directoryName">Name des Verzeichnisses</param>
    /// <returns>Das Verzeichnis oder null, wenn nicht gefunden</returns>
    public VirtualDirectory GetSubdirectory(string directoryName)
    {
        return _subdirectories.FirstOrDefault(d => d.Name == directoryName);
    }
    
    /// <summary>
    /// Gibt alle Dateien zurück
    /// </summary>
    /// <returns>Liste der Dateien</returns>
    public IEnumerable<VirtualFile> GetFiles()
    {
        MarkAsAccessed();
        return _files;
    }
    
    /// <summary>
    /// Gibt alle Unterverzeichnisse zurück
    /// </summary>
    /// <returns>Liste der Unterverzeichnisse</returns>
    public IEnumerable<VirtualDirectory> GetSubdirectories()
    {
        MarkAsAccessed();
        return _subdirectories;
    }
    
    /// <summary>
    /// Markiert das Verzeichnis als gelesen
    /// </summary>
    public void MarkAsAccessed()
    {
        LastAccessTime = DateTime.Now;
    }
    
    /// <summary>
    /// Prüft, ob ein Benutzer Leserechte hat
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <returns>True, wenn der Benutzer Leserechte hat, False sonst</returns>
    public bool CanRead(string username)
    {
        if (username == Owner)
            return (Permissions & FileSystemPermissions.OwnerRead) != 0;
            
        // Einfache Implementierung, ohne Gruppen
        return (Permissions & FileSystemPermissions.OthersRead) != 0;
    }
    
    /// <summary>
    /// Prüft, ob ein Benutzer Schreibrechte hat
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <returns>True, wenn der Benutzer Schreibrechte hat, False sonst</returns>
    public bool CanWrite(string username)
    {
        if (username == Owner)
            return (Permissions & FileSystemPermissions.OwnerWrite) != 0;
            
        // Einfache Implementierung, ohne Gruppen
        return (Permissions & FileSystemPermissions.OthersWrite) != 0;
    }
    
    /// <summary>
    /// Prüft, ob ein Benutzer Ausführrechte hat
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <returns>True, wenn der Benutzer Ausführrechte hat, False sonst</returns>
    public bool CanExecute(string username)
    {
        if (username == Owner)
            return (Permissions & FileSystemPermissions.OwnerExecute) != 0;
            
        // Einfache Implementierung, ohne Gruppen
        return (Permissions & FileSystemPermissions.OthersExecute) != 0;
    }
}