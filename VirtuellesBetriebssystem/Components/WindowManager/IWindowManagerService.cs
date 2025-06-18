using System.Windows;

namespace VirtuellesBetriebssystem.Components.WindowManager;

/// <summary>
/// Interface für den Fenstermanager-Dienst
/// </summary>
public interface IWindowManagerService
{
    /// <summary>
    /// Erstellt ein neues App-Fenster
    /// </summary>
    /// <param name="title">Titel des Fensters</param>
    /// <param name="content">Inhalt des Fensters</param>
    /// <param name="processId">ID des zugehörigen Prozesses</param>
    /// <returns>Das erstellte Fenster</returns>
    AppWindow CreateWindow(string title, UIElement content, string processId);
    
    /// <summary>
    /// Schließt ein Fenster
    /// </summary>
    /// <param name="window">Das zu schließende Fenster</param>
    void CloseWindow(AppWindow window);
    
    /// <summary>
    /// Fokussiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu fokussierende Fenster</param>
    void FocusWindow(AppWindow window);
    
    /// <summary>
    /// Minimiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu minimierende Fenster</param>
    void MinimizeWindow(AppWindow window);
    
    /// <summary>
    /// Maximiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu maximierende Fenster</param>
    void MaximizeWindow(AppWindow window);
    
    /// <summary>
    /// Stellt ein Fenster wieder her
    /// </summary>
    /// <param name="window">Das wiederherzustellende Fenster</param>
    void RestoreWindow(AppWindow window);
}