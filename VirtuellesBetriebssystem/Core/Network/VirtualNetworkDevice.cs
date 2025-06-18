using System;
using System.Net;

namespace VirtuellesBetriebssystem.Core.Network
{
    /// <summary>
    /// Status eines virtuellen Netzwerkgerätes
    /// </summary>
    public enum NetworkStatus
    {
        Offline,
        Online,
        Connecting,
        Error
    }
    
    /// <summary>
    /// Repräsentiert ein virtuelles Netzwerkgerät im System
    /// </summary>
    public class VirtualNetworkDevice
    {
        /// <summary>
        /// Eindeutige ID des Geräts im Netzwerk
        /// </summary>
        public string DeviceId { get; }
        
        /// <summary>
        /// Name des Geräts
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// IP-Adresse des Geräts
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// MAC-Adresse des Geräts
        /// </summary>
        public string MacAddress { get; }
        
        /// <summary>
        /// Aktueller Status des Netzwerkgeräts
        /// </summary>
        public NetworkStatus Status { get; private set; }
        
        /// <summary>
        /// Zeitpunkt der letzten Aktivität
        /// </summary>
        public DateTime LastActivity { get; private set; }
        
        /// <summary>
        /// Konstruktor
        /// </summary>
        public VirtualNetworkDevice(string name, string ipAddress)
        {
            DeviceId = Guid.NewGuid().ToString("N");
            Name = name;
            IpAddress = ipAddress;
            MacAddress = GenerateRandomMacAddress();
            Status = NetworkStatus.Offline;
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Generiert eine zufällige MAC-Adresse
        /// </summary>
        private string GenerateRandomMacAddress()
        {
            var random = new Random();
            byte[] macBytes = new byte[6];
            random.NextBytes(macBytes);
            
            // Erste Byte anpassen für lokale Administrierung
            macBytes[0] = (byte)(macBytes[0] & 0xFE | 0x02);
            
            return string.Join(":", macBytes.Select(b => b.ToString("X2")));
        }
        
        /// <summary>
        /// Schaltet das Gerät online
        /// </summary>
        public void GoOnline()
        {
            Status = NetworkStatus.Online;
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Schaltet das Gerät offline
        /// </summary>
        public void GoOffline()
        {
            Status = NetworkStatus.Offline;
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Aktualisiert die letzte Aktivitätszeit
        /// </summary>
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Gibt eine String-Repräsentation des Geräts zurück
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({IpAddress}) - {Status}";
        }
    }
}