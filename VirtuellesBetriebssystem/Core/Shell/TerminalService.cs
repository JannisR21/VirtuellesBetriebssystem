using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtuellesBetriebssystem.Core.FileSystem;
using VirtuellesBetriebssystem.Core.Process;
using VirtuellesBetriebssystem.Core.Shell.Commands;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Implementation des Terminal-Dienstes
/// </summary>
public class TerminalService : ITerminalService
{
    private readonly CommandParser _parser;
    private readonly VirtualFileSystem _fileSystem;
    private readonly IProcessManager _processManager;
    private readonly ICommandHistoryService _historyService;
    private readonly IAliasService _aliasService;
    private readonly ICompletionService _completionService;
    
    // Befehle
    private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich das Arbeitsverzeichnis ändert
    /// </summary>
    public event EventHandler<string> WorkingDirectoryChanged;
    
    /// <summary>
    /// Gibt das aktuelle Arbeitsverzeichnis zurück
    /// </summary>
    public string CurrentDirectory => _fileSystem.CurrentDirectory.FullPath;
    
    /// <summary>
    /// Der aktuelle Benutzername
    /// </summary>
    public string CurrentUser { get; } = "user";
    
    /// <summary>
    /// Der Hostname des Systems
    /// </summary>
    public string Hostname { get; } = "sim-os";
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="fileSystem">Das virtuelle Dateisystem</param>
    /// <param name="processManager">Der Prozess-Manager</param>
    public TerminalService(VirtualFileSystem fileSystem, IProcessManager processManager)
    {
        _fileSystem = fileSystem;
        _processManager = processManager;
        _parser = new CommandParser();
        
        // Hilfsdienste initialisieren
        _historyService = new CommandHistoryService();
        _aliasService = new AliasService();
        
        // Einige Standard-Aliasse setzen
        _aliasService.SetAlias("ll", "ls -l");
        _aliasService.SetAlias("la", "ls -a");
        _aliasService.SetAlias("cls", "clear");
        _aliasService.SetAlias("clear", "echo -e \"\\033c\"");
        
        // CompletionService nach den anderen Diensten initialisieren,
        // da er Abhängigkeiten zu diesen hat
        _completionService = new CompletionService(this, fileSystem, _aliasService);
        
        // Befehle registrieren
        RegisterCommands();
    }
    
    /// <summary>
    /// Führt einen Befehl aus
    /// </summary>
    /// <param name="commandLine">Der auszuführende Befehl</param>
    /// <returns>Das Ergebnis des Befehls</returns>
    public string ExecuteCommand(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return string.Empty;
            
        // Befehl zur Historie hinzufügen
        _historyService.AddCommand(commandLine);
        
        // Befehl parsen
        var command = _parser.ParseCommand(commandLine);
        if (command == null)
            return "Ungültiger Befehl";
            
        // Prüfen, ob ein Alias für den Befehl existiert
        if (_aliasService.AliasExists(command.Name))
        {
            string aliasCommand = _aliasService.GetAlias(command.Name);
            
            // Argumente an Alias-Befehl anhängen, falls vorhanden
            if (command.Arguments.Length > 0)
            {
                aliasCommand += " " + string.Join(" ", command.Arguments);
            }
            
            // Aliaskette begrenzen, um Endlosschleifen zu vermeiden
            return ExecuteAliasCommand(aliasCommand, 5);
        }
            
        // Befehl ausführen
        return ExecuteCommandInternal(command);
    }
    
