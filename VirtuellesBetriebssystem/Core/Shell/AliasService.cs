using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Implementierung des Alias-Dienstes
/// </summary>
public class AliasService : IAliasService
{
    private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();
    
    /// <summary>
    /// Legt einen Befehlsalias fest
    /// </summary>
    /// <param name="name">Name des Alias</param>
    /// <param name="command">Der Befehl, auf den der Alias verweist</param>
    public void SetAlias(string name, string command)
    {
        _aliases[name] = command;
    }
    
    /// <summary>
    /// Gibt den Befehl für einen Alias zurück
    /// </summary>
    /// <param name="name">Name des Alias</param>
    /// <returns>Der hinterlegte Befehl oder null, wenn der Alias nicht existiert</returns>
    public string GetAlias(string name)
    {
        if (_aliases.TryGetValue(name, out var command))
        {
            return command;
        }
        
        return null;
    }
    
    /// <summary>
    /// Entfernt einen Alias
    /// </summary>
    /// <param name="name">Name des zu entfernenden Alias</param>
    /// <returns>True, wenn der Alias entfernt wurde, sonst False</returns>
    public bool RemoveAlias(string name)
    {
        return _aliases.Remove(name);
    }
    
    /// <summary>
    /// Prüft, ob ein Alias existiert
    /// </summary>
    /// <param name="name">Name des zu prüfenden Alias</param>
    /// <returns>True, wenn der Alias existiert, sonst False</returns>
    public bool AliasExists(string name)
    {
        return _aliases.ContainsKey(name);
    }
    
    /// <summary>
    /// Gibt alle definierten Aliasse zurück
    /// </summary>
    /// <returns>Dictionary mit allen Aliassen (Name -> Befehl)</returns>
    public Dictionary<string, string> GetAllAliases()
    {
        return new Dictionary<string, string>(_aliases);
    }
    
    /// <summary>
    /// Entfernt alle Aliasse
    /// </summary>
    public void ClearAliases()
    {
        _aliases.Clear();
    }
}