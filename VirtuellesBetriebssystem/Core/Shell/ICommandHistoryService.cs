using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Interface für den Befehlshistorie-Dienst
/// </summary>
public interface ICommandHistoryService
{
    /// <summary>
    /// Fügt einen Befehl zur Historie hinzu
    /// </summary>
    /// <param name="command">Der hinzuzufügende Befehl</param>
    void AddCommand(string command);
    
    /// <summary>
    /// Gibt die komplette Befehlshistorie zurück
    /// </summary>
    /// <returns>Liste der Befehle in chronologischer Reihenfolge</returns>
    List<string> GetHistory();
    
    /// <summary>
    /// Gibt einen Befehl anhand seines Index in der Historie zurück
    /// </summary>
    /// <param name="index">Der Index des Befehls (1-basiert)</param>
    /// <returns>Der Befehl oder null, wenn nicht gefunden</returns>
    string GetCommandByIndex(int index);
    
    /// <summary>
    /// Sucht den letzten Befehl, der mit dem angegebenen Präfix beginnt
    /// </summary>
    /// <param name="prefix">Das Präfix, nach dem gesucht werden soll</param>
    /// <returns>Der Befehl oder null, wenn nicht gefunden</returns>
    string FindCommandByPrefix(string prefix);
    
    /// <summary>
    /// Löscht die gesamte Befehlshistorie
    /// </summary>
    void ClearHistory();
    
    /// <summary>
    /// Maximale Anzahl der Befehle, die in der Historie gespeichert werden
    /// </summary>
    int MaxHistorySize { get; set; }
}