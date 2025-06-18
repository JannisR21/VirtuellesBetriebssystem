using System;
using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Interface für das virtuelle Dateisystem
/// </summary>
public interface IVirtualFileSystem
{
    /// <summary>
    /// Erstellt eine neue Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <param name="content">Inhalt der Datei</param>
    /// <returns>Die erstellte Datei</returns>
    VirtualFile CreateFile(string path, string content = "");
    
    /// <summary>
    /// Erstellt ein neues Verzeichnis
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Das erstellte Verzeichnis</returns>
    VirtualDirectory CreateDirectory(string path);
    
    /// <summary>
    /// Löscht eine Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>True, wenn die Datei gelöscht wurde, False sonst</returns>
    bool DeleteFile(string path);
    
    /// <summary>
    /// Löscht ein Verzeichnis
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <param name="recursive">Ob rekursiv gelöscht werden soll</param>
    /// <returns>True, wenn das Verzeichnis gelöscht wurde, False sonst</returns>
    bool DeleteDirectory(string path, bool recursive = false);
    
    /// <summary>
    /// Prüft, ob eine Datei existiert
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>True, wenn die Datei existiert, False sonst</returns>
    bool FileExists(string path);
    
    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>True, wenn das Verzeichnis existiert, False sonst</returns>
    bool DirectoryExists(string path);
    
    /// <summary>
    /// Liest den Inhalt einer Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>Inhalt der Datei</returns>
    string ReadFile(string path);
    
    /// <summary>
    /// Schreibt Inhalt in eine Datei
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <param name="content">Zu schreibender Inhalt</param>
    void WriteFile(string path, string content);
    
    /// <summary>
    /// Verschiebt eine Datei
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn die Datei verschoben wurde, False sonst</returns>
    bool MoveFile(string sourcePath, string destinationPath);
    
    /// <summary>
    /// Verschiebt ein Verzeichnis
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn das Verzeichnis verschoben wurde, False sonst</returns>
    bool MoveDirectory(string sourcePath, string destinationPath);
    
    /// <summary>
    /// Kopiert eine Datei
    /// </summary>
    /// <param name="sourcePath">Quellpfad</param>
    /// <param name="destinationPath">Zielpfad</param>
    /// <returns>True, wenn die Datei kopiert wurde, False sonst</returns>
    bool CopyFile(string sourcePath, string destinationPath);
    
    /// <summary>
    /// Listet alle Dateien in einem Verzeichnis auf
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Liste der Dateien</returns>
    IEnumerable<VirtualFile> GetFiles(string path);
    
    /// <summary>
    /// Listet alle Verzeichnisse in einem Verzeichnis auf
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Liste der Verzeichnisse</returns>
    IEnumerable<VirtualDirectory> GetDirectories(string path);
    
    /// <summary>
    /// Holt eine Datei anhand ihres Pfades
    /// </summary>
    /// <param name="path">Pfad der Datei</param>
    /// <returns>Die Datei oder null, wenn nicht gefunden</returns>
    VirtualFile GetFile(string path);
    
    /// <summary>
    /// Holt ein Verzeichnis anhand seines Pfades
    /// </summary>
    /// <param name="path">Pfad des Verzeichnisses</param>
    /// <returns>Das Verzeichnis oder null, wenn nicht gefunden</returns>
    VirtualDirectory GetDirectory(string path);
    
    /// <summary>
    /// Normalisiert einen Pfad
    /// </summary>
    /// <param name="path">Der zu normalisierende Pfad</param>
    /// <returns>Der normalisierte Pfad</returns>
    string NormalizePath(string path);
    
    /// <summary>
    /// Kombiniert Pfadsegmente
    /// </summary>
    /// <param name="paths">Die zu kombinierenden Pfadsegmente</param>
    /// <returns>Der kombinierte Pfad</returns>
    string CombinePath(params string[] paths);
    
    /// <summary>
    /// Gibt das Root-Verzeichnis zurück
    /// </summary>
    VirtualDirectory Root { get; }
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich das Dateisystem ändert
    /// </summary>
    event EventHandler<FileSystemEventArgs> FileSystemChanged;
}