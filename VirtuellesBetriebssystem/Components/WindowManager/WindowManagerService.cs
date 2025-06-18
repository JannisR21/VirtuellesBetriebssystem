using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VirtuellesBetriebssystem.Components.WindowManager;

/// <summary>
/// Implementation des Fenstermanager-Dienstes
/// </summary>
public class WindowManagerService : IWindowManagerService
{
    private readonly Canvas _desktopCanvas;
    private readonly List<AppWindow> _windows = new List<AppWindow>();
    private readonly Random _random = new Random();

    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="desktopCanvas">Der Canvas, auf dem die Fenster angezeigt werden</param>
    public WindowManagerService(Canvas desktopCanvas)
    {
        _desktopCanvas = desktopCanvas ?? throw new ArgumentNullException(nameof(desktopCanvas));
    }

    /// <summary>
    /// Erstellt ein neues App-Fenster
    /// </summary>
    /// <param name="title">Titel des Fensters</param>
    /// <param name="content">Inhalt des Fensters</param>
    /// <param name="processId">ID des zugehörigen Prozesses</param>
    /// <returns>Das erstellte Fenster</returns>
    public AppWindow CreateWindow(string title, UIElement content, string processId)
    {
        var window = new AppWindow();
        window.Initialize(title, content, processId);
        
        // Fenster auf dem Desktop positionieren
        _desktopCanvas.Children.Add(window);
        
        // Zufällige Position für kaskadierte Fenster
        double left = 50 + (_windows.Count * 30) % 200;
        double top = 50 + (_windows.Count * 30) % 150;
        
        Canvas.SetLeft(window, left);
        Canvas.SetTop(window, top);
        
        // Z-Index für neue Fenster ist höher
        Panel.SetZIndex(window, 1);
        
        // Alle anderen Fenster unfokussieren
        foreach (var existingWindow in _windows)
        {
            existingWindow.Unfocus();
            Panel.SetZIndex(existingWindow, 0);
        }
        
        // Fenster in Liste aufnehmen
        _windows.Add(window);
        
        // Event-Handler für Schließen
        window.Closed += (s, e) => 
        {
            CloseWindow(window);
        };
        
        // Event-Handler für Aktivierung
        window.Activated += (s, e) => 
        {
            foreach (var w in _windows)
            {
                if (w != window)
                {
                    w.Unfocus();
                }
            }
        };
        
        return window;
    }

    /// <summary>
    /// Schließt ein Fenster
    /// </summary>
    /// <param name="window">Das zu schließende Fenster</param>
    public void CloseWindow(AppWindow window)
    {
        if (_windows.Contains(window))
        {
            _desktopCanvas.Children.Remove(window);
            _windows.Remove(window);
        }
    }

    /// <summary>
    /// Fokussiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu fokussierende Fenster</param>
    public void FocusWindow(AppWindow window)
    {
        if (_windows.Contains(window))
        {
            window.Focus();
            window.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Minimiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu minimierende Fenster</param>
    public void MinimizeWindow(AppWindow window)
    {
        if (_windows.Contains(window))
        {
            window.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Maximiert ein Fenster
    /// </summary>
    /// <param name="window">Das zu maximierende Fenster</param>
    public void MaximizeWindow(AppWindow window)
    {
        if (_windows.Contains(window) && window.Visibility == Visibility.Visible)
        {
            // Maximieren-Button klicken, wenn das Fenster nicht maximiert ist
            var button = window.FindName("MaximizeRestoreButton") as Button;
            if (button != null)
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }

    /// <summary>
    /// Stellt ein Fenster wieder her
    /// </summary>
    /// <param name="window">Das wiederherzustellende Fenster</param>
    public void RestoreWindow(AppWindow window)
    {
        if (_windows.Contains(window) && window.Visibility == Visibility.Visible)
        {
            // Wiederherstellen-Button klicken, wenn das Fenster maximiert ist
            var button = window.FindName("MaximizeRestoreButton") as Button;
            if (button != null)
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}