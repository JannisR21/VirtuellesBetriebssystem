using System;
using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Network
{
    /// <summary>
    /// Repräsentiert eine virtuelle Website im simulierten Netzwerk
    /// </summary>
    public class VirtualWebsite
    {
        /// <summary>
        /// Domain-Name der Website (z.B. "example.com")
        /// </summary>
        public string Domain { get; }
        
        /// <summary>
        /// Titel der Website
        /// </summary>
        public string Title { get; }
        
        /// <summary>
        /// IP-Adresse des Servers, auf dem die Website gehostet wird
        /// </summary>
        public string ServerIp { get; }
        
        /// <summary>
        /// Verfügbare Seiten auf der Website
        /// </summary>
        private readonly Dictionary<string, VirtualWebPage> _pages;
        
        /// <summary>
        /// Konstruktor
        /// </summary>
        public VirtualWebsite(string domain, string title, string serverIp)
        {
            Domain = domain.ToLower();
            Title = title;
            ServerIp = serverIp;
            _pages = new Dictionary<string, VirtualWebPage>();
            
            // Standardmäßig eine Index-Seite hinzufügen
            AddPage("/", "Startseite", $"<h1>{title}</h1><p>Willkommen auf {domain}</p>");
        }
        
        /// <summary>
        /// Fügt eine neue Seite zur Website hinzu
        /// </summary>
        public void AddPage(string path, string title, string content)
        {
            // Sicherstellen, dass der Pfad mit / beginnt
            if (!path.StartsWith("/"))
                path = "/" + path;
                
            _pages[path] = new VirtualWebPage(title, content);
        }
        
        /// <summary>
        /// Ruft eine Seite anhand ihres Pfades ab
        /// </summary>
        public VirtualWebPage GetPage(string path)
        {
            // Sicherstellen, dass der Pfad mit / beginnt
            if (!path.StartsWith("/"))
                path = "/" + path;
                
            // Leerer Pfad oder / zeigt auf die Index-Seite
            if (string.IsNullOrEmpty(path) || path == "/")
                path = "/";
                
            // Wenn die Seite existiert, zurückgeben
            if (_pages.TryGetValue(path, out VirtualWebPage page))
                return page;
                
            // Andernfalls 404-Seite zurückgeben
            return new VirtualWebPage("404 - Seite nicht gefunden", 
                "<h1>404 - Seite nicht gefunden</h1><p>Die angeforderte Seite existiert nicht.</p>");
        }
        
        /// <summary>
        /// Gibt alle verfügbaren Pfade auf der Website zurück
        /// </summary>
        public IEnumerable<string> GetAvailablePaths()
        {
            return _pages.Keys;
        }
    }
    
    /// <summary>
    /// Repräsentiert eine einzelne Seite auf einer virtuellen Website
    /// </summary>
    public class VirtualWebPage
    {
        /// <summary>
        /// Titel der Seite
        /// </summary>
        public string Title { get; }
        
        /// <summary>
        /// HTML-Inhalt der Seite
        /// </summary>
        public string HtmlContent { get; }
        
        /// <summary>
        /// Zeitpunkt der letzten Aktualisierung
        /// </summary>
        public DateTime LastModified { get; }
        
        /// <summary>
        /// Konstruktor
        /// </summary>
        public VirtualWebPage(string title, string htmlContent)
        {
            Title = title;
            HtmlContent = htmlContent;
            LastModified = DateTime.Now;
        }
    }
}