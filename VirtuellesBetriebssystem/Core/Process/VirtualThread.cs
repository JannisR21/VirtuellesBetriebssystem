using System;
using System.Diagnostics;

namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Repräsentiert einen virtuellen Thread im simulierten Betriebssystem
/// </summary>
public class VirtualThread
{
    // Basisdaten
    public string ThreadId { get; }
    public string Name { get; }
    public VirtualProcess ParentProcess { get; }
    public ProcessStatus Status { get; private set; }
    public DateTime StartTime { get; }
    
    // Performance-Daten (simuliert)
    public int CpuUsage { get; private set; }
    public long ElapsedMilliseconds => (long)_stopwatch.Elapsed.TotalMilliseconds;
    
    // Interne Daten
    private readonly Stopwatch _stopwatch;
    private readonly Random _random = new Random();
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="parentProcess">Der Elternprozess</param>
    /// <param name="threadId">ID des Threads</param>
    /// <param name="name">Name des Threads</param>
    public VirtualThread(VirtualProcess parentProcess, string threadId, string name)
    {
        ParentProcess = parentProcess;
        ThreadId = threadId;
        Name = name;
        Status = ProcessStatus.Running;
        StartTime = DateTime.Now;
        
        // Startwerte setzen
        CpuUsage = _random.Next(1, 10);
        
        // Zeitmessung starten
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }
    
    /// <summary>
    /// Beendet den Thread
    /// </summary>
    public void Terminate()
    {
        Status = ProcessStatus.Terminated;
        _stopwatch.Stop();
    }
    
    /// <summary>
    /// Pausiert den Thread
    /// </summary>
    public void Suspend()
    {
        if (Status == ProcessStatus.Running)
        {
            Status = ProcessStatus.Suspended;
            _stopwatch.Stop();
        }
    }
    
    /// <summary>
    /// Setzt den Thread fort
    /// </summary>
    public void Resume()
    {
        if (Status == ProcessStatus.Suspended)
        {
            Status = ProcessStatus.Running;
            _stopwatch.Start();
        }
    }
    
    /// <summary>
    /// Aktualisiert die simulierten Leistungsdaten
    /// </summary>
    public void UpdateStats()
    {
        if (Status == ProcessStatus.Running)
        {
            // Zufällige Schwankungen für CPU
            CpuUsage = Math.Min(100, Math.Max(1, CpuUsage + _random.Next(-3, 4)));
        }
    }
}