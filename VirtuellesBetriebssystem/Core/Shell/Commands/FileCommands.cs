using System;
using System.Linq;
using System.Text;
using VirtuellesBetriebssystem.Core.FileSystem;

namespace VirtuellesBetriebssystem.Core.Shell.Commands;

/// <summary>
/// Befehl zum Auflisten von Dateien und Verzeichnissen (ls)
/// </summary>
public class LsCommand : CommandBase
{
    public override string Name => "ls";
    public override string Description => "Listet Dateien und Verzeichnisse auf";
    public override string Usage => "ls [Optionen] [Verzeichnis]";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public LsCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        bool showAll = options.ContainsKey("a") || options.ContainsKey("all");
        bool showDetails = options.ContainsKey("l");
        
        string path = filteredArgs.Length > 0 ? filteredArgs[0] : TerminalService.CurrentDirectory;
        
        if (!_fileSystem.DirectoryExists(path))
        {
            return $"ls: Verzeichnis '{path}' existiert nicht.";
        }
        
        var sb = new StringBuilder();
        
        try
        {
            var files = _fileSystem.GetFiles(path);
            var directories = _fileSystem.GetDirectories(path);
            
            if (showDetails)
            {
                // Detaillierte Auflistung
                sb.AppendLine("Berechtigung  Besitzer   Größe     Datum               Name");
                sb.AppendLine("------------------------------------------------------------------");
                
                foreach (var dir in directories)
                {
                    if (!showAll && dir.Name.StartsWith("."))
                        continue;
                        
                    sb.AppendLine($"drwxr-xr-x   {dir.Owner,-8}  {"<DIR>",-8}  {dir.LastWriteTime:yyyy-MM-dd HH:mm}  {dir.Name}/");
                }
                
                foreach (var file in files)
                {
                    if (!showAll && file.Name.StartsWith("."))
                        continue;
                        
                    sb.AppendLine($"-rw-r--r--   {file.Owner,-8}  {file.Size,-8}  {file.LastWriteTime:yyyy-MM-dd HH:mm}  {file.Name}");
                }
            }
            else
            {
                // Einfache Auflistung
                foreach (var dir in directories)
                {
                    if (!showAll && dir.Name.StartsWith("."))
                        continue;
                        
                    sb.Append($"{dir.Name}/  ");
                }
                
                foreach (var file in files)
                {
                    if (!showAll && file.Name.StartsWith("."))
                        continue;
                        
                    sb.Append($"{file.Name}  ");
                }
            }
        }
        catch (Exception ex)
        {
            return $"ls: Fehler: {ex.Message}";
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Wechseln des Verzeichnisses (cd)
/// </summary>
public class CdCommand : CommandBase
{
    public override string Name => "cd";
    public override string Description => "Wechselt das aktuelle Verzeichnis";
    public override string Usage => "cd [Verzeichnis]";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public CdCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        string path;
        
        if (arguments.Length == 0 || arguments[0] == "~")
        {
            // Wechseln zum Home-Verzeichnis
            path = $"/home/{TerminalService.CurrentUser}";
        }
        else
        {
            path = arguments[0];
        }
        
        try
        {
            // Pfad auflösen
            if (!path.StartsWith("/"))
            {
                path = _fileSystem.CombinePath(TerminalService.CurrentDirectory, path);
            }
            
            path = _fileSystem.NormalizePath(path);
            
            if (!_fileSystem.DirectoryExists(path))
            {
                return $"cd: Verzeichnis '{path}' existiert nicht.";
            }
            
            // Wechseln zum Verzeichnis im Dateisystem
            if (!(_fileSystem as VirtualFileSystem).ChangeDirectory(path))
            {
                return $"cd: Zugriff auf '{path}' nicht möglich.";
            }
            
            // TerminalService über den Verzeichniswechsel informieren
            // (wird im Terminal-Dienst implementiert)
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            return $"cd: Fehler: {ex.Message}";
        }
    }
}

/// <summary>
/// Befehl zum Erstellen von Verzeichnissen (mkdir)
/// </summary>
public class MkdirCommand : CommandBase
{
    public override string Name => "mkdir";
    public override string Description => "Erstellt Verzeichnisse";
    public override string Usage => "mkdir Verzeichnis...";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public MkdirCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return "mkdir: Fehlender Operand\nVerwendung: " + Usage;
        }
        
        var sb = new StringBuilder();
        
