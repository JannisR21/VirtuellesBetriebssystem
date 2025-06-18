using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VirtuellesBetriebssystem.Core.FileSystem;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Implementierung des Autovervollständigungs-Dienstes
/// </summary>
public class CompletionService : ICompletionService
{
    private readonly ITerminalService _terminalService;
    private readonly IVirtualFileSystem _fileSystem;
    private readonly IAliasService _aliasService;
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="terminalService">Der Terminal-Dienst</param>
    /// <param name="fileSystem">Das virtuelle Dateisystem</param>
    /// <param name="aliasService">Der Alias-Dienst</param>
    public CompletionService(
        ITerminalService terminalService, 
        IVirtualFileSystem fileSystem,
        IAliasService aliasService)
    {
        _terminalService = terminalService;
        _fileSystem = fileSystem;
        _aliasService = aliasService;
    }
    
    /// <summary>
    /// Liefert Vervollständigungen für den angegebenen Befehlspräfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Vervollständigungen</returns>
    public List<string> GetCompletions(string prefix)
    {
        var result = new List<string>();
        
        // Befehle vervollständigen
        result.AddRange(GetCommandCompletions(prefix));
        
        // Dateien und Verzeichnisse vervollständigen
        result.AddRange(GetFileCompletions(prefix));
        
        return result.Distinct().OrderBy(s => s).ToList();
    }
    