    /// <summary>
    /// Führt einen Alias-Befehl aus (mit Rekursionsschutz)
    /// </summary>
    /// <param name="aliasCommand">Der auszuführende Befehl</param>
    /// <param name="remainingAliasDepth">Verbleibende Alias-Auflösungstiefe</param>
    /// <returns>Das Ergebnis des Befehls</returns>
    private string ExecuteAliasCommand(string aliasCommand, int remainingAliasDepth)
    {
        if (remainingAliasDepth <= 0)
        {
            return "Fehler: Maximale Alias-Auflösungstiefe erreicht. Mögliche Endlosschleife in Alias-Definition.";
        }
        
        // Befehl parsen
        var command = _parser.ParseCommand(aliasCommand);
        if (command == null)
            return "Ungültiger Alias-Befehl";
            
        // Prüfen, ob ein weiterer Alias für den Befehl existiert
        if (_aliasService.AliasExists(command.Name))
        {
            string nextAliasCommand = _aliasService.GetAlias(command.Name);
            
            // Argumente an Alias-Befehl anhängen, falls vorhanden
            if (command.Arguments.Length > 0)
            {
                nextAliasCommand += " " + string.Join(" ", command.Arguments);
            }
            
            // Rekursiv den nächsten Alias auflösen
            return ExecuteAliasCommand(nextAliasCommand, remainingAliasDepth - 1);
        }
        
        // Befehl ausführen
        return ExecuteCommandInternal(command);
    }
    
    /// <summary>
    /// Führt einen geparsten Befehl aus
    /// </summary>
    /// <param name="command">Der auszuführende Befehl</param>
    /// <param name="input">Eingabe aus Pipe</param>
    /// <returns>Das Ergebnis des Befehls</returns>
    private string ExecuteCommandInternal(Command command, string input = null)
    {
        // Befehl in der Registry suchen
        if (!_commands.TryGetValue(command.Name.ToLower(), out var cmdHandler))
        {
            return $"{command.Name}: Befehl nicht gefunden";
        }
        
        // Befehl ausführen
        string output;
        try
        {
            output = cmdHandler.Execute(command.Arguments, input);
        }
        catch (Exception ex)
        {
            output = $"{command.Name}: Fehler: {ex.Message}";
        }
        
        // Eingabe aus Datei
        if (!string.IsNullOrEmpty(command.InputFile))
        {
            try
            {
                string filePath = command.InputFile;
                if (!filePath.StartsWith("/"))
                {
                    filePath = _fileSystem.CombinePath(CurrentDirectory, filePath);
                }
                
                if (_fileSystem.FileExists(filePath))
                {
                    input = _fileSystem.ReadFile(filePath);
                    output = ExecuteCommandInternal(command, input);
                }
                else
                {
                    output = $"{command.Name}: {command.InputFile}: Datei oder Verzeichnis nicht gefunden";
                }
            }
            catch (Exception ex)
            {
                output = $"{command.Name}: Fehler beim Lesen von {command.InputFile}: {ex.Message}";
            }
        }
        
        // Output in Datei umleiten
        if (!string.IsNullOrEmpty(command.OutputFile))
        {
            try
            {
                string filePath = command.OutputFile;
                if (!filePath.StartsWith("/"))
                {
                    filePath = _fileSystem.CombinePath(CurrentDirectory, filePath);
                }
                
                if (command.AppendOutput && _fileSystem.FileExists(filePath))
                {
                    string existingContent = _fileSystem.ReadFile(filePath);
                    _fileSystem.WriteFile(filePath, existingContent + output);
                }
                else
                {
                    _fileSystem.WriteFile(filePath, output);
                }
                
                // Leere Ausgabe zurückgeben, da in Datei umgeleitet
                output = string.Empty;
            }
            catch (Exception ex)
            {
                output = $"{command.Name}: Fehler beim Schreiben in {command.OutputFile}: {ex.Message}";
            }
        }
        
        // Output an nächsten Befehl in der Pipe weiterleiten
        if (command.OutputCommand != null)
        {
            return ExecuteCommandInternal(command.OutputCommand, output);
        }
        
        return output;
    }
    
    /// <summary>
    /// Gibt den aktuellen Prompt zurück
    /// </summary>
    /// <returns>Der aktuelle Prompt</returns>
    public string GetPrompt()
    {
        return $"{CurrentUser}@{Hostname}:{GetPromptPath()}$ ";
    }
    
