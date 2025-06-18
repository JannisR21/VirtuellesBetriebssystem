using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Interface für den Autovervollständigungs-Dienst
/// </summary>
public interface ICompletionService
{
    /// <summary>
    /// Liefert Vervollständigungen für den angegebenen Befehlspräfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Vervollständigungen</returns>
    List<string> GetCompletions(string prefix);
    
    /// <summary>
    /// Liefert Vervollständigungen für Befehle mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Befehlsvervollständigungen</returns>
    List<string> GetCommandCompletions(string prefix);
    
    /// <summary>
    /// Liefert Vervollständigungen für Dateien mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Dateivervollständigungen</returns>
    List<string> GetFileCompletions(string prefix);
    
    /// <summary>
    /// Liefert Vervollständigungen für Verzeichnisse mit dem angegebenen Präfix
    /// </summary>
    /// <param name="prefix">Präfix, das vervollständigt werden soll</param>
    /// <returns>Liste möglicher Verzeichnisvervollständigungen</returns>
    List<string> GetDirectoryCompletions(string prefix);
    
    /// <summary>
    /// Vervollständigt eine Befehlszeile an der angegebenen Cursorposition
    /// </summary>
    /// <param name="commandLine">Die aktuelle Befehlszeile</param>
    /// <param name="cursorPosition">Die Position des Cursors in der Befehlszeile</param>
    /// <returns>Informationen zur Vervollständigung (Vervollständigte Zeile und neue Cursorposition)</returns>
    CompletionResult CompleteCommandLine(string commandLine, int cursorPosition);
}