        foreach (var path in arguments)
        {
            try
            {
                string fullPath = path;
                if (!path.StartsWith("/"))
                {
                    fullPath = _fileSystem.CombinePath(TerminalService.CurrentDirectory, path);
                }
                
                _fileSystem.CreateDirectory(fullPath);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"mkdir: Kann Verzeichnis '{path}' nicht anlegen: {ex.Message}");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Löschen von Dateien (rm)
/// </summary>
public class RmCommand : CommandBase
{
    public override string Name => "rm";
    public override string Description => "Löscht Dateien oder Verzeichnisse";
    public override string Usage => "rm [Optionen] Datei...";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public RmCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return "rm: Fehlender Operand\nVerwendung: " + Usage;
        }
        
        var options = ParseOptions(arguments);
        var filteredArgs = FilterArguments(arguments);
        
        bool recursive = options.ContainsKey("r") || options.ContainsKey("R") || options.ContainsKey("recursive");
        bool force = options.ContainsKey("f") || options.ContainsKey("force");
        
        var sb = new StringBuilder();
        
        foreach (var path in filteredArgs)
        {
            try
            {
                string fullPath = path;
                if (!path.StartsWith("/"))
                {
                    fullPath = _fileSystem.CombinePath(TerminalService.CurrentDirectory, path);
                }
                
                if (_fileSystem.FileExists(fullPath))
                {
                    if (!_fileSystem.DeleteFile(fullPath) && !force)
                    {
                        sb.AppendLine($"rm: Kann Datei '{path}' nicht entfernen.");
                    }
                }
                else if (_fileSystem.DirectoryExists(fullPath))
                {
                    if (recursive)
                    {
                        if (!_fileSystem.DeleteDirectory(fullPath, true) && !force)
                        {
                            sb.AppendLine($"rm: Kann Verzeichnis '{path}' nicht entfernen.");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"rm: Kann '{path}' nicht entfernen: Ist ein Verzeichnis");
                    }
                }
                else if (!force)
                {
                    sb.AppendLine($"rm: Kann '{path}' nicht entfernen: Datei oder Verzeichnis nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                if (!force)
                {
                    sb.AppendLine($"rm: Fehler beim Entfernen von '{path}': {ex.Message}");
                }
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Erstellen einer leeren Datei (touch)
/// </summary>
public class TouchCommand : CommandBase
{
    public override string Name => "touch";
    public override string Description => "Erstellt leere Dateien oder aktualisiert Datei-Zeitstempel";
    public override string Usage => "touch Datei...";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public TouchCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0)
        {
            return "touch: Fehlender Operand\nVerwendung: " + Usage;
        }
        
        var sb = new StringBuilder();
        
        foreach (var path in arguments)
        {
            try
            {
                string fullPath = path;
                if (!path.StartsWith("/"))
                {
                    fullPath = _fileSystem.CombinePath(TerminalService.CurrentDirectory, path);
                }
                
                if (!_fileSystem.FileExists(fullPath))
                {
                    _fileSystem.CreateFile(fullPath, "");
                }
                else
                {
                    // Beim existierenden File nur Zeitstempel aktualisieren
                    // (wird automatisch beim Lesen gemacht)
                    var file = _fileSystem.GetFile(fullPath);
                    if (file != null)
                    {
                        file.MarkAsAccessed();
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"touch: Kann '{path}' nicht berühren: {ex.Message}");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Anzeigen von Dateiinhalten (cat)
/// </summary>
public class CatCommand : CommandBase
{
    public override string Name => "cat";
    public override string Description => "Zeigt den Inhalt von Dateien an";
    public override string Usage => "cat Datei...";
    
    private readonly IVirtualFileSystem _fileSystem;
    
    public CatCommand(ITerminalService terminalService, IVirtualFileSystem fileSystem) 
        : base(terminalService)
    {
        _fileSystem = fileSystem;
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        if (arguments.Length == 0 && input == null)
        {
            return "cat: Fehlender Operand\nVerwendung: " + Usage;
        }
        
        var sb = new StringBuilder();
        
        // Wenn Input vorhanden (z.B. von Pipe), diesen zuerst ausgeben
        if (!string.IsNullOrEmpty(input))
        {
            sb.Append(input);
        }
        
        foreach (var path in arguments)
        {
            try
            {
                string fullPath = path;
                if (!path.StartsWith("/"))
                {
                    fullPath = _fileSystem.CombinePath(TerminalService.CurrentDirectory, path);
                }
                
                if (!_fileSystem.FileExists(fullPath))
                {
                    sb.AppendLine($"cat: {path}: Datei oder Verzeichnis nicht gefunden");
                    continue;
                }
                
                string content = _fileSystem.ReadFile(fullPath);
                sb.Append(content);
                
                // Zeilenumbruch hinzufügen, wenn die Datei nicht mit einem endet
                if (!content.EndsWith("\n"))
                {
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"cat: Fehler beim Lesen von '{path}': {ex.Message}");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Befehl zum Schreiben in Dateien (echo)
/// </summary>
public class EchoCommand : CommandBase
{
    public override string Name => "echo";
    public override string Description => "Gibt Text aus";
    public override string Usage => "echo [Text]...";
    
    public EchoCommand(ITerminalService terminalService) 
        : base(terminalService)
    {
    }
    
    public override string Execute(string[] arguments, string input = null)
    {
        // Wenn Input vorhanden (z.B. von Pipe), diesen ausgeben
        if (!string.IsNullOrEmpty(input))
        {
            return input;
        }
        
        return string.Join(" ", arguments);
    }
}