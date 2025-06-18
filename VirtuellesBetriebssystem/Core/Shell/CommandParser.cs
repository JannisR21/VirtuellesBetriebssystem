using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VirtuellesBetriebssystem.Core.Shell;

/// <summary>
/// Parser für Shell-Befehle
/// </summary>
public class CommandParser
{
    // Regex-Muster für das Parsen von Befehlen
    private static readonly Regex CommandRegex = new Regex(@"^([^\s|<>]+)(.*)$", RegexOptions.Compiled);
    private static readonly Regex ArgumentRegex = new Regex(@"(?:""([^""]*)""|'([^']*)'|([^\s]+))", RegexOptions.Compiled);
    private static readonly Regex PipeRegex = new Regex(@"\s*\|\s*", RegexOptions.Compiled);
    private static readonly Regex RedirectRegex = new Regex(@"\s*(>+|<)\s*(\S+)", RegexOptions.Compiled);
    
    /// <summary>
    /// Parst einen Befehl
    /// </summary>
    /// <param name="commandLine">Die zu parsende Befehlszeile</param>
    /// <returns>Das geparste Command-Objekt</returns>
    public Command ParseCommand(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return null;
            
        // Pipes verarbeiten
        var pipeCommands = PipeRegex.Split(commandLine).Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
        if (pipeCommands.Length > 1)
        {
            var commands = new List<Command>();
            foreach (var cmd in pipeCommands)
            {
                commands.Add(ParseSingleCommand(cmd));
            }
            
            // Verkettung von Befehlen durch Pipes
            for (int i = 0; i < commands.Count - 1; i++)
            {
                commands[i].OutputCommand = commands[i + 1];
            }
            
            return commands[0];
        }
        
        // Einzelnen Befehl verarbeiten
        return ParseSingleCommand(commandLine);
    }
    
    /// <summary>
    /// Parst einen einzelnen Befehl ohne Pipe
    /// </summary>
    /// <param name="commandLine">Die zu parsende Befehlszeile</param>
    /// <returns>Das geparste Command-Objekt</returns>
    private Command ParseSingleCommand(string commandLine)
    {
        // Umleitung extrahieren
        string outputFile = null;
        string inputFile = null;
        bool appendOutput = false;
        
        var redirectMatch = RedirectRegex.Match(commandLine);
        while (redirectMatch.Success)
        {
            var redirectType = redirectMatch.Groups[1].Value;
            var fileName = redirectMatch.Groups[2].Value;
            
            if (redirectType == ">")
            {
                outputFile = fileName;
                appendOutput = false;
            }
            else if (redirectType == ">>")
            {
                outputFile = fileName;
                appendOutput = true;
            }
            else if (redirectType == "<")
            {
                inputFile = fileName;
            }
            
            // Umleitung aus der Befehlszeile entfernen
            commandLine = commandLine.Remove(redirectMatch.Index, redirectMatch.Length);
            redirectMatch = RedirectRegex.Match(commandLine);
        }
        
        // Befehlsnamen und Argumente extrahieren
        var commandMatch = CommandRegex.Match(commandLine);
        if (!commandMatch.Success)
            return null;
            
        var commandName = commandMatch.Groups[1].Value.Trim();
        var argumentsText = commandMatch.Groups[2].Value.Trim();
        
        // Argumente parsen
        var arguments = new List<string>();
        var argMatches = ArgumentRegex.Matches(argumentsText);
        foreach (Match match in argMatches)
        {
            // Zitate entfernen
            string arg = match.Groups[1].Success ? match.Groups[1].Value :
                         match.Groups[2].Success ? match.Groups[2].Value :
                         match.Groups[3].Value;
            arguments.Add(arg);
        }
        
        return new Command
        {
            Name = commandName,
            Arguments = arguments.ToArray(),
            InputFile = inputFile,
            OutputFile = outputFile,
            AppendOutput = appendOutput
        };
    }
}