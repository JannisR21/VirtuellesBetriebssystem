using System;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Interface für den Terminal-Dienst
/// </summary>
public interface ITerminalService
{
    /// <summary>
    /// Führt einen Befehl aus
    /// </summary>
    /// <param name="command">Der auszuführende Befehl</param>
    /// <returns>Das Ergebnis des Befehls</returns>
    string ExecuteCommand(string command);
    
    /// <summary>
    /// Gibt den aktuellen Prompt zurück
    /// </summary>
    /// <returns>Der aktuelle Prompt</returns>
    string GetPrompt();
    
    /// <summary>
    /// Gibt eine Liste der verfügbaren Befehle zurück
    /// </summary>
    /// <returns>Liste der Befehle</returns>
    string[] GetAvailableCommands();
    
    /// <summary>
    /// Liefert die Hilfe-Information für einen Befehl
    /// </summary>
    /// <param name="command">Der Befehl, für den Hilfe angezeigt werden soll</param>
    /// <returns>Hilfe-Text für den Befehl</returns>
    string GetCommandHelp(string command);
    
    /// <summary>
    /// Führt eine Autovervollständigung für eine Befehlszeile durch
    /// </summary>
    /// <param name="commandLine">Die zu vervollständigende Befehlszeile</param>
    /// <param name="cursorPosition">Die Position des Cursors</param>
    /// <returns>Das Vervollständigungsergebnis</returns>
    CompletionResult CompleteCommandLine(string commandLine, int cursorPosition);
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich der Arbeitsverzeichnispfad ändert
    /// </summary>
    event EventHandler<string> WorkingDirectoryChanged;
    
    /// <summary>
    /// Gibt das aktuelle Arbeitsverzeichnis zurück
    /// </summary>
    string CurrentDirectory { get; }
    
    /// <summary>
    /// Der aktuelle Benutzername
    /// </summary>
    string CurrentUser { get; }
    
    /// <summary>
    /// Der Hostname des Systems
    /// </summary>
    string Hostname { get; }
    
    /// <summary>
    /// Gibt den Befehlshistorie-Dienst zurück
    /// </summary>
    ICommandHistoryService HistoryService { get; }
    
    /// <summary>
    /// Gibt den Alias-Dienst zurück
    /// </summary>
    IAliasService AliasService { get; }
}