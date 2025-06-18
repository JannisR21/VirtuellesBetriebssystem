using System.Collections.Generic;
using System.Linq;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Implementierung des Befehlshistorie-Dienstes
/// </summary>
public class CommandHistoryService : ICommandHistoryService
{
    private readonly List<string> _history = new List<string>();
    
    /// <summary>
    /// Maximale Anzahl der Befehle in der Historie
    /// </summary>
    public int MaxHistorySize { get; set; } = 100;
    
    /// <summary>
    /// Fügt einen Befehl zur Historie hinzu
    /// </summary>
    /// <param name="command">Der hinzuzufügende Befehl</param>
    public void AddCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;
        
        // Wenn der letzte Befehl identisch ist, nicht erneut hinzufügen
        if (_history.Count > 0 && _history[_history.Count - 1] == command)
            return;
        
        _history.Add(command);
        
        // Bei Überschreitung der maximalen Größe alte Einträge entfernen
        if (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Gibt die komplette Befehlshistorie zurück
    /// </summary>
    /// <returns>Liste der Befehle in chronologischer Reihenfolge</returns>
    public List<string> GetHistory()
    {
        return new List<string>(_history);
    }
    
    /// <summary>
    /// Gibt einen Befehl anhand seines Index in der Historie zurück
    /// </summary>
    /// <param name="index">Der Index des Befehls (1-basiert)</param>
    /// <returns>Der Befehl oder null, wenn nicht gefunden</returns>
    public string GetCommandByIndex(int index)
    {
        // Index ist 1-basiert in der Ausgabe, aber 0-basiert in der Liste
        int listIndex = index - 1;
        
        if (listIndex < 0 || listIndex >= _history.Count)
            return null;
            
        return _history[listIndex];
    }
    
    /// <summary>
    /// Sucht den letzten Befehl, der mit dem angegebenen Präfix beginnt
    /// </summary>
    /// <param name="prefix">Das Präfix, nach dem gesucht werden soll</param>
    /// <returns>Der Befehl oder null, wenn nicht gefunden</returns>
    public string FindCommandByPrefix(string prefix)
    {
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (_history[i].StartsWith(prefix))
            {
                return _history[i];
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Löscht die gesamte Befehlshistorie
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }
}