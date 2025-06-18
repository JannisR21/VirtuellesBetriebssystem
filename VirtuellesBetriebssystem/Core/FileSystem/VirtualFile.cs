using System;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Repräsentiert eine virtuelle Datei im simulierten Dateisystem
/// </summary>
public class VirtualFile
{
    // Basisdaten
    public string Name { get; set; }
    public string FullPath { get; set; }
    public string Extension => System.IO.Path.GetExtension(Name);
    public VirtualDirectory Parent { get; set; }
    
    // Inhalt
    public string Content { get; set; }
    
    // Metadaten
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public string Owner { get; set; }
    public FileSystemPermissions Permissions { get; set; }
    public long Size => Content?.Length ?? 0;
    public bool IsReadOnly { get; set; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="name">Name der Datei</param>
    /// <param name="parent">Übergeordnetes Verzeichnis</param>
    /// <param name="content">Inhalt der Datei</param>
    /// <param name="owner">Besitzer der Datei</param>
    /// <param name="permissions">Berechtigungen der Datei</param>
    public VirtualFile(string name, VirtualDirectory parent, string content = "", string owner = "user", FileSystemPermissions permissions = FileSystemPermissions.OwnerReadWrite | FileSystemPermissions.GroupRead | FileSystemPermissions.OthersRead)
    {
        Name = name;
        Parent = parent;
        Content = content;
        
        // Pfad berechnen
        FullPath = parent != null 
            ? System.IO.Path.Combine(parent.FullPath, name).Replace("\\", "/") 
            : "/" + name;
        
        // Metadaten setzen
        var now = DateTime.Now;
        CreationTime = now;
        LastAccessTime = now;
        LastWriteTime = now;
        Owner = owner;
        Permissions = permissions;
        IsReadOnly = false;
    }
    
    /// <summary>
    /// Aktualisiert den Inhalt der Datei
    /// </summary>
    /// <param name="content">Neuer Inhalt</param>
    public void UpdateContent(string content)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("Die Datei ist schreibgeschützt.");
            
        Content = content;
        LastWriteTime = DateTime.Now;
    }
    
    /// <summary>
    /// Markiert die Datei als gelesen
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