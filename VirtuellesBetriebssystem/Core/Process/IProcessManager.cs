using System;
using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Interface für den Prozess-Manager
/// </summary>
public interface IProcessManager
{
    /// <summary>
    /// Startet einen neuen Prozess
    /// </summary>
    /// <param name="processName">Name des Prozesses</param>
    /// <param name="owner">Besitzer des Prozesses</param>
    /// <returns>ID des gestarteten Prozesses</returns>
    string StartProcess(string processName, string owner = "user");
    
    /// <summary>
    /// Beendet einen Prozess
    /// </summary>
    /// <param name="processId">ID des zu beendenden Prozesses</param>
    /// <returns>True, wenn der Prozess beendet wurde, False sonst</returns>
    bool EndProcess(string processId);
    
    /// <summary>
    /// Holt eine Liste aller laufenden Prozesse
    /// </summary>
    /// <returns>Liste der Prozesse</returns>
    IEnumerable<VirtualProcess> GetProcesses();
    
    /// <summary>
    /// Holt einen Prozess anhand seiner ID
    /// </summary>
    /// <param name="processId">ID des Prozesses</param>
    /// <returns>Der Prozess oder null, wenn nicht gefunden</returns>
    VirtualProcess GetProcess(string processId);
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich der Prozess-Status ändert
    /// </summary>
    event EventHandler<ProcessStatusEventArgs> ProcessStatusChanged;
}