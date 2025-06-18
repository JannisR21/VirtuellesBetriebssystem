using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Ergebnis einer Autovervollständigungs-Operation
/// </summary>
public class CompletionResult
{
    /// <summary>
    /// Die vervollständigte Befehlszeile
    /// </summary>
    public string CompletedCommandLine { get; set; }
    
    /// <summary>
    /// Die neue Cursorposition in der vervollständigten Befehlszeile
    /// </summary>
    public int NewCursorPosition { get; set; }
    
    /// <summary>
    /// Liste von Vervollständigungsoptionen, wenn mehrere Möglichkeiten existieren
    /// </summary>
    public List<string> Options { get; set; } = new List<string>();
    
    /// <summary>
    /// Gibt an, ob mehrere Vervollständigungsoptionen verfügbar sind
    /// </summary>
    public bool HasMultipleOptions => Options.Count > 1;
    
    /// <summary>
    /// Gibt an, ob eine eindeutige Vervollständigung gefunden wurde
    /// </summary>
    public bool IsUnique => Options.Count == 1;
}