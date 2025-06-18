using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VirtuellesBetriebssystem.Services;

/// <summary>
/// Service für Einstellungen im System
/// </summary>
public class SettingsService
{
    private Dictionary<string, object> _settings = new Dictionary<string, object>();
    private readonly string _settingsFilePath;
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="settingsFilePath">Pfad zur Einstellungsdatei</param>
    public SettingsService(string settingsFilePath = "settings.json")
    {
        _settingsFilePath = settingsFilePath;
        LoadSettings();
    }
    
    /// <summary>
    /// Lädt die Einstellungen aus der Datei
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
        }
        catch (Exception)
        {
            // Fehler beim Laden der Einstellungen, Standard-Einstellungen verwenden
            _settings = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Speichert die Einstellungen in die Datei
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            string json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception)
        {
            // Fehler beim Speichern der Einstellungen
        }
    }
    
    /// <summary>
    /// Holt eine Einstellung
    /// </summary>
    /// <typeparam name="T">Typ der Einstellung</typeparam>
    /// <param name="key">Schlüssel der Einstellung</param>
    /// <param name="defaultValue">Standardwert, falls die Einstellung nicht existiert</param>
    /// <returns>Wert der Einstellung</returns>
    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            try
            {
                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// Setzt eine Einstellung
    /// </summary>
    /// <typeparam name="T">Typ der Einstellung</typeparam>
    /// <param name="key">Schlüssel der Einstellung</param>
    /// <param name="value">Wert der Einstellung</param>
    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value;
        
        // Event auslösen
        SettingChanged?.Invoke(this, new SettingChangedEventArgs(key, value));
    }
    
    /// <summary>
    /// Event, das ausgelöst wird, wenn sich eine Einstellung ändert
    /// </summary>
    public event EventHandler<SettingChangedEventArgs> SettingChanged;
}

/// <summary>
/// Event-Args für Einstellungsänderungen
/// </summary>
public class SettingChangedEventArgs : EventArgs
{
    /// <summary>
    /// Schlüssel der Einstellung
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// Neuer Wert der Einstellung
    /// </summary>
    public object Value { get; }
    
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="key">Schlüssel</param>
    /// <param name="value">Wert</param>
    public SettingChangedEventArgs(string key, object value)
    {
        Key = key;
        Value = value;
    }
}