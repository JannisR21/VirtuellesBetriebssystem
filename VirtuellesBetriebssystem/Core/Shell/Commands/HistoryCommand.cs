using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtuellesBetriebssystem.Core.Shell.Commands;

/// <summary>
/// Befehl zur Anzeige und Verwaltung der Befehlshistorie (history)
/// </summary>
public class HistoryCommand : CommandBase
{
    public override string Name => "history";
    public override string Description => "Zeigt die Befehlshistorie an oder führt Befehle aus der Historie aus";
    public override string Usage => "history [Option] [Nummer]";

    private readonly ICommandHistoryService _historyService;

    public HistoryCommand(ITerminalService terminalService, ICommandHistoryService historyService) 
        : base(terminalService)
    {
        _historyService = historyService;
    }

    public override string Execute(string[] arguments, string input = null)
    {
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        // Optionen für den Befehl
        bool clearHistory = options.ContainsKey("c") || options.ContainsKey("clear");
        
        // Historie löschen
        if (clearHistory)
        {
            _historyService.ClearHistory();
            return "Befehlshistorie gelöscht.";
        }
        
        // Ausführen eines Befehls aus der Historie (!n)
        if (filteredArgs.Length > 0 && filteredArgs[0].StartsWith("!"))
        {
            var argValue = filteredArgs[0].Substring(1);
            
            if (int.TryParse(argValue, out int commandIndex))
            {
                var command = _historyService.GetCommandByIndex(commandIndex);
                if (string.IsNullOrEmpty(command))
                {
                    return $"history: Kein Befehl mit Nummer {commandIndex} in der Historie gefunden.";
                }
                
                // Befehl zurückgeben und vom Terminal ausführen lassen
                return $"Führe Befehl aus: {command}\n" + TerminalService.ExecuteCommand(command);
            }
            else
            {
                // Suche nach letztem Befehl, der mit dem angegebenen Text beginnt
                var matchingCommand = _historyService.FindCommandByPrefix(argValue);
                if (string.IsNullOrEmpty(matchingCommand))
                {
                    return $"history: Kein Befehl mit Präfix '{argValue}' in der Historie gefunden.";
                }
                
                // Befehl zurückgeben und vom Terminal ausführen lassen
                return $"Führe Befehl aus: {matchingCommand}\n" + TerminalService.ExecuteCommand(matchingCommand);
            }
        }
        
        // Anzeigen der Historie (Standardverhalten)
        var history = _historyService.GetHistory();
        if (history.Count == 0)
        {
            return "Keine Befehle in der Historie.";
        }
        
        var sb = new StringBuilder();
        for (int i = 0; i < history.Count; i++)
        {
            sb.AppendLine($"{i + 1,4}  {history[i]}");
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Erstellen von Aliassen für Befehle (alias)
/// </summary>
public class AliasCommand : CommandBase
{
    public override string Name => "alias";
    public override string Description => "Definiert oder zeigt Aliasse für Befehle an";
    public override string Usage => "alias [name[=wert]]";

    private readonly IAliasService _aliasService;

    public AliasCommand(ITerminalService terminalService, IAliasService aliasService) 
        : base(terminalService)
    {
        _aliasService = aliasService;
    }

    public override string Execute(string[] arguments, string input = null)
    {
        // Wenn keine Argumente, alle Aliasse anzeigen
        if (arguments.Length == 0)
        {
            var aliases = _aliasService.GetAllAliases();
            if (aliases.Count == 0)
            {
                return "Keine Aliasse definiert.";
            }
            
            var sb = new StringBuilder();
            foreach (var alias in aliases)
            {
                sb.AppendLine($"alias {alias.Key}='{alias.Value}'");
            }
            
            return sb.ToString();
        }
        
        var filteredArgs = string.Join(" ", arguments);
        
        // Prüfen, ob ein Alias definiert wird (name=wert)
        if (filteredArgs.Contains("="))
        {
            var parts = filteredArgs.Split(new[] {'='}, 2);
            var name = parts[0].Trim();
            var value = parts[1].Trim();
            
            // Anführungszeichen entfernen, falls vorhanden
            if ((value.StartsWith("'") && value.EndsWith("'")) || 
                (value.StartsWith("\"") && value.EndsWith("\"")))
            {
                value = value.Substring(1, value.Length - 2);
            }
            
            _aliasService.SetAlias(name, value);
            return string.Empty; // Erfolg ohne Ausgabe
        }
        else
        {
            // Ansonsten einzelnen Alias anzeigen
            var name = filteredArgs.Trim();
            var value = _aliasService.GetAlias(name);
            
            if (string.IsNullOrEmpty(value))
            {
                return $"alias: {name}: nicht gefunden";
            }
            
            return $"alias {name}='{value}'";
        }
    }
}

/// <summary>
/// Befehl zum Entfernen von Aliassen (unalias)
/// </summary>
public class UnaliasCommand : CommandBase
{
    public override string Name => "unalias";
    public override string Description => "Entfernt Aliasse";
    public override string Usage => "unalias name | -a";

    private readonly IAliasService _aliasService;

    public UnaliasCommand(ITerminalService terminalService, IAliasService aliasService) 
        : base(terminalService)
    {
        _aliasService = aliasService;
    }

    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return "unalias: Fehlender Operand\nVerwendung: " + Usage;
        }
        
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        // Alle Aliasse entfernen
        if (options.ContainsKey("a"))
        {
            _aliasService.ClearAliases();
            return "Alle Aliasse entfernt.";
        }
        
        // Einzelne Aliasse entfernen
        var sb = new StringBuilder();
        foreach (var name in filteredArgs)
        {
            if (!_aliasService.RemoveAlias(name))
            {
                sb.AppendLine($"unalias: {name}: nicht gefunden");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Auflisten verfügbarer Befehle (compgen)
/// </summary>
public class CompgenCommand : CommandBase
{
    public override string Name => "compgen";
    public override string Description => "Generiert mögliche Vervollständigungen";
    public override string Usage => "compgen [Optionen] [Wort]";

    private readonly ICompletionService _completionService;

    public CompgenCommand(ITerminalService terminalService, ICompletionService completionService) 
        : base(terminalService)
    {
        _completionService = completionService;
    }

    public override string Execute(string[] arguments, string input = null)
    {
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        string prefix = filteredArgs.Length > 0 ? filteredArgs[0] : "";
        
        var sb = new StringBuilder();
        
        // Befehle auflisten
        if (options.ContainsKey("c"))
        {
            var commands = _completionService.GetCommandCompletions(prefix);
            foreach (var command in commands)
            {
                sb.AppendLine(command);
            }
        }
        // Dateien und Verzeichnisse auflisten
        else if (options.ContainsKey("f"))
        {
            var files = _completionService.GetFileCompletions(prefix);
            foreach (var file in files)
            {
                sb.AppendLine(file);
            }
        }
        // Verzeichnisse auflisten
        else if (options.ContainsKey("d"))
        {
            var dirs = _completionService.GetDirectoryCompletions(prefix);
            foreach (var dir in dirs)
            {
                sb.AppendLine(dir);
            }
        }
        // Alle Typen auflisten (Standard)
        else
        {
            var allCompletions = _completionService.GetCompletions(prefix);
            foreach (var completion in allCompletions)
            {
                sb.AppendLine(completion);
            }
        }
        
        return sb.ToString();
    }
}