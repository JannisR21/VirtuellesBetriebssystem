using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
// Hier keine System.Web-Referenz verwenden!
namespace VirtuellesBetriebssystem.Core.Network
{
    /// <summary>
    /// Implementierung des virtuellen Netzwerkdienstes
    /// </summary>
    public class VirtualNetworkService : IVirtualNetworkService
    {
        /// <summary>
        /// Liste aller Netzwerkgeräte
        /// </summary>
        private readonly List<VirtualNetworkDevice> _devices;
        
        /// <summary>
        /// DNS-Tabelle für Hostnamen-Auflösung
        /// </summary>
        private readonly Dictionary<string, string> _dnsTable;
        
        /// <summary>
        /// Verfügbare Websites
        /// </summary>
        private readonly List<VirtualWebsite> _websites;
        
        /// <summary>
        /// Zufallsgenerator
        /// </summary>
        private readonly Random _random;
        
        /// <summary>
        /// Das lokale Gerät
        /// </summary>
        public VirtualNetworkDevice LocalDevice { get; }
        
        /// <summary>
        /// Ereignis für Netzwerkänderungen
        /// </summary>
        public event EventHandler<NetworkEventArgs> NetworkChanged;
        
        /// <summary>
        /// Konstruktor
        /// </summary>
        public VirtualNetworkService()
        {
            _devices = new List<VirtualNetworkDevice>();
            _dnsTable = new Dictionary<string, string>();
            _websites = new List<VirtualWebsite>();
            _random = new Random();
            
            // Lokales Gerät erstellen
            LocalDevice = new VirtualNetworkDevice("Lokaler Computer", "192.168.1.100");
            LocalDevice.GoOnline();
            _devices.Add(LocalDevice);
            
            // DNS-Server hinzufügen
            var dnsServer = new VirtualNetworkDevice("DNS-Server", "192.168.1.1");
            dnsServer.GoOnline();
            _devices.Add(dnsServer);
            
            // Router hinzufügen
            var router = new VirtualNetworkDevice("Router", "192.168.1.254");
            router.GoOnline();
            _devices.Add(router);
            
            // Weitere Geräte hinzufügen
            AddDevice("Webserver-1", "192.168.1.10").GoOnline();
            AddDevice("Webserver-2", "192.168.1.11").GoOnline();
            AddDevice("Datenbankserver", "192.168.1.20").GoOnline();
            AddDevice("Drucker", "192.168.1.50").GoOnline();
            AddDevice("Gastcomputer", "192.168.1.101").GoOnline();
            
            // DNS-Einträge hinzufügen
            AddDnsEntry("router.local", "192.168.1.254");
            AddDnsEntry("dns.local", "192.168.1.1");
            AddDnsEntry("webserver1.local", "192.168.1.10");
            AddDnsEntry("webserver2.local", "192.168.1.11");
            AddDnsEntry("db.local", "192.168.1.20");
            AddDnsEntry("printer.local", "192.168.1.50");
            
            // Externe Websites
            AddDnsEntry("example.com", "203.0.113.10");
            AddDnsEntry("wiki.local", "192.168.1.10");
            AddDnsEntry("intranet.local", "192.168.1.11");
            AddDnsEntry("mail.local", "192.168.1.10");
            AddDnsEntry("news.local", "192.168.1.11");
            AddDnsEntry("downloads.local", "192.168.1.10");
            
            // Populäre Websites
            AddDnsEntry("google.com", "203.0.113.100");
            AddDnsEntry("youtube.com", "203.0.113.101");
            AddDnsEntry("facebook.com", "203.0.113.102");
            AddDnsEntry("twitter.com", "203.0.113.103");
            AddDnsEntry("amazon.de", "203.0.113.104");
            
            // Musterwebsites erstellen
            InitializeWebsites();
            
            // Event auslösen
            OnNetworkChanged("Netzwerk initialisiert");
        }
        
