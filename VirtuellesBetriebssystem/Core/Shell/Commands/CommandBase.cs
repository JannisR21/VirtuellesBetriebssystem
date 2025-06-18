using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Shell.Commands;

/// <summary>
/// Basisklasse für alle Shell-Befehle
/// </summary>
public abstract class CommandBase
{
    /// <summary>
    /// Name des Befehls
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Beschreibung des Befehls für die Hilfe
    /// </summary>
    public abstract string Description { get; }
    
    /// <summary>
    /// Verwendung des Befehls für die Hilfe
    /// </summary>
    public abstract string Usage { get; }
    
    /// <summary>
    /// Referenz auf den Terminal-Dienst
    /// </summary>
    protected ITerminalService TerminalService { get; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="terminalService">Referenz auf den Terminal-Dienst</param>
    protected CommandBase(ITerminalService terminalService)
    {
        TerminalService = terminalService;
    }
    
    /// <summary>
    /// Führt den Befehl aus
    /// </summary>
    /// <param name="arguments">Argumente des Befehls</param>
    /// <param name="input">Eingabe des Befehls (für Pipes)</param>
    /// <returns>Ausgabe des Befehls</returns>
    public abstract string Execute(string[] arguments, string input = null);
    
    /// <summary>
    /// Gibt die Hilfe-Information für den Befehl zurück
    /// </summary>
    /// <returns>Hilfe-Text für den Befehl</returns>
    public virtual string GetHelp()
    {
        return $"{Name} - {Description}\n\nVerwendung: {Usage}";
    }
    
    /// <summary>
    /// Parst die Befehlsargumente nach Optionen
    /// </summary>
    /// <param name="arguments">Argumente des Befehls</param>
    /// <returns>Dictionary mit geparsten Optionen</returns>
    protected Dictionary<string, string> ParseOptions(string[] arguments)
    {
        var options = new Dictionary<string, string>();
        
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].StartsWith("-"))
            {
                string option = arguments[i].TrimStart('-');
                
                // Option mit Wert
                if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                {
                    options[option] = arguments[i + 1];
                    i++; // Skip next argument
                }
                else
                {
                    // Option ohne Wert
                    options[option] = "true";
                }
            }
        }
        
        return options;
    }
    
    /// <summary>
    /// Filtert die Argumente ohne Optionen
    /// </summary>
    /// <param name="arguments">Argumente des Befehls</param>
    /// <returns>Array mit gefilterten Argumenten</returns>
    protected string[] FilterArguments(string[] arguments)
    {
        var filteredArgs = new List<string>();
        
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].StartsWith("-"))
            {
                // Option mit Wert überspringen
                if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                {
                    i++;
                }
            }
            else
            {
                filteredArgs.Add(arguments[i]);
            }
        }
        
        return filteredArgs.ToArray();
    }
}