    /// <summary>
    /// Gibt eine Liste der verfügbaren Befehle zurück
    /// </summary>
    /// <returns>Liste der Befehle</returns>
    public string[] GetAvailableCommands()
    {
        // Befehle und Aliasse kombinieren
        var commands = _commands.Keys.ToList();
        commands.AddRange(_aliasService.GetAllAliases().Keys);
        return commands.Distinct().ToArray();
    }
    
    /// <summary>
    /// Liefert die Hilfe-Information für einen Befehl
    /// </summary>
    /// <param name="command">Der Befehl, für den Hilfe angezeigt werden soll</param>
    /// <returns>Hilfe-Text für den Befehl</returns>
    public string GetCommandHelp(string command)
    {
        // Prüfen, ob es ein Alias ist
        if (_aliasService.AliasExists(command))
        {
            string aliasTarget = _aliasService.GetAlias(command);
            return $"alias {command}='{aliasTarget}'";
        }
        
        // Normaler Befehl
        if (_commands.TryGetValue(command.ToLower(), out var cmdHandler))
        {
            return cmdHandler.GetHelp();
        }
        
        return $"{command}: Befehl nicht gefunden";
    }
    
    /// <summary>
    /// Führt eine Autovervollständigung für eine Befehlszeile durch
    /// </summary>
    /// <param name="commandLine">Die zu vervollständigende Befehlszeile</param>
    /// <param name="cursorPosition">Die Position des Cursors</param>
    /// <returns>Das Vervollständigungsergebnis</returns>
    public CompletionResult CompleteCommandLine(string commandLine, int cursorPosition)
    {
        return _completionService.CompleteCommandLine(commandLine, cursorPosition);
    }
    
    /// <summary>
    /// Gibt den Befehlshistorie-Dienst zurück
    /// </summary>
    public ICommandHistoryService HistoryService => _historyService;
    
    /// <summary>
    /// Gibt den Alias-Dienst zurück
    /// </summary>
    public IAliasService AliasService => _aliasService;
    
    /// <summary>
    /// Registriert alle verfügbaren Befehle
    /// </summary>
    private void RegisterCommands()
    {
        // Datei-Befehle
        RegisterCommand(new LsCommand(this, _fileSystem));
        RegisterCommand(new CdCommand(this, _fileSystem));
        RegisterCommand(new MkdirCommand(this, _fileSystem));
        RegisterCommand(new RmCommand(this, _fileSystem));
        RegisterCommand(new TouchCommand(this, _fileSystem));
        RegisterCommand(new CatCommand(this, _fileSystem));
        RegisterCommand(new EchoCommand(this));
        
        // System-Befehle
        RegisterCommand(new HelpCommand(this));
        RegisterCommand(new PwdCommand(this));
        RegisterCommand(new PsCommand(this, _processManager));
        RegisterCommand(new DateCommand(this));
        RegisterCommand(new WhoamiCommand(this));
        RegisterCommand(new HostnameCommand(this));
        RegisterCommand(new GrepCommand(this));
        
        // Neue Befehle
        RegisterCommand(new HistoryCommand(this, _historyService));
        RegisterCommand(new AliasCommand(this, _aliasService));
        RegisterCommand(new UnaliasCommand(this, _aliasService));
        RegisterCommand(new CompgenCommand(this, _completionService));
    }
    
    /// <summary>
    /// Registriert einen Befehl
    /// </summary>
    /// <param name="command">Der zu registrierende Befehl</param>
    private void RegisterCommand(CommandBase command)
    {
        _commands[command.Name.ToLower()] = command;
    }
    
    /// <summary>
    /// Gibt den Pfad für den Prompt zurück (mit ~ für Home-Verzeichnis)
    /// </summary>
    /// <returns>Der formatierte Pfad</returns>
    private string GetPromptPath()
    {
        string path = CurrentDirectory;
        string homePath = $"/home/{CurrentUser}";
        
        if (path.StartsWith(homePath))
        {
            path = "~" + path.Substring(homePath.Length);
            if (string.IsNullOrEmpty(path))
                path = "~";
        }
        
        return path;
    }
}