        /// <summary>
        /// Initialisiert die simulierten Websites
        /// </summary>
        private void InitializeWebsites()
        {
            // Beispielwebsite: Wiki
            var wikiSite = new VirtualWebsite("wiki.local", "Virtuelles Wiki", "192.168.1.10");
            wikiSite.AddPage("/about", "Über uns", 
                "<h1>Über das Wiki</h1><p>Diese Seite stellt ein virtuelles Wiki-System dar.</p>" +
                "<p>Hier können Sie verschiedene Informationen finden.</p>" +
                "<ul><li><a href=\"/\">Startseite</a></li><li><a href=\"/help\">Hilfe</a></li></ul>");
            wikiSite.AddPage("/help", "Hilfe", 
                "<h1>Hilfebereich</h1><p>Hier finden Sie Hilfe zur Verwendung des Wikis.</p>");
            _websites.Add(wikiSite);
            
            // Beispielwebsite: Intranet
            var intranetSite = new VirtualWebsite("intranet.local", "Firmen-Intranet", "192.168.1.11");
            intranetSite.AddPage("/news", "Neuigkeiten", 
                "<h1>Aktuelle Neuigkeiten</h1><p>Willkommen im Firmen-Intranet!</p>" +
                "<h2>System-Updates</h2><p>Das System wurde kürzlich aktualisiert.</p>");
            intranetSite.AddPage("/contact", "Kontakte", 
                "<h1>Kontaktliste</h1><ul><li>IT-Support: support@intranet.local</li>" +
                "<li>Verwaltung: admin@intranet.local</li></ul>");
            _websites.Add(intranetSite);
            
            // Beispielwebsite: E-Mail
            var mailSite = new VirtualWebsite("mail.local", "Webmail-System", "192.168.1.10");
            mailSite.AddPage("/inbox", "Posteingang", 
                "<h1>Posteingang</h1><p>Sie haben 3 ungelesene Nachrichten</p>" +
                "<ul><li>System: Willkommen im Mailsystem</li>" +
                "<li>Admin: Zugangsdaten für das Intranet</li>" +
                "<li>Support: Antwort auf Ihre Anfrage</li></ul>");
            _websites.Add(mailSite);
            
            // Beispielwebsite: News
            var newsSite = new VirtualWebsite("news.local", "Nachrichtenportal", "192.168.1.11");
            newsSite.AddPage("/tech", "Technologie", 
                "<h1>Technologie-News</h1><p>Die neuesten Entwicklungen in der Technikwelt</p>" +
                "<h2>Neues Betriebssystem</h2><p>Ein revolutionäres virtuelles Betriebssystem wurde heute vorgestellt.</p>");
            _websites.Add(newsSite);
            
            // Beispielwebsite: Downloads
            var downloadsSite = new VirtualWebsite("downloads.local", "Downloads Portal", "192.168.1.10");
            downloadsSite.AddPage("/", "Downloads", 
                "<h1>Downloads-Test</h1>" +
                "<p>Hier können Sie verschiedene Dateien herunterladen:</p>" +
                "<ul>" +
                "<li><a href=\"download:dokument.pdf:2500000\">Dokument.pdf (2.5 MB)</a></li>" +
                "<li><a href=\"download:beispiel.zip:15000000\">Beispiel.zip (15 MB)</a></li>" +
                "<li><a href=\"download:video.mp4:85000000\">Video.mp4 (85 MB)</a></li>" +
                "</ul>");
            _websites.Add(downloadsSite);
            
            // Beispielwebsite: Example.com
            var exampleSite = new VirtualWebsite("example.com", "Example Domain", "203.0.113.10");
            exampleSite.AddPage("/", "Example Domain", 
                "<h1>Example Domain</h1><p>This domain is for use in illustrative examples in documents.</p>" +
                "<p>You may use this domain in literature without prior coordination.</p>");
            _websites.Add(exampleSite);
            
            // Google Simulation
            var googleSite = new VirtualWebsite("google.com", "Google", "203.0.113.100");
            googleSite.AddPage("/", "Google", 
                "<div style=\"text-align:center; padding-top:100px;\">" +
                "<img src=\"data:image/png;base64,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\" style=\"width:272px;height:92px;\">" +
                "<form action=\"/search\" method=\"get\" style=\"margin-top:30px;\">" +
                "<input type=\"text\" name=\"q\" style=\"width:500px;height:40px;padding:5px;border:1px solid #ccc;border-radius:2px;font-size:18px;\">" +
                "<div style=\"margin-top:20px;\">" +
                "<button type=\"submit\" style=\"background:#f2f2f2;border:1px solid #f2f2f2;color:#757575;border-radius:2px;padding:10px 16px;font-size:14px;margin:0 4px;\">Google Suche</button>" +
                "<button type=\"submit\" name=\"btnI\" value=\"lucky\" style=\"background:#f2f2f2;border:1px solid #f2f2f2;color:#757575;border-radius:2px;padding:10px 16px;font-size:14px;margin:0 4px;\">Auf gut Glück!</button>" +
                "</div>" +
                "</form>" +
                "</div>");
                
            googleSite.AddPage("/search", "Google-Suche", 
                "<div style=\"padding:20px;\">" +
                "<div style=\"margin-bottom:20px;\">" +
                "<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAYMAAACCCAMAAACTkVQxAAAA/FBMVEX///9ChfTqQzX7vAU0qFPA4Mjx+fMho0eKyp5WtW7n9OrZ7t+q2LZzwYcnpUult/NtnfYzfvRVkPXqPi+DqfbpNiXpMB3pOyvqQTP7twB2pvboKhTpfXfsW1HuhnL95bj+9N77wSr+7Mr92pP8yU/8z2n8zmD+8NT93Z793qbrTj38xkDsXVj1r6r0kCPvbiHsVCzsUxP4oxr2nSL95rztYRzva0/weR7zjIDwgwD1rX73tQD1mTJdlvVBrV17woy63sLQ6taYzqeCUgIZAAALXklEQVR4nO2da3vaOBqGwSB8AgxYJglJMDllEmiS6WQ7abIzPe1Op7vT7i7//9+sbIOlV5Ydm6SxDTxfmlqg+JHfV0dvZB2c7qjOj472LQGl7lvHVq59S1Aacvf4+Lj7ct9SFN/Ll8fdbrfT7bzetyRF9/GYqIgm8/J43xIU3T+RCO2A6/N+P9rvHh/vW4qCO8WjPzrZ71uQYrsQRGAi+G3fohTaB04EVgQ/7FuWIjsVZJgrcLdvaYqL22yijHdnxHpvOTlmKhy92rc8hfXiWJLj+d2+JSoobyCEo6O7fYtUTG+FEOh+TfftJE7E9UZQgR6G7/YtUxGJ6o2owN/3LVMRCeuNqMB7EYQndF/S6BcUoXN0s2+pCkdcb3QFfty3VAXzcQ+6o7q7t2+xCkbKjhkF3u5brGIRnv/1vu5brGKR9EMywX/sW6xikTsaSgq83bdYxUJXTEUBDf6C3ojNf8nRv1Doj2T+iyE8iQR38n9QpMB7ixkJvUcU+F+xKODxRnRD9Gnh+P3y1vVPQTEGBxCO5BFB11rS9svl8u3ZxdrfJkWTpVgsv7Gx0I3cHTERvqWBMBpEGl1eL5a3IhVGUjRZisXynhkMRx/EZ4QjJ+dVZaSw8e7t4vqTyAzHUjRZisWmIWwYxGOhlR4NxxvrZvkWqvDLRnfz8XzxdgCHQ6SQpVgsb+ER8UjukhkVDlZz3lhsLgUVyOyHqwj+A4p1LoKw6KgX+N2RcD6IFTAVGP4qLMO1WEEwZhfjIviSzP1eXCIVFpsLkRmhycDYu1GkwF+C6zZ0HwfnsuXm4zcCGW7mKVw0nUHwTmQaGN7AYHhD2CRwvBFRYFbC8JtIBTKZMQluQaHOROJAaCB8FpoGnkMUEIyHQkfDqQaGPwhUIM+HQaKA8TGkwJlQqA9CE0HfhQZ8R0TsAXHwbSPD/Q9CC7MJ78a+iSTbHRlzH+TkR8F8aNJsE0vxYv2BHAhvhRTwYXMYP+0YIvsgGRFuYOfA8QZ89/oi3C3M0yTdJqEU17D3icuOwInA8Eb8afepqJuGkUOGx4DwiPwRmf+WCOHFMgr0NqYJpbjDJj+JA+wdEfPOZCLoLxdXTALp/DCc6Rkun+Ay+1J0xOECRniGwFzQGNCHARN8vLiGk5Dl7I0WBM+AAnch/FI0w4UbmEgdYRHSJyJJBdlA9+rqGk5CDia8eHYDPYI+F+GLQ4ZfpcN3YRHeiqQDEsHPcO7dABP+3CZDpZHNFVThzGZQODOaIhoIoxXTGGTkTQwGB4Ye3V4JiHAE7r56hpuGMJcMTYL5VyjD+UfLt1yGX2GhghXeaOmtpNlv7+3FlZACWAHvO9sAFnPE8EeZAaEUF+CQXcQbmbFepDey+2o+kRUAgwEs0xWpwCW00Ig3Ot0JBx1hb2Q+fZZ/FqtAlqsAMAO4mEtYqIkE7iHhcGR4I7uhUAXSKFcKNBqGLTRlYLcOCnUj/4nDvVGYJLmFvQFiuOZehfb0ApRq2B3FgVDEG5mzwO11thHgamB4rEDz/O16pB4Fs+UU+OFwL8obHQmPiOG9gcGHay4VgjaCdBQZXSsMRifpA3OwFKFQWIEmSMBuIfgYDEe7WDUyFI/qXl1eQQlwnvAkLNRYhE+JOQlnoliOMm9kOYSgPu15ynCFqg66pwQhbqXCNRQqvO0cCDURWG/0WigexvdeXkJfZLkaQMJbzCM7DtbQsMhARNjnPRiOzP+xQ0F7cLPgT8DpfWUVqpfr/NbQXR2nDKI4bwzvjYzPIvOw5eX1DSAAXsYnYaFO7P45vgKO0RDujT7uQHeU+9V9GHyCEiRLTl6mG8N/hQqw9JHtjUKQqvuE9V5eX18JJYClAkfK+FzMYfVHhhcyg6G4N+IiIaPjkICIAWexiXEOVCi/KQ+TpwCLJXZEJBxZC0ZX1zdQAp6CnF+mO0ZWwwY7GIp6IyrCa9l6727EJcgV5rBQ9//QfJtgBkNxb3TyN5EYRFI4uICfZQDtR3FDOMyG4ZuAWh7h3kgInYilm8urq4VAH7SmQlpYqFFTxVLAB8OzWTZKzL6IJMJfCp8ury6F8yEfTdnBMGJ4BRQAzQRpNER5I4FcYOgvj+BmgAB5N/SQjcuJA6ZVOB+y0ZCeDWNkb9C9QXRHBheBkxsX4CsDmAcYiuQzXOdwQuwvwQUAOhgu5I2k9+t2b26uRS4NnFamcDnJcN07mww/AhO8KaQ3OsVdERVh8QlJgNsB90Z3bDmpKt2YRWDvGcZ6I/4Nwj2C+eXV9cXyOssBsOuKP7Y3nEOGf4IK9FnKG4nuwBLdvbu4uPgkNAHQF8n75X5ysm8IZrCXV9gbvRCfEY44vbq5Wbxf8FeXxIBcRgVwfnD4aTAcsj3CudAf5Y3EIt3Rh5sFIcbyU9bZQGggTHq94j40ghFXXG8kvjuO9G5x8/7CUoDHG5GJ4ELWoN2cnQJnBlUoyRtJ+CFcbm5vFtc3MBjyWW4G3zgqEgfHON6IJEMDnGc2y88X18szoEJJNjBcfSXaoPVRaP6PvJH0YhjnRr8ul8uL5bvl1Z+L94txkggFZZgBBcx1PvXrqF8XeW9kIVvv8u7y9vr69vr63fLsLCMCGYzBfpi0WJvxqFiGWx/leSORDZYp3d29uxVcwOMD80AcDROVpcAQaAqT/I3CXHYM0QSKfQAI1hqMYD5M/jnYEWD0oYdnIu+N9ihAc60xoAq077+kIQiOR4kKiAIgGX7aRwIAVfDfRN4b7S8BnCAF2udbgPAw4WRYlgxXYYcgJkPkjfbngYh8NcQpYM72ZxuA4HOamxYZXjYXVOAPQ/ZN5L3RvnwRGQkNN8QVoAoMp1umAOFzmoLLKcNLkAx/H7IW6h++rL/f55YAd6/p0QgqsFUCzLDDwdFn7I1KkQy/fxuyz0jeG+3B1OPGQi9L8ZwZbrxB5c1wxe2S4J+gS+wr3hvF6w37zg1+LAzpCrQK8YV1w2Nk9OkJ+zg1JkPsjUrgjSx7IXnQZ1wGmg+dWAlQjEGwxggPvFG50pBltOk/3+Mv6pWuAGX7CHnRUnjBVrY8ZDQc/gEHw71HR5zsGr8ejFJLpAVs1eUb6KaNM7wxGLLZ8HnTBFOB5h/I0xCUoEWuqYbVPcQ3k+F/hux/0/NGTxsn2DKSdXlh0ZFDAjSZQlKAMtxXtmmgfwxD5naeNzrLF7+Gw/ck/IQKTLGjYVGgOZ06DEGnwxEz/Z83TnC2qm0YTTsEwTnMg0WKJz/C5TQC8GU6+/JG0q8YGtIUaGEF4D7dBQRfyFBKrSsGlTc3vvntZrPPWZP6CDgavDRCvEFHQ4MuIfUegfZ9fGsEAzRE3WlTWXnjxGaYGRbQ1HAEHQ0zzIY1GmvREJVlQ/5kOKs9YwWaTrOD0HKZA8CwkZ10FAm9Ea4kGU5GGTwK1FtNn6VDPBqW3RbVnQZeJG+kKCJp8MZ/o0CL22bRHZAHnQO3R6VQKv1mBm9EJoKtFICORkUqMGTdxl8lRfJGatN1fAaFSdXmCmCqjXs76Qz/qpZ8Lig/AvTGQAVaBvN+UQDZ4E81n0eB9qhS3UEIhv+OTAQVXCLWmxE2AnRBZQehPcgfDQ0Fmp7JFMBUPeVnOaB0KSyfXdFfZe17FQXHn+vZHH7bw8r/YQc1R4KUNzJVoGd+TIYdAOwfQMtAMsweDdNFU24e8gbDZiWv8pQKw28a3jMpWG/UhwpUkA9iqdaMbzJ6+bpzBVqmv+kHlSUVDzm29k1k98ZXdTQiOBiC8aS+tQI0y/1NPqg8qbgYWDNkMkzZQZiWp1NpbKMAq0L7JudnJBVKSb9JW7Jh5W4/0ZFHpj+a+LUcMb5fxg7bnEp/PBXbR1Rx1CrmCrGkVOpjBcrb7dXRbZJhnjDsWP8HkPNdEzAf0CcAAAAASUVORK5CYII=\" style=\"width:120px;height:40px;margin-right:30px;\">" +
                "<input type=\"text\" value=\"Suchanfrage\" style=\"width:500px;height:30px;padding:5px;border:1px solid #ccc;border-radius:2px;font-size:16px;\">" +
                "</div>" +
                "<div style=\"border-bottom:1px solid #e0e0e0;margin-bottom:10px;\"></div>" +
                "<div>" +
                "<p>Ihre Suche nach <b>\"Suchanfrage\"</b> ergab keine genauen Treffer.</p>" +
                "<p>Vorschläge:</p>" +
                "<ul>" +
                "<li>Vergewissern Sie sich, dass alle Wörter richtig geschrieben sind.</li>" +
                "<li>Versuchen Sie andere Suchbegriffe.</li>" +
                "<li>Versuchen Sie allgemeinere Suchbegriffe.</li>" +
                "</ul>" +
                "<p>In einer realen Anwendung würden hier Suchergebnisse angezeigt werden. In dieser simulierten Version wird stattdessen diese Mitteilung angezeigt.</p>" +
                "<h3>Hinweis:</h3>" +
                "<p>Da dies ein simulierter Browser in einem virtuellen Betriebssystem ist, können keine echten Websites aufgerufen werden.</p>" +
                "</div>" +
                "</div>");
            _websites.Add(googleSite);
            
            // YouTube Simulation
            var youtubeSite = new VirtualWebsite("youtube.com", "YouTube", "203.0.113.101");
            youtubeSite.AddPage("/", "YouTube", 
                "<div style=\"padding:20px;\">" +
                "<div style=\"background:#ff0000;color:white;padding:10px 20px;display:inline-block;\">" +
                "<span style=\"font-size:24px;font-weight:bold;\">YouTube</span>" +
                "</div>" +
                "<div style=\"margin-top:20px;\">" +
                "<input type=\"text\" placeholder=\"Suchen\" style=\"width:500px;height:40px;padding:5px;border:1px solid #ccc;border-radius:2px;font-size:16px;\">" +
                "<button style=\"background:#f8f8f8;border:1px solid #d3d3d3;color:#333;padding:10px 16px;margin-left:8px;\">Suchen</button>" +
                "</div>" +
                "<div style=\"margin-top:40px;\">" +
                "<div style=\"font-size:18px;font-weight:bold;margin-bottom:15px;\">Empfohlene Videos</div>" +
                "<div style=\"display:flex;flex-wrap:wrap;\">" +
                "<div style=\"width:240px;margin:10px;border:1px solid #e0e0e0;\">" +
                "<div style=\"height:135px;background:#e0e0e0;display:flex;align-items:center;justify-content:center;\">Vorschaubild</div>" +
                "<div style=\"padding:10px;\">" +
                "<div style=\"font-weight:bold;\">Virtuelles Betriebssystem Demo</div>" +
                "<div style=\"color:#606060;font-size:13px;\">Demo-Kanal</div>" +
                "<div style=\"color:#606060;font-size:13px;\">5.321 Aufrufe • vor 2 Stunden</div>" +
                "</div>" +
                "</div>" +
                "<div style=\"width:240px;margin:10px;border:1px solid #e0e0e0;\">" +
                "<div style=\"height:135px;background:#e0e0e0;display:flex;align-items:center;justify-content:center;\">Vorschaubild</div>" +
                "<div style=\"padding:10px;\">" +
                "<div style=\"font-weight:bold;\">Web-Browser Tutorial</div>" +
                "<div style=\"color:#606060;font-size:13px;\">Technik-Tipps</div>" +
                "<div style=\"color:#606060;font-size:13px;\">12.481 Aufrufe • vor 1 Tag</div>" +
                "</div>" +
                "</div>" +
                "<div style=\"width:240px;margin:10px;border:1px solid #e0e0e0;\">" +
                "<div style=\"height:135px;background:#e0e0e0;display:flex;align-items:center;justify-content:center;\">Vorschaubild</div>" +
                "<div style=\"padding:10px;\">" +
                "<div style=\"font-weight:bold;\">Programmieren lernen leicht gemacht</div>" +
                "<div style=\"color:#606060;font-size:13px;\">Programmier-Hilfe</div>" +
                "<div style=\"color:#606060;font-size:13px;\">45.127 Aufrufe • vor 3 Tagen</div>" +
                "</div>" +
                "</div>" +
                "</div>" +
                "</div>" +
                "</div>");
            _websites.Add(youtubeSite);
        }
        
