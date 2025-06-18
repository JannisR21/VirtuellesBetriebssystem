using System;
using System.Windows;

namespace VirtuellesBetriebssystem.Services;

/// <summary>
/// Service für Benachrichtigungen im System
/// </summary>
public class NotificationService
{
    /// <summary>
    /// Zeigt eine Benachrichtigung an
    /// </summary>
    /// <param name="message">Die Nachricht</param>
    /// <param name="title">Der Titel</param>
    /// <param name="severity">Die Schwere (Info, Warning, Error)</param>
    public void ShowNotification(string message, string title = "Information", NotificationSeverity severity = NotificationSeverity.Info)
    {
        MessageBoxImage icon = MessageBoxImage.Information;
        
        switch (severity)
        {
            case NotificationSeverity.Warning:
                icon = MessageBoxImage.Warning;
                break;
            case NotificationSeverity.Error:
                icon = MessageBoxImage.Error;
                break;
        }
        
        // Im echten Betriebssystem würde hier eine Toast-Notification oder ähnliches angezeigt werden
        // In unserer Simulation verwenden wir MessageBox
        MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        
        // Event auslösen
        NotificationShown?.Invoke(this, new NotificationEventArgs(title, message, severity));
    }
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn eine Benachrichtigung angezeigt wird
    /// </summary>
    public event EventHandler<NotificationEventArgs> NotificationShown;
}

/// <summary>
/// Schwere einer Benachrichtigung
/// </summary>
public enum NotificationSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Event-Args für Benachrichtigungen
/// </summary>
public class NotificationEventArgs : EventArgs
{
    /// <summary>
    /// Titel der Benachrichtigung
    /// </summary>
    public string Title { get; }
    
    /// <summary>
    /// Nachricht der Benachrichtigung
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Schwere der Benachrichtigung
    /// </summary>
    public NotificationSeverity Severity { get; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="title">Titel</param>
    /// <param name="message">Nachricht</param>
    /// <param name="severity">Schwere</param>
    public NotificationEventArgs(string title, string message, NotificationSeverity severity)
    {
        Title = title;
        Message = message;
        Severity = severity;
    }
}