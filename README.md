# VirtuellesBetriebssystem

Ein simuliertes Betriebssystem mit grafischer Benutzeroberfläche, entwickelt in C# und WPF. Das Projekt bietet eine realistische Simulation eines modernen Betriebssystems mit verschiedenen integrierten Anwendungen und Systemdiensten.
## 🚀 Funktionen

- **Dateisystem-Verwaltung**: Virtuelles Dateisystem mit Verzeichnissen und Dateien
- **Prozess-Management**: Verwaltung von virtuellen Prozessen und Threads
- **Netzwerk-Simulation**: Virtuelle Netzwerkdienste und Webseiten
- **Shell-Umgebung**: Interaktive Kommandozeile mit Befehlshistorie und Auto-Vervollständigung
- **Fensterverwaltung**: Moderne grafische Benutzeroberfläche mit mehreren Anwendungsfenstern

![Screenshot 2025-06-18 141054](https://github.com/user-attachments/assets/cb2d5924-681e-42ec-b517-eec30f9d6977)


## 📋 Integrierte Anwendungen

### Terminal
Ein leistungsfähiges Terminal mit:
- Über 20 integrierten Befehlen (z.B. ls, cd, mkdir, rm)
- Befehlshistorie und -vervollständigung
- Farbige Ausgabe und klare Strukturierung
- Hilfe-System für alle verfügbaren Befehle

![Screenshot 2025-06-18 141107](https://github.com/user-attachments/assets/689edc1f-32d8-4a41-a522-0837ea939324)
![Screenshot 2025-06-18 141117](https://github.com/user-attachments/assets/68a11dfb-67b3-4326-aac0-cfd0d886d13f)



### Datei-Explorer
- Hierarchische Verzeichnisstruktur
- Navigation durch das virtuelle Dateisystem
- Grundlegende Dateiverwaltung (Erstellen, Löschen, Umbenennen)
- Unterstützung für verschiedene Ansichtsmodi

  ![Screenshot 2025-06-18 141147](https://github.com/user-attachments/assets/c00629e8-ef53-49ee-8b87-5670a98bd812)


### Task-Manager
- Echtzeitüberwachung von Systemprozessen
- Detaillierte Prozessinformationen (PID, CPU, Speicher)
- Ressourcenauslastung (CPU, RAM, Festplatte, Netzwerk)
- Thread-Verwaltung und Prozesssteuerung

  ![Screenshot 2025-06-18 141158](https://github.com/user-attachments/assets/0d167ebf-d52d-4a5d-9f75-8a6f4aa7889c)
  ![Screenshot 2025-06-18 141202](https://github.com/user-attachments/assets/67fef0fe-9e0d-4001-8934-834206fe813c)
  ![Screenshot 2025-06-18 141207](https://github.com/user-attachments/assets/de7e728b-9f49-4529-b216-09dc5581abdc)




### Text-Editor
- Einfache Textbearbeitung
- Syntax-Highlighting
- Datei speichern/laden Funktionalität
- UTF-8 Unterstützung

  ![Screenshot 2025-06-18 141152](https://github.com/user-attachments/assets/52c05001-3285-4c40-b25e-9d48d911501e)


### Web-Browser
- Simulation von Webseiten-Navigation
- Adressleiste mit Verlauf
- Tab-Management
- Basis-Browserfunktionen (Vor/Zurück, Neuladen)

  ![Screenshot 2025-06-18 141242](https://github.com/user-attachments/assets/d164b6e6-827b-42e3-b7c2-01a68e85b318)


## 🛠 Technische Details

Das Projekt ist in C# mit WPF (Windows Presentation Foundation) entwickelt und nutzt folgende Architekturkonzepte:
- MVVM-Architekturpattern für saubere Trennung von UI und Logik
- Dependency Injection für lose Kopplung der Komponenten
- Service-basierte Architektur für modulare Erweiterbarkeit
- Event-driven Programming für reaktive Benutzeroberfläche

### Systemanforderungen
- Windows 10 oder höher
- .NET 6.0 oder höher
- Mindestens 4GB RAM
- 100MB freier Festplattenspeicher

## 🔧 Installation

1. Klonen Sie das Repository
```bash
git clone https://github.com/Jannisr21/VirtuellesBetriebssystem.git
```

2. Öffnen Sie die Solution-Datei `VirtuellesBetriebssystem.sln` in Visual Studio

3. Stellen Sie sicher, dass alle erforderlichen NuGet-Pakete installiert sind

4. Kompilieren und starten Sie das Projekt

## 💻 Erste Schritte

Nach dem Start können Sie:

1. **Terminal öffnen**
   - Nutzen Sie den Terminal-Button in der oberen Leiste
   - Geben Sie `help` ein, um verfügbare Befehle anzuzeigen
   - Navigieren Sie mit `cd` und `ls` durch das Dateisystem

2. **Dateisystem erkunden**
   - Öffnen Sie den Datei-Explorer
   - Erstellen Sie neue Verzeichnisse und Dateien
   - Navigieren Sie durch die vorhandene Verzeichnisstruktur

3. **Prozesse überwachen**
   - Öffnen Sie den Task-Manager
   - Beobachten Sie Systemressourcen in Echtzeit
   - Analysieren Sie laufende Prozesse und Threads

4. **Web-Browser verwenden**
   - Simulieren Sie Internetzugriff
   - Testen Sie die Navigation
   - Erkunden Sie die Browser-Funktionen

## 🔍 Projektstruktur

```
VirtuellesBetriebssystem/
├── Components/          # UI-Komponenten
├── Core/               # Kernfunktionalität
│   ├── FileSystem/    # Virtuelles Dateisystem
│   ├── Network/       # Netzwerksimulation
│   ├── Process/       # Prozessverwaltung
│   └── Shell/         # Terminal und Befehle
├── Services/          # Systemdienste
└── Views/             # Hauptansichten
```

## 🛠️ Entwicklung

### Architektur
Das System basiert auf einer modularen Architektur mit klarer Trennung von:
- Kernfunktionalität (Core)
- Benutzeroberfläche (UI)
- Diensten (Services)
- Geschäftslogik (Business Logic)

### Erweiterbarkeit
Das System wurde mit Fokus auf Erweiterbarkeit entwickelt:
- Modulare Struktur für einfache Integration neuer Funktionen
- Klare Schnittstellen für Systemkomponenten
- Dokumentierte Erweiterungspunkte


