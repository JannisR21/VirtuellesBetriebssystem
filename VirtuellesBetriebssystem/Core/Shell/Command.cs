namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Repräsentiert einen geparsten Shell-Befehl
/// </summary>
public class Command
{
    /// <summary>
    /// Name des Befehls
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Argumente des Befehls
    /// </summary>
    public string[] Arguments { get; set; }
    
    /// <summary>
    /// Eingabedatei für Umleitung
    /// </summary>
    public string InputFile { get; set; }
    
    /// <summary>
    /// Ausgabedatei für Umleitung
    /// </summary>
    public string OutputFile { get; set; }
    
    /// <summary>
    /// Ob die Ausgabe an die Datei angehängt werden soll
    /// </summary>
    public bool AppendOutput { get; set; }
    
    /// <summary>
    /// Befehl, an den die Ausgabe weitergeleitet wird (für Pipes)
    /// </summary>
    public Command OutputCommand { get; set; }
}