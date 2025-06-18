namespace VirtuellesBetriebssystem.Core.Process;

/// <summary>
/// Status eines Prozesses oder Threads
/// </summary>
public enum ProcessStatus
{
    /// <summary>
    /// Der Prozess oder Thread wird ausgeführt
    /// </summary>
    Running,
    
    /// <summary>
    /// Der Prozess oder Thread wurde angehalten
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Der Prozess oder Thread wurde beendet
    /// </summary>
    Terminated
}