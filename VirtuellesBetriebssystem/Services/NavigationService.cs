using System.Windows.Controls;

namespace VirtuellesBetriebssystem.Services;

/// <summary>
/// Service für die Navigation zwischen Views
/// </summary>
public class NavigationService
{
    private readonly ContentControl _contentControl;
    
    public NavigationService(ContentControl contentControl)
    {
        _contentControl = contentControl;
    }
    
    /// <summary>
    /// Navigiert zu einer bestimmten View
    /// </summary>
    /// <param name="content">Die anzuzeigende View</param>
    public void Navigate(object content)
    {
        _contentControl.Content = content;
    }
}