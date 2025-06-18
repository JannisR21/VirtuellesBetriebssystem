using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Implementation des Prozess-Managers
/// </summary>
public class ProcessManager : IProcessManager
{
    private readonly Dictionary<string, VirtualProcess> _processes = new Dictionary<string, VirtualProcess>();
    private readonly DispatcherTimer _statsUpdateTimer;
    private int _nextProcessId = 1;
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich der Prozess-Status ändert
    /// </summary>
    public event EventHandler<ProcessStatusEventArgs> ProcessStatusChanged;
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    public ProcessManager()
    {
        // Timer für die Aktualisierung der Leistungsdaten
        _statsUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _statsUpdateTimer.Tick += StatsUpdateTimer_Tick;
        _statsUpdateTimer.Start();
        
        // System-Prozesse erstellen
        StartProcess("System");
        StartProcess("Dateisystem");
        StartProcess("Benutzerservice");
    }
    
    /// <summary>
    /// Startet einen neuen Prozess
    /// </summary>
    /// <param name="processName">Name des Prozesses</param>
    /// <param name="owner">Besitzer des Prozesses</param>
    /// <returns>ID des gestarteten Prozesses</returns>
    public string StartProcess(string processName, string owner = "user")
    {
        string processId = GenerateProcessId();
        var process = new VirtualProcess(processId, processName, owner);
        _processes[processId] = process;
        
        // Event auslösen
        ProcessStatusChanged?.Invoke(this, new ProcessStatusEventArgs(process, ProcessStatus.Terminated, ProcessStatus.Running));
        
        return processId;
    }
    
    /// <summary>
    /// Beendet einen Prozess
    /// </summary>
    /// <param name="processId">ID des zu beendenden Prozesses</param>
    /// <returns>True, wenn der Prozess beendet wurde, False sonst</returns>
    public bool EndProcess(string processId)
    {
        if (_processes.TryGetValue(processId, out var process))
        {
            var oldStatus = process.Status;
            process.Terminate();
            
            // Event auslösen
            ProcessStatusChanged?.Invoke(this, new ProcessStatusEventArgs(process, oldStatus, ProcessStatus.Terminated));
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Holt eine Liste aller laufenden Prozesse
    /// </summary>
    /// <returns>Liste der Prozesse</returns>
    public IEnumerable<VirtualProcess> GetProcesses()
    {
        return _processes.Values.ToList();
    }
    
    /// <summary>
    /// Holt einen Prozess anhand seiner ID
    /// </summary>
    /// <param name="processId">ID des Prozesses</param>
    /// <returns>Der Prozess oder null, wenn nicht gefunden</returns>
    public VirtualProcess GetProcess(string processId)
    {
        _processes.TryGetValue(processId, out var process);
        return process;
    }
    
    /// <summary>
    /// Generiert eine neue Prozess-ID
    /// </summary>
    /// <returns>Die generierte ID</returns>
    private string GenerateProcessId()
    {
        return $"PID-{Interlocked.Increment(ref _nextProcessId):D4}";
    }
    
    /// <summary>
    /// Aktualisiert die Leistungsdaten aller Prozesse
    /// </summary>
    private void StatsUpdateTimer_Tick(object sender, EventArgs e)
    {
        foreach (var process in _processes.Values)
        {
            if (process.Status == ProcessStatus.Running)
            {
                process.UpdateStats();
                
                // Thread-Statistiken aktualisieren
                foreach (var thread in process.GetThreads())
                {
                    thread.UpdateStats();
                }
            }
        }
    }
}