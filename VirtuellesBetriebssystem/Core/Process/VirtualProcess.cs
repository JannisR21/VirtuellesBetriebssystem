using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Repräsentiert einen virtuellen Prozess im simulierten Betriebssystem
/// </summary>
public class VirtualProcess
{
    // Basisdaten
    public string ProcessId { get; }
    public string Name { get; }
    public ProcessStatus Status { get; private set; }
    public DateTime StartTime { get; }
    public string Owner { get; }
    
    // Performance-Daten (simuliert)
    public int CpuUsage { get; private set; }
    public int MemoryUsage { get; private set; }
    public long ElapsedMilliseconds => (long)_stopwatch.Elapsed.TotalMilliseconds;
    
    // Interne Daten
    private readonly Stopwatch _stopwatch;
    private readonly Random _random = new Random();
    private readonly List<VirtualThread> _threads = new List<VirtualThread>();
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="processId">ID des Prozesses</param>
    /// <param name="name">Name des Prozesses</param>
    /// <param name="owner">Besitzer des Prozesses</param>
    public VirtualProcess(string processId, string name, string owner = "user")
    {
        ProcessId = processId;
        Name = name;
        Status = ProcessStatus.Running;
        StartTime = DateTime.Now;
        Owner = owner;
        
        // Startwerte setzen
        CpuUsage = _random.Next(5, 20);
        MemoryUsage = _random.Next(50, 200);
        
        // Zeitmessung starten
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        
        // Hauptthread erstellen
        AddThread("main");
    }
    
    /// <summary>
    /// Beendet den Prozess
    /// </summary>
    public void Terminate()
    {
        Status = ProcessStatus.Terminated;
        _stopwatch.Stop();
        
        // Alle Threads beenden
        foreach (var thread in _threads)
        {
            thread.Terminate();
        }
    }
    
    /// <summary>
    /// Pausiert den Prozess
    /// </summary>
    public void Suspend()
    {
        if (Status == ProcessStatus.Running)
        {
            Status = ProcessStatus.Suspended;
            _stopwatch.Stop();
            
            // Alle Threads pausieren
            foreach (var thread in _threads)
            {
                thread.Suspend();
            }
        }
    }
    
    /// <summary>
    /// Setzt den Prozess fort
    /// </summary>
    public void Resume()
    {
        if (Status == ProcessStatus.Suspended)
        {
            Status = ProcessStatus.Running;
            _stopwatch.Start();
            
            // Alle Threads fortsetzen
            foreach (var thread in _threads)
            {
                thread.Resume();
            }
        }
    }
    
    /// <summary>
    /// Aktualisiert die simulierten Leistungsdaten
    /// </summary>
    public void UpdateStats()
    {
        if (Status == ProcessStatus.Running)
        {
            // Zufällige Schwankungen für CPU und RAM
            CpuUsage = Math.Min(100, Math.Max(1, CpuUsage + _random.Next(-5, 6)));
            MemoryUsage = Math.Min(1000, Math.Max(10, MemoryUsage + _random.Next(-10, 11)));
        }
    }
    
    /// <summary>
    /// Fügt dem Prozess einen Thread hinzu
    /// </summary>
    /// <param name="name">Name des Threads</param>
    /// <returns>Der erstellte Thread</returns>
    public VirtualThread AddThread(string name)
    {
        var thread = new VirtualThread(this, $"{ProcessId}-{_threads.Count + 1}", name);
        _threads.Add(thread);
        return thread;
    }
    
    /// <summary>
    /// Gibt alle Threads des Prozesses zurück
    /// </summary>
    /// <returns>Liste der Threads</returns>
    public IEnumerable<VirtualThread> GetThreads()
    {
        return _threads;
    }
    
    /// <summary>
    /// Gibt die Anzahl der Threads zurück
    /// </summary>
    public int ThreadCount => _threads.Count;
}