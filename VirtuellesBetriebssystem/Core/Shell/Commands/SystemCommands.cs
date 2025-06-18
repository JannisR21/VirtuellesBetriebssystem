using System;
using System.Linq;
using System.Text;
using VirtuellesBetriebssystem.Core.Process;

namespace VirtuellesBetriebssystem.Core.Shell.Commands;

/// <summary>
/// Befehl zum Anzeigen der Hilfe (help)
/// </summary>
public class HelpCommand : CommandBase
{
    public override string Name => "help";
    public override string Description => "Zeigt Hilfe zu Befehlen an";
    public override string Usage => "help [Befehl]";
    
    public HelpCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Verfügbare Befehle:");
            
            var commands = TerminalService.GetAvailableCommands();
            foreach (var command in commands.OrderBy(c => c))
            {
                var helpText = TerminalService.GetCommandHelp(command);
                var description = helpText.Split('\n').FirstOrDefault();
                
                if (!string.IsNullOrEmpty(description) && description.Contains(" - "))
                {
                    description = description.Split(new[] { " - " }, StringSplitOptions.None)[1];
                }
                else
                {
                    description = "(keine Beschreibung)";
                }
                
                sb.AppendLine($"  {command,-10} - {description}");
            }
            
            sb.AppendLine();
            sb.AppendLine("Geben Sie 'help Befehl' ein, um detaillierte Hilfe zu einem Befehl zu erhalten.");
            
            return sb.ToString();
        }
        else
        {
            var command = arguments[0];
            return TerminalService.GetCommandHelp(command);
        }
    }
}

/// <summary>
/// Befehl zum Anzeigen des aktuellen Verzeichnisses (pwd)
/// </summary>
public class PwdCommand : CommandBase
{
    public override string Name => "pwd";
    public override string Description => "Zeigt das aktuelle Arbeitsverzeichnis an";
    public override string Usage => "pwd";
    
    public PwdCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        return TerminalService.CurrentDirectory;
    }
}

/// <summary>
/// Befehl zum Auflisten von Prozessen (ps)
/// </summary>
public class PsCommand : CommandBase
{
    public override string Name => "ps";
    public override string Description => "Zeigt laufende Prozesse an";
    public override string Usage => "ps [Optionen]";
    
    private readonly IProcessManager _processManager;
    
    public PsCommand(ITerminalService terminalService, IProcessManager processManager) 
        : base(terminalService)
    {
        _processManager = processManager;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        var options = ParseOptions(arguments);
        bool showDetailed = options.ContainsKey("l") || options.ContainsKey("f");
        
        var sb = new StringBuilder();
        
        if (showDetailed)
        {
            sb.AppendLine("  PID  User     Status    Started          CPU%   Memory   Threads  Name");
            sb.AppendLine("-----------------------------------------------------------------------");
        }
        else
        {
            sb.AppendLine("  PID  Status    CPU%   Name");
            sb.AppendLine("---------------------------");
        }
        
        foreach (var process in _processManager.GetProcesses())
        {
            if (showDetailed)
            {
                sb.AppendLine($"{process.ProcessId,6}  {process.Owner,-8} {process.Status,-9} " +
                              $"{process.StartTime:HH:mm:ss}  {process.CpuUsage,5}%  {process.MemoryUsage,7}K  " +
                              $"{process.ThreadCount,7}  {process.Name}");
            }
            else
            {
                sb.AppendLine($"{process.ProcessId,6}  {process.Status,-9} {process.CpuUsage,5}%  {process.Name}");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Anzeigen der aktuellen Zeit (date)
/// </summary>
public class DateCommand : CommandBase
{
    public override string Name => "date";
    public override string Description => "Zeigt das aktuelle Datum und die Uhrzeit an";
    public override string Usage => "date [Format]";
    
    public DateCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return DateTime.Now.ToString("ddd MMM d HH:mm:ss yyyy");
        }
        else
        {
            try
            {
                return DateTime.Now.ToString(arguments[0]);
            }
            catch (Exception)
            {
                return $"date: Ungültiges Format '{arguments[0]}'";
            }
        }
    }
}

/// <summary>
/// Befehl zum Anzeigen des Benutzers (whoami)
/// </summary>
public class WhoamiCommand : CommandBase
{
    public override string Name => "whoami";
    public override string Description => "Zeigt den aktuellen Benutzernamen an";
    public override string Usage => "whoami";
    
    public WhoamiCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        return TerminalService.CurrentUser;
    }
}

/// <summary>
/// Befehl zum Anzeigen des Hostnamens (hostname)
/// </summary>
public class HostnameCommand : CommandBase
{
    public override string Name => "hostname";
    public override string Description => "Zeigt den Hostnamen an";
    public override string Usage => "hostname";
    
    public HostnameCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        return TerminalService.Hostname;
    }
}

/// <summary>
/// Befehl zum Filtern von Text (grep)
/// </summary>
public class GrepCommand : CommandBase
{
    public override string Name => "grep";
    public override string Description => "Sucht nach Mustern in Texten";
    public override string Usage => "grep [Optionen] Muster [Datei...]";
    
    public GrepCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return "grep: Fehlender Muster-Operand\nVerwendung: " + Usage;
        }
        
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        bool ignoreCase = options.ContainsKey("i");
        bool showLineNumbers = options.ContainsKey("n");
        
        string pattern = filteredArgs[0];
        var textToSearch = string.Empty;
        
        // Text aus der Pipe oder von Dateien
        if (!string.IsNullOrEmpty(input))
        {
            textToSearch = input;
        }
        else if (filteredArgs.Length > 1)
        {
            // ToDo: Hier müssten Dateien gelesen werden
            return "grep: Dateisuche noch nicht implementiert. Verwenden Sie Pipes.";
        }
        else
        {
            return "grep: Keine Eingabedaten\nVerwendung: " + Usage;
        }
        
        var sb = new StringBuilder();
        var lines = textToSearch.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].IndexOf(pattern, comparisonType) >= 0)
            {
                if (showLineNumbers)
                {
                    sb.AppendLine($"{i + 1}:{lines[i]}");
                }
                else
                {
                    sb.AppendLine(lines[i]);
                }
            }
        }
        
        return sb.ToString();
    }
}