        /// <summary>
        /// Fügt einen DNS-Eintrag hinzu
        /// </summary>
        private void AddDnsEntry(string hostname, string ipAddress)
        {
            _dnsTable[hostname.ToLower()] = ipAddress;
        }

        /// <summary>
        /// Liefert alle Netzwerkgeräte zurück
        /// </summary>
        public IEnumerable<VirtualNetworkDevice> GetNetworkDevices()
        {
            return _devices.AsReadOnly();
        }

        /// <summary>
        /// Holt ein Gerät anhand seiner ID
        /// </summary>
        public VirtualNetworkDevice GetDeviceById(string deviceId)
        {
            return _devices.FirstOrDefault(d => d.DeviceId == deviceId);
        }

        /// <summary>
        /// Holt ein Gerät anhand seiner IP-Adresse
        /// </summary>
        public VirtualNetworkDevice GetDeviceByIp(string ipAddress)
        {
            return _devices.FirstOrDefault(d => d.IpAddress == ipAddress);
        }

        /// <summary>
        /// Holt ein Gerät anhand seines Namens
        /// </summary>
        public VirtualNetworkDevice GetDeviceByName(string name)
        {
            return _devices.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Fügt ein neues Gerät zum Netzwerk hinzu
        /// </summary>
        public VirtualNetworkDevice AddDevice(string name, string ipAddress = null)
        {
            // IP-Adresse generieren, falls nicht angegeben
            if (string.IsNullOrEmpty(ipAddress))
            {
                do
                {
                    int lastOctet = _random.Next(1, 254);
                    ipAddress = $"192.168.1.{lastOctet}";
                } while (_devices.Any(d => d.IpAddress == ipAddress));
            }
            
            // Prüfen, ob die IP-Adresse bereits verwendet wird
            if (_devices.Any(d => d.IpAddress == ipAddress))
            {
                throw new InvalidOperationException($"Die IP-Adresse {ipAddress} wird bereits verwendet.");
            }
            
            // Neues Gerät erstellen
            var device = new VirtualNetworkDevice(name, ipAddress);
            _devices.Add(device);
            
            // Event auslösen
            OnNetworkChanged($"Gerät {name} hinzugefügt", device);
            
            return device;
        }

        /// <summary>
        /// Entfernt ein Gerät aus dem Netzwerk
        /// </summary>
        public bool RemoveDevice(string deviceId)
        {
            var device = GetDeviceById(deviceId);
            if (device != null)
            {
                _devices.Remove(device);
                OnNetworkChanged($"Gerät {device.Name} entfernt", device);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Führt einen Ping zu einer IP-Adresse durch
        /// </summary>
        public int Ping(string ipAddress)
        {
            // Für externe Websites (wie Google, etc.) immer eine Verbindung zulassen
            if (ipAddress.StartsWith("203.0.113."))
            {
                // Gerät ist online, also Ping-Zeit simulieren
                Thread.Sleep(10); // Kurze Verzögerung für die Simulation
                
                // Zufällige Ping-Zeit zwischen 1 und 50 ms simulieren
                return _random.Next(1, 50);
            }
            
            // Prüfen, ob das Gerät existiert und online ist
            var device = GetDeviceByIp(ipAddress);
            if (device == null || device.Status != NetworkStatus.Online)
            {
                return -1; // Timeout
            }
            
            // Gerät ist online, also Ping-Zeit simulieren
            Thread.Sleep(10); // Kurze Verzögerung für die Simulation
            
            // Zufällige Ping-Zeit zwischen 1 und 50 ms simulieren
            int pingTime = _random.Next(1, 50);
            
            // Aktivität des Geräts aktualisieren
            device.UpdateActivity();
            
            return pingTime;
        }

        /// <summary>
        /// Löst einen Hostnamen in eine IP-Adresse auf
        /// </summary>
        public string ResolveHostname(string hostname)
        {
            // Normalisieren
            hostname = hostname.ToLower();
            
            // In DNS-Tabelle suchen
            if (_dnsTable.TryGetValue(hostname, out string ipAddress))
            {
                return ipAddress;
            }
            
            // Nicht gefunden
            return null;
        }

        /// <summary>
        /// Ruft eine URL ab und gibt den Inhalt zurück
        /// </summary>
        public string FetchUrl(string url)
        {
            try
            {
                // URL parsen
                var uri = new Uri(url);
                
                // Hostname auflösen
                string hostname = uri.Host.ToLower();
                string ipAddress = ResolveHostname(hostname);
                
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return $"<h1>Fehler: Host nicht gefunden</h1><p>Der Hostname {hostname} konnte nicht aufgelöst werden.</p>";
                }
                
                // Ping zum Server durchführen (Verbindung simulieren)
                int pingTime = Ping(ipAddress);
                if (pingTime < 0)
                {
                    return $"<h1>Fehler: Verbindung fehlgeschlagen</h1><p>Die Verbindung zu {hostname} ({ipAddress}) konnte nicht hergestellt werden.</p>";
                }
                
                // Website finden
                var website = _websites.FirstOrDefault(w => w.Domain.Equals(hostname, StringComparison.OrdinalIgnoreCase));
                if (website == null)
                {
                    return $"<h1>Fehler: Website nicht gefunden</h1><p>Die Website {hostname} existiert nicht.</p>";
                }
                
                // Pfad extrahieren
                string path = uri.AbsolutePath;
                
                // Wenn bei Google eine Suche durchgeführt wird, übergebe das Such-Query
                if (hostname == "google.com" && path == "/search")
                {
                    string query = "Suchanfrage"; // Standardwert
                    
                    try
                    {
                        // Parameter aus der URI auslesen
                        if (!string.IsNullOrEmpty(uri.Query))
                        {
                            // Eigene Implementierung für Parse QueryString
                            var queryParams = ParseQueryString(uri.Query);
                            if (queryParams.ContainsKey("q") && !string.IsNullOrEmpty(queryParams["q"]))
                            {
                                query = queryParams["q"];
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Im Fehlerfall verwenden wir den Standardwert
                        query = "Suchanfrage";
                    }
                    
                    // Die GetPage-Methode ersetzen durch eine dynamische Seite
                    string searchContent = "<div style=\"padding:20px;\">" +
                        "<div style=\"margin-bottom:20px;\">" +
                        "<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAYMAAACCCAMAAACTkVQxAAAA/FBMVEX///9ChfTqQzX7vAU0qFPA4Mjx+fMho0eKyp5WtW7n9OrZ7t+q2LZzwYcnpUult/NtnfYzfvRVkPXqPi+DqfbpNiXpMB3pOyvqQTP7twB2pvboKhToIwgQoD398/LpOTjrTkHwsq74vbntaF/61tX++Pj74eDzoZz0qqXe5/385uXveHHuioPufXfsW1HuhnL95bj+9N77wSr+7Mr92pP8yU/8z2n8zmD+8NT93Z793qbrTj38xkDsXVj1r6r0kCPvbiHsVCzsUxP4oxr2nSL95rztYRzva0/weR7zjIDwgwD1rX73tQD1mTJdlvVBrV17woy63sLQ6taYzqeCUgIZAAALXklEQVR4nO2da3vaOBqGwSB8AgxYJglJMDllEmiS6WQ7abIzPe1Op7vT7i7//9+sbIOlV5Ydm6SxDTxfmlqg+JHfV0dvZB2c7qjOj472LQGl7lvHVq59S1Aacvf4+Lj7ct9SFN/Ll8fdbrfT7bzetyRF9/GYqIgm8/J43xIU3T+RCO2A6/N+P9rvHh/vW4qCO8WjPzrZ71uQYrsQRGAi+G3fohTaB04EVgQ/7FuWIjsVZJgrcLdvaYqL22yijHdnxHpvOTlmKhy92rc8hfXiWJLj+d2+JSoobyCEo6O7fYtUTG+FEOh+TfftJE7E9UZQgR6G7/YtUxGJ6o2owN/3LVMRCeuNqMB7EYQndF/S6BcUoXN0s2+pCkdcb3QFfty3VAXzcQ+6o7q7t2+xCkbKjhkF3u5brGIRnv/1vu5brGKR9EMywX/sW6xikTsaSgq83bdYxUJXTEUBDf6C3ojNf8nRv1Doj2T+iyE8iQR38n9QpMB7ixkJvUcU+F+xKODxRnRD9Gnh+P3y1vVPQTEGBxCO5BFB11rS9svl8u3ZxdrfJkWTpVgsv7Gx0I3cHTERvqWBMBpEGl1eL5a3IhVGUjRZisXynhkMRx/EZ4QjJ+dVZaSw8e7t4vqTyAzHUjRZisWmIWwYxGOhlR4NxxvrZvkWqvDLRnfz8XzxdgCHQ6SQpVgsb+ER8UjukhkVDlZz3lhsLgUVyOyHqwj+A4p1LoKw6KgX+N2RcD6IFTAVGP4qLMO1WEEwZhfjIviSzP1eXCIVFpsLkRmhycDYu1GkwF+C6zZ0HwfnsuXm4zcCGW7mKVw0nUHwTmQaGN7AYHhD2CRwvBFRYFbC8JtIBTKZMQluQaHOROJAaCB8FpoGnkMUEIyHQkfDqQaGPwhUIM+HQaKA8TGkwJlQqA9CE0HfhQZ8R0TsAXHwbSPD/Q9CC7MJ78a+iSTbHRlzH+TkR8F8aNJsE0vxYv2BHAhvhRTwYXMYP+0YIvsgGRFuYOfA8QZ89/oi3C3M0yTdJqEU17D3icuOwInA8Eb8afepqJuGkUOGx4DwiPwRmf+WCOHFMgr0NqYJpbjDJj+JA+wdEfPOZCLoLxdXTALp/DCc6Rkun+Ay+1J0xOECRniGwFzQGNCHARN8vLiGk5Dl7I0WBM+AAnch/FI0w4UbmEgdYRHSJyJJBdlA9+rqGk5CDia8eHYDPYI+F+GLQ4ZfpcN3YRHeiqQDEsHPcO7dABP+3CZDpZHNFVThzGZQODOaIhoIoxXTGGTkTQwGB4Ye3V4JiHAE7r56hpuGMJcMTYL5VyjD+UfLt1yGX2GhghXeaOmtpNlv7+3FlZACWAHvO9sAFnPE8EeZAaEUF+CQXcQbmbFepDey+2o+kRUAgwEs0xWpwCW00Ig3Ot0JBx1hb2Q+fZZ/FqtAlqsAMAO4mEtYqIkE7iHhcGR4I7uhUAXSKFcKNBqGLTRlYLcOCnUj/4nDvVGYJLmFvQFiuOZehfb0ApRq2B3FgVDEG5mzwO11thHgamB4rEDz/O16pB4Fs+UU+OFwL8obHQmPiOG9gcGHay4VgjaCdBQZXSsMRifpA3OwFKFQWIEmSMBuIfgYDEe7WDUyFI/qXl1eQQlwnvAkLNRYhE+JOQlnoliOMm9kOYSgPu15ynCFqg66pwQhbqXCNRQqvO0cCDURWG/0WigexvdeXkJfZLkaQMJbzCM7DtbQsMhARNjnPRiOzP+xQ0F7cLPgT8DpfWUVqpfr/NbQXR2nDKI4bwzvjYzPIvOw5eX1DSAAXsYnYaFO7P45vgKO0RDujT7uQHeU+9V9GHyCEiRLTl6mG8N/hQqw9JHtjUKQqvuE9V5eX18JJYClAkfK+FzMYfVHhhcyg6G4N+IiIaPjkICIAWexiXEOVCi/KQ+TpwCLJXZEJBxZC0ZX1zdQAp6CnF+mO0ZWwwY7GIp6IyrCa9l6727EJcgV5rBQ9//QfJtgBkNxb3TyN5EYRFI4uICfZQDtR3FDOMyG4ZuAWh7h3kgInYilm8urq4VAH7SmQlpYqFFTxVLAB8OzWTZKzL6IJMJfCp8ury6F8yEfTdnBMGJ4BRQAzQRpNER5I4FcYOgvj+BmgAB5N/SQjcuJA6ZVOB+y0ZCeDWNkb9C9QXRHBheBkxsX4CsDmAcYiuQzXOdwQuwvwQUAOhgu5I2k9+t2b26uRS4NnFamcDnJcN07mww/AhO8KaQ3OsVdERVh8QlJgNsB90Z3bDmpKt2YRWDvGcZ6I/4Nwj2C+eXV9cXyOssBsOuKP7Y3nEOGf4IK9FnKG4nuwBLdvbu4uPgkNAHQF8n75X5ysm8IZrCXV9gbvRCfEY44vbq5Wbxf8FeXxIBcRgVwfnD4aTAcsj3CudAf5Y3EIt3Rh5sFIcbyU9bZQGggTHq94j40ghFXXG8kvjuO9G5x8/7CUoDHG5GJ4ELWoN2cnQJnBlUoyRtJ+CFcbm5vFtc3MBjyWW4G3zgqEgfHON6IJEMDnGc2y88X18szoEJJNjBcfSXaoPVRaP6PvJH0YhjnRr8ul8uL5bvl1Z+L94txkggFZZgBBcx1PvXrqF8XeW9kIVvv8u7y9vr69vr63fLsLCMCGYzBfpi0WJvxqFiGWx/leSORDZYp3d29uxVcwOMD80AcDROVpcAQaAqT/I3CXHYM0QSKfQAI1hqMYD5M/jnYEWD0oYdnIu+N9ihAc60xoAq077+kIQiOR4kKiAIgGX7aRwIAVfDfRN4b7S8BnCAF2udbgPAw4WRYlgxXYYcgJkPkjfbngYh8NcQpYM72ZxuA4HOamxYZXjYXVOAPQ/ZN5L3RvnwRGQkNN8QVoAoMp1umAOFzmoLLKcNLkAx/H7IW6h++rL/f55YAd6/p0QgqsFUCzLDDwdFn7I1KkQy/fxuyz0jeG+3B1OPGQi9L8ZwZbrxB5c1wxe2S4J+gS+wr3hvF6w37zg1+LAzpCrQK8YV1w2Nk9OkJ+zg1JkPsjUrgjSx7IXnQZ1wGmg+dWAlQjEGwxggPvFG50pBltOk/3+Mv6pWuAGX7CHnRUnjBVrY8ZDQc/gEHw71HR5zsGr8ejFJLpAVs1eUb6KaNM7wxGLLZ8HnTBFOB5h/I0xCUoEWuqYbVPcQ3k+F/hux/0/NGTxsn2DKSdXlh0ZFDAjSZQlKAMtxXtmmgfwxD5naeNzrLF7+Gw/ck/IQKTLGjYVGgOZ06DEGnwxEz/Z83TnC2qm0YTTsEwTnMg0WKJz/C5TQC8GU6+/JG0q8YGtIUaGEF4D7dBQRfyFBKrSsGlTc3vvntZrPPWZP6CDgavDRCvEFHQ4MuIfUegfZ9fGsEAzRE3WlTWXnjxGaYGRbQ1HAEHQ0zzIY1GmvREJVlQ/5kOKs9YwWaTrOD0HKZA8CwkZ10FAm9Ea4kGU5GGTwK1FtNn6VDPBqW3RbVnQZeJG+kKCJp8MZ/o0CL22bRHZAHnQO3R6VQKv1mBm9EJoKtFICORkUqMGTdxl8lRfJGatN1fAaFSdXmCmCqjXs76Qz/qpZ8Lig/AvTGQAVaBvN+UQDZ4E81n0eB9qhS3UEIhv+OTAQVXCLWmxE2AnRBZQehPcgfDQ0Fmp7JFMBUPeVnOaB0KSyfXdFfZe17FQXHn+vZHH7bw8r/YQc1R4KUNzJVoGd+TIYdAOwfQMtAMsweDdNFU24e8gbDZiWv8pQKw28a3jMpWG/UhwpUkA9iqdaMbzJ6+bpzBVqmv+kHlSUVDzm29k1k98ZXdTQiOBiC8aS+tQI0y/1NPqg8qbgYWDNkMkzZQZiWp1NpbKMAq0L7JudnJBVKSb9JW7Jh5W4/0ZFHpj+a+LUcMb5fxg7bnEp/PBXbR1Rx1CrmCrGkVOpjBcrb7dXRbZJhnjDsWP8HkPNdEzAf0CcAAAAASUVORK5CYII=\" style=\"width:120px;height:40px;margin-right:30px;\">" +
                        "<input type=\"text\" value=\"" + query + "\" style=\"width:500px;height:30px;padding:5px;border:1px solid #ccc;border-radius:2px;font-size:16px;\">" +
                        "</div>" +
                        "<div style=\"border-bottom:1px solid #e0e0e0;margin-bottom:10px;\"></div>" +
                        "<div>" +
                        "<p>Ihre Suche nach <b>\"" + query + "\"</b> ergab keine genauen Treffer.</p>" +
                        "<p>Vorschläge:</p>" +
                        "<ul>" +
                        "<li>Vergewissern Sie sich, dass alle Wörter richtig geschrieben sind.</li>" +
                        "<li>Versuchen Sie andere Suchbegriffe.</li>" +
                        "<li>Versuchen Sie allgemeinere Suchbegriffe.</li>" +
                        "</ul>" +
                        "<p>In einer realen Anwendung würden hier Suchergebnisse angezeigt werden. In dieser simulierten Version wird stattdessen diese Mitteilung angezeigt.</p>" +
                        "<h3>Hinweis:</h3>" +
                        "<p>Da dies ein simulierter Browser in einem virtuellen Betriebssystem ist, können keine echten Websites aufgerufen werden.</p>" +
                        "</div>" +
                        "</div>";
                        
                    return searchContent;
                }
                
                // Seite abrufen
                var page = website.GetPage(path);
                
                // Antwort mit simulierter Verzögerung
                Thread.Sleep(_random.Next(50, 200)); // Netzwerklatenz simulieren (50-200ms)
                
                // Inhalt zurückgeben
                return page.HtmlContent;
            }
            catch (Exception ex)
            {
                return $"<h1>Fehler</h1><p>Beim Abrufen der URL ist ein Fehler aufgetreten: {ex.Message}</p>";
            }
        }

        /// <summary>
        /// Holt die Liste aller verfügbaren Websites
        /// </summary>
        public IEnumerable<VirtualWebsite> GetAvailableWebsites()
        {
            return _websites.AsReadOnly();
        }
        
        /// <summary>
        /// Fügt eine neue Website hinzu
        /// </summary>
        public void AddWebsite(VirtualWebsite website)
        {
            // Prüfen, ob die Website bereits existiert
            if (_websites.Any(w => w.Domain.Equals(website.Domain, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Eine Website mit der Domain {website.Domain} existiert bereits.");
            }
            
            // Website hinzufügen
            _websites.Add(website);
            
            // DNS-Eintrag automatisch hinzufügen, falls nicht vorhanden
            if (!_dnsTable.ContainsKey(website.Domain.ToLower()))
            {
                AddDnsEntry(website.Domain, website.ServerIp);
            }
            
            // Event auslösen
            OnNetworkChanged($"Website {website.Domain} hinzugefügt");
        }
        
        /// <summary>
        /// Löst das NetworkChanged-Ereignis aus
        /// </summary>
        protected virtual void OnNetworkChanged(string message, VirtualNetworkDevice device = null)
        {
            NetworkChanged?.Invoke(this, new NetworkEventArgs(message, device));
        }
        
        /// <summary>
        /// Eigene Implementierung für das Parsen von Query-Strings
        /// </summary>
        private Dictionary<string, string> ParseQueryString(string query)
        {
            // Dictionary zur Speicherung der Parameter
            var result = new Dictionary<string, string>();
            
            // Entferne führendes "?" falls vorhanden
            if (query.StartsWith("?"))
            {
                query = query.Substring(1);
            }
            
            // Aufteilen an "&"
            var pairs = query.Split('&');
            
            foreach (var pair in pairs)
            {
                // Aufteilen an "=" in Schlüssel und Wert
                var parts = pair.Split('=');
                
                if (parts.Length >= 1)
                {
                    string key = Uri.UnescapeDataString(parts[0]);
                    string value = parts.Length >= 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                    
                    // Zum Dictionary hinzufügen (ggf. überschreiben)
                    result[key] = value;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Generiert einen zufälligen Titel basierend auf dem Suchbegriff
        /// </summary>
        private string GenerateTitle(string query, int resultNumber)
        {
            string[] titleFormats = new string[] 
            {
                "{0} - Alles was du wissen musst",
                "{0}: Eine umfassende Anleitung",
                "Lerne {0} einfach und schnell",
                "{0} für Anfänger und Fortgeschrittene",
                "Die besten Tipps zu {0}",
                "{0} verstehen: Der ultimative Guide",
                "Wie man mit {0} erfolgreich wird",
                "{0} im Überblick",
                "{0} - Grundlagen und Expertenwissen",
                "Alles über {0} - vollständige Anleitung"
            };
            
            int index = (resultNumber + query.Length) % titleFormats.Length;
            return string.Format(titleFormats[index], query);
        }
        
        /// <summary>
        /// Generiert eine zufällige URL basierend auf dem Suchbegriff
        /// </summary>
        private string GenerateRandomURL(string query)
        {
            string[] domains = new string[] 
            {
                "wikipedia.org",
                "tutorial-zentrale.de",
                "infoportal.info",
                "ratgeber-welt.com",
                "dasgrosse{0}portal.de",
                "allesüber{0}.info",
                "tippsund{0}.de",
                "wissen-{0}.net",
                "lexikon-{0}.org",
                "online-{0}-akademie.de"
            };
            
            string normalizedQuery = query.ToLower().Replace(" ", "-");
            int index = (_random.Next(0, domains.Length));
            string domain = domains[index];
            
            // Wenn ein Platzhalter im Domain-Namen enthalten ist, ersetzen
            if (domain.Contains("{0}"))
            {
                domain = string.Format(domain, normalizedQuery);
            }
            
            return $"https://www.{domain}/{normalizedQuery}";
        }
        
        /// <summary>
        /// Generiert einen zufälligen Snippet-Text basierend auf dem Suchbegriff
        /// </summary>
        private string GenerateSnippet(string query, int resultNumber)
        {
            string[] snippetFormats = new string[] 
            {
                "Hier finden Sie alles zum Thema {0}. Wir bieten Ihnen umfassende Informationen, praktische Anleitungen und Expertentipps. Ideal für Einsteiger und Fortgeschrittene.",
                "Lernen Sie alles Wichtige über {0} in diesem umfassenden Tutorial. Von den Grundlagen bis hin zu fortgeschrittenen Techniken - hier werden Sie fündig.",
                "{0} verstehen und anwenden. Die besten Strategien und Methoden, um {0} effektiv einzusetzen. Mit zahlreichen Praxisbeispielen und Übungen.",
                "Entdecken Sie die Geheimnisse von {0}. Unser Experten-Team hat die wichtigsten Informationen zusammengestellt, damit Sie schnell und einfach lernen können.",
                "Der ultimative Ratgeber zu {0}. Tipps, Tricks und nützliche Hinweise, die Ihnen helfen, das Thema {0} vollständig zu verstehen.",
                "Alles was Sie über {0} wissen müssen. Diese Seite bietet Ihnen einen vollständigen Überblick und beantwortet alle wichtigen Fragen zum Thema.",
                "{0} - Ein kompletter Leitfaden für Anfänger und Fortgeschrittene. Lernen Sie die wichtigsten Aspekte kennen und verstehen Sie die Feinheiten.",
                "Die besten Ressourcen zum Thema {0}. Hier finden Sie sorgfältig ausgewählte Informationen, die Ihnen helfen, {0} besser zu verstehen.",
                "Verstehen Sie die Grundlagen von {0} mit unserer Schritt-für-Schritt-Anleitung. Ideal für alle, die sich für dieses Thema interessieren.",
                "Warum {0} wichtig ist und wie Sie es effektiv nutzen können. Die wichtigsten Fakten und Methoden, verständlich erklärt."
            };
            
            int index = (resultNumber + query.Length + _random.Next(0, 3)) % snippetFormats.Length;
            return string.Format(snippetFormats[index], query);
        }
    }
}