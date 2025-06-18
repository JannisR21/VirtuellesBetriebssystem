using System;

namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Event-Args für Prozess-Status-Änderungen
/// </summary>
public class ProcessStatusEventArgs : EventArgs
{
    /// <summary>
    /// Der Prozess, dessen Status sich geändert hat
    /// </summary>
    public VirtualProcess Process { get; }
    
    /// <summary>
    /// Der vorherige Status
    /// </summary>
    public ProcessStatus OldStatus { get; }
    
    /// <summary>
    /// Der neue Status
    /// </summary>
    public ProcessStatus NewStatus { get; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="process">Der Prozess</param>
    /// <param name="oldStatus">Vorheriger Status</param>
    /// <param name="newStatus">Neuer Status</param>
    public ProcessStatusEventArgs(VirtualProcess process, ProcessStatus oldStatus, ProcessStatus newStatus)
    {
        Process = process;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}