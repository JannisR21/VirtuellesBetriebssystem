using System;
using System.Collections.Generic;

namespace VirtuellesBetriebssystem.Core.Network
{
    /// <summary>
    /// Interface für den virtuellen Netzwerkdienst
    /// </summary>
    public interface IVirtualNetworkService
    {
        /// <summary>
        /// Liste aller Netzwerkgeräte im virtuellen Netzwerk
        /// </summary>
        IEnumerable<VirtualNetworkDevice> GetNetworkDevices();
        
        /// <summary>
        /// Holt ein Netzwerkgerät anhand seiner ID
        /// </summary>
        VirtualNetworkDevice GetDeviceById(string deviceId);
        
        /// <summary>
        /// Holt ein Netzwerkgerät anhand seiner IP-Adresse
        /// </summary>
        VirtualNetworkDevice GetDeviceByIp(string ipAddress);
        
        /// <summary>
        /// Holt ein Netzwerkgerät anhand seines Namens
        /// </summary>
        VirtualNetworkDevice GetDeviceByName(string name);
        
        /// <summary>
        /// Fügt ein neues Gerät zum Netzwerk hinzu
        /// </summary>
        VirtualNetworkDevice AddDevice(string name, string ipAddress = null);
        
        /// <summary>
        /// Entfernt ein Gerät aus dem Netzwerk
        /// </summary>
        bool RemoveDevice(string deviceId);
        
        /// <summary>
        /// Führt einen Ping zu einer IP-Adresse durch
        /// </summary>
        /// <returns>Antwortzeit in ms, -1 bei Timeout</returns>
        int Ping(string ipAddress);
        
        /// <summary>
        /// Löst einen Hostnamen in eine IP-Adresse auf
        /// </summary>
        string ResolveHostname(string hostname);
        
        /// <summary>
        /// Ruft eine URL ab und gibt den Inhalt zurück
        /// </summary>
        string FetchUrl(string url);
        
        /// <summary>
        /// Holt die Liste aller verfügbaren Websites
        /// </summary>
        IEnumerable<VirtualWebsite> GetAvailableWebsites();
        
        /// <summary>
        /// Fügt eine neue Website hinzu
        /// </summary>
        void AddWebsite(VirtualWebsite website);
        
        /// <summary>
        /// Gibt das Lokalgerät zurück (eigener PC)
        /// </summary>
        VirtualNetworkDevice LocalDevice { get; }
        
        /// <summary>
        /// Ereignis, das bei Änderungen im Netzwerk ausgelöst wird
        /// </summary>
        event EventHandler<NetworkEventArgs> NetworkChanged;
    }
    
    /// <summary>
    /// Ereignisargumente für Netzwerkänderungen
    /// </summary>
    public class NetworkEventArgs : EventArgs
    {
        public string Message { get; }
        public VirtualNetworkDevice Device { get; }
        
        public NetworkEventArgs(string message, VirtualNetworkDevice device = null)
        {
            Message = message;
            Device = device;
        }
    }
}