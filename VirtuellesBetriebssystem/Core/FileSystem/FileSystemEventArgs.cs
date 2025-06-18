using System;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Event-Args für Dateisystem-Änderungen
/// </summary>
public class FileSystemEventArgs : EventArgs
{
    /// <summary>
    /// Pfad der betroffenen Datei oder des Verzeichnisses
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Art der Änderung
    /// </summary>
    public FileSystemChangeType ChangeType { get; }
    
    /// <summary>
    /// Typ des betroffenen Elements (Datei oder Verzeichnis)
    /// </summary>
    public FileSystemElementType ElementType { get; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="path">Pfad des Elements</param>
    /// <param name="changeType">Art der Änderung</param>
    /// <param name="elementType">Typ des Elements</param>
    public FileSystemEventArgs(string path, FileSystemChangeType changeType, FileSystemElementType elementType)
    {
        Path = path;
        ChangeType = changeType;
        ElementType = elementType;
    }
}

/// <summary>
/// Art der Änderung im Dateisystem
/// </summary>
public enum FileSystemChangeType
{
    /// <summary>
    /// Element wurde erstellt
    /// </summary>
    Created,
    
    /// <summary>
    /// Element wurde gelöscht
    /// </summary>
    Deleted,
    
    /// <summary>
    /// Element wurde geändert
    /// </summary>
    Modified,
    
    /// <summary>
    /// Element wurde verschoben
    /// </summary>
    Moved
}

/// <summary>
/// Typ des Elements im Dateisystem
/// </summary>
public enum FileSystemElementType
{
    /// <summary>
    /// Element ist eine Datei
    /// </summary>
    File,
    
    /// <summary>
    /// Element ist ein Verzeichnis
    /// </summary>
    Directory
}