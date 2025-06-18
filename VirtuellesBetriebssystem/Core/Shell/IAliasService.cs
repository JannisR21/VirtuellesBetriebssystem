using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Interface für den Alias-Dienst
/// </summary>
public interface IAliasService
{
    /// <summary>
    /// Legt einen Befehlsalias fest
    /// </summary>
    /// <param name="name">Name des Alias</param>
    /// <param name="command">Der Befehl, auf den der Alias verweist</param>
    void SetAlias(string name, string command);
    
    /// <summary>
    /// Gibt den Befehl für einen Alias zurück
    /// </summary>
    /// <param name="name">Name des Alias</param>
    /// <returns>Der hinterlegte Befehl oder null, wenn der Alias nicht existiert</returns>
    string GetAlias(string name);
    
    /// <summary>
    /// Entfernt einen Alias
    /// </summary>
    /// <param name="name">Name des zu entfernenden Alias</param>
    /// <returns>True, wenn der Alias entfernt wurde, sonst False</returns>
    bool RemoveAlias(string name);
    
    /// <summary>
    /// Prüft, ob ein Alias existiert
    /// </summary>
    /// <param name="name">Name des zu prüfenden Alias</param>
    /// <returns>True, wenn der Alias existiert, sonst False</returns>
    bool AliasExists(string name);
    
    /// <summary>
    /// Gibt alle definierten Aliasse zurück
    /// </summary>
    /// <returns>Dictionary mit allen Aliassen (Name -> Befehl)</returns>
    Dictionary<string, string> GetAllAliases();
    
    /// <summary>
    /// Entfernt alle Aliasse
    /// </summary>
    void ClearAliases();
}