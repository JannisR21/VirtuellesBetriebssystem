using System;

namespace VirtuellesBetriebssystem.Core.FileSystem;

/// <summary>
/// Berechtigungen für Dateien und Verzeichnisse im Dateisystem
/// </summary>
[Flags]
public enum FileSystemPermissions
{
    None = 0,
    
    // Besitzer-Berechtigungen
    OwnerRead = 1 << 0,
    OwnerWrite = 1 << 1,
    OwnerExecute = 1 << 2,
    OwnerAll = OwnerRead | OwnerWrite | OwnerExecute,
    OwnerReadWrite = OwnerRead | OwnerWrite,
    
    // Gruppen-Berechtigungen
    GroupRead = 1 << 3,
    GroupWrite = 1 << 4,
    GroupExecute = 1 << 5,
    GroupAll = GroupRead | GroupWrite | GroupExecute,
    GroupReadWrite = GroupRead | GroupWrite,
    
    // Andere-Berechtigungen
    OthersRead = 1 << 6,
    OthersWrite = 1 << 7,
    OthersExecute = 1 << 8,
    OthersAll = OthersRead | OthersWrite | OthersExecute,
    OthersReadWrite = OthersRead | OthersWrite,
    
    // Kombinierte Berechtigungen
    ReadOnly = OwnerRead | GroupRead | OthersRead,
    ReadWrite = ReadOnly | OwnerWrite,
    ExecuteOnly = OwnerExecute | GroupExecute | OthersExecute,
    GroupReadExecute = GroupRead | GroupExecute,
    OthersReadExecute = OthersRead | OthersExecute,
    
    // Vollständige Berechtigungen
    All = OwnerAll | GroupAll | OthersAll
}