    /// <summary>
    /// Liefert Vervollständigungen für Befehle mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Befehlsvervollständigungen</returns>
    public List<string> GetCommandCompletions(string prefix)
    {
        // Verfügbare Befehle abrufen
        var commands = _terminalService.GetAvailableCommands();
        
        // Aliasse abrufen
        var aliases = _aliasService.GetAllAliases().Keys;
        
        // Alle Befehle und Aliasse zusammenführen
        var allCommands = commands.Concat(aliases).Distinct();
        
        // Nach Präfix filtern
        return allCommands
            .Where(cmd => cmd.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(cmd => cmd)
            .ToList();
    }
    
    /// <summary>
    /// Liefert Vervollständigungen für Dateien mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Dateivervollständigungen</returns>
    public List<string> GetFileCompletions(string prefix)
    {
        var result = new List<string>();
        
        // Pfad aus Präfix extrahieren
        string basePath = _terminalService.CurrentDirectory;
        string searchPattern = prefix;
        
        if (prefix.Contains("/"))
        {
            // Absoluter oder relativer Pfad angegeben
            int lastSlashIndex = prefix.LastIndexOf('/');
            string pathPart = prefix.Substring(0, lastSlashIndex);
            searchPattern = prefix.Substring(lastSlashIndex + 1);
            
            if (pathPart.StartsWith("/"))
            {
                // Absoluter Pfad
                basePath = pathPart;
            }
            else
            {
                // Relativer Pfad
                basePath = _fileSystem.CombinePath(_terminalService.CurrentDirectory, pathPart);
            }
        }
        
        try
        {
            // Dateien im Verzeichnis suchen
            if (_fileSystem.DirectoryExists(basePath))
            {
                var files = _fileSystem.GetFiles(basePath);
                foreach (var file in files)
                {
                    if (file.Name.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prefix.Contains("/"))
                        {
                            // Pfadpräfix beibehalten
                            int lastSlashIndex = prefix.LastIndexOf('/');
                            string pathPart = prefix.Substring(0, lastSlashIndex + 1);
                            result.Add(pathPart + file.Name);
                        }
                        else
                        {
                            result.Add(file.Name);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Fehler ignorieren
        }
        
        return result;
    }
    
    /// <summary>
    /// Liefert Vervollständigungen für Verzeichnisse mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Verzeichnisvervollständigungen</returns>
    public List<string> GetDirectoryCompletions(string prefix)
    {
        var result = new List<string>();
        
        // Pfad aus Präfix extrahieren
        string basePath = _terminalService.CurrentDirectory;
        string searchPattern = prefix;
        
        if (prefix.Contains("/"))
        {
            // Absoluter oder relativer Pfad angegeben
            int lastSlashIndex = prefix.LastIndexOf('/');
            string pathPart = prefix.Substring(0, lastSlashIndex);
            searchPattern = prefix.Substring(lastSlashIndex + 1);
            
            if (pathPart.StartsWith("/"))
            {
                // Absoluter Pfad
                basePath = pathPart;
            }
            else
            {
                // Relativer Pfad
                basePath = _fileSystem.CombinePath(_terminalService.CurrentDirectory, pathPart);
            }
        }
        
        try
        {
            // Verzeichnisse im Verzeichnis suchen
            if (_fileSystem.DirectoryExists(basePath))
            {
                var directories = _fileSystem.GetDirectories(basePath);
                foreach (var dir in directories)
                {
                    if (dir.Name.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prefix.Contains("/"))
                        {
                            // Pfadpräfix beibehalten
                            int lastSlashIndex = prefix.LastIndexOf('/');
                            string pathPart = prefix.Substring(0, lastSlashIndex + 1);
                            result.Add(pathPart + dir.Name + "/");
                        }
                        else
                        {
                            result.Add(dir.Name + "/");
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Fehler ignorieren
        }
        
        return result;
    }
    
    /// <summary>
    /// Vervollständigt eine Befehlszeile an der angegebenen Cursorposition
    /// </summary>
    /// <param name="commandLine">Die aktuelle Befehlszeile</param>
    /// <param name="cursorPosition">Die Position des Cursors in der Befehlszeile</param>
    /// <returns>Informationen zur Vervollständigung (Vervollständigte Zeile und neue Cursorposition)</returns>
    public CompletionResult CompleteCommandLine(string commandLine, int cursorPosition)
    {
        var result = new CompletionResult
        {
            CompletedCommandLine = commandLine,
            NewCursorPosition = cursorPosition
        };
        
        if (string.IsNullOrEmpty(commandLine))
            return result;
            
        // Wort an der Cursorposition ermitteln
        var (wordStart, word) = GetWordAtCursor(commandLine, cursorPosition);
        
        if (string.IsNullOrEmpty(word))
            return result;
            
        // Prüfen, ob es das erste Wort ist (Befehl) oder ein Argument
        bool isFirstWord = IsFirstWord(commandLine, wordStart);
        
        List<string> completions;
        if (isFirstWord)
        {
            // Befehle vervollständigen
            completions = GetCommandCompletions(word);
        }
        else
        {
            // Argumente (Dateien und Verzeichnisse) vervollständigen
            completions = new List<string>();
            completions.AddRange(GetDirectoryCompletions(word));
            completions.AddRange(GetFileCompletions(word));
        }
        
        if (completions.Count == 0)
        {
            // Keine Vervollständigungen gefunden
            return result;
        }
        
        result.Options = completions;
        
        if (completions.Count == 1)
        {
            // Eindeutige Vervollständigung
            string completion = completions[0];
            string newCommandLine = commandLine.Substring(0, wordStart) + completion;
            
            // Rest der Zeile hinzufügen, falls vorhanden und der Cursor nicht am Ende steht
            if (cursorPosition < commandLine.Length)
            {
                newCommandLine += commandLine.Substring(cursorPosition);
            }
            
            // Bei Verzeichnissen ein Leerzeichen anhängen, oder wenn das Wort mit einem Leerzeichen endet
            if (completion.EndsWith("/") || (isFirstWord && !completion.EndsWith(" ")))
            {
                // Kein Leerzeichen für Verzeichnisse, damit weitere Pfadangaben möglich sind
            }
            else
            {
                newCommandLine += " ";
            }
            
            result.CompletedCommandLine = newCommandLine;
            result.NewCursorPosition = wordStart + completion.Length + (completion.EndsWith("/") ? 0 : 1);
        }
        
        return result;
    }
    
    /// <summary>
    /// Ermittelt das Wort an der Cursorposition
    /// </summary>
    /// <param name="commandLine">Die Befehlszeile</param>
    /// <param name="cursorPosition">Die Position des Cursors</param>
    /// <returns>Tuple mit Start-Index des Wortes und dem Wort selbst</returns>
    private (int, string) GetWordAtCursor(string commandLine, int cursorPosition)
    {
        // Sonderzeichen für Wortbegrenzung
        var delimiterChars = new[] { ' ', '\t' };
        
        // Anfang des Wortes suchen
        int wordStart = cursorPosition;
        while (wordStart > 0 && !delimiterChars.Contains(commandLine[wordStart - 1]))
        {
            wordStart--;
        }
        
        // Ende des Wortes begrenzen (falls Cursor mitten im Wort)
        int wordEnd = cursorPosition;
        
        // Wort extrahieren
        string word = commandLine.Substring(wordStart, cursorPosition - wordStart);
        
        return (wordStart, word);
    }
    
    /// <summary>
    /// Prüft, ob ein Wort das erste Wort in der Befehlszeile ist
    /// </summary>
    /// <param name="commandLine">Die Befehlszeile</param>
    /// <param name="wordStart">Der Start-Index des Wortes</param>
    /// <returns>True, wenn es das erste Wort ist, sonst False</returns>
    private bool IsFirstWord(string commandLine, int wordStart)
    {
        // Prüfen, ob vor dem Wort nur Leerzeichen stehen
        for (int i = 0; i < wordStart; i++)
        {
            if (commandLine[i] != ' ' && commandLine[i] != '\t')
            {
                return false;
            }
        }
        
        return true;
    }
}