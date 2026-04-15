# 🌍 Green Analyzer - Environmental Data Dashboard

**Green Analyzer** is a C# Windows Forms desktop application developed to explore, visualize, and analyze the correlation between weather conditions and air pollution. 
The project aims to demonstrate how Information Technology and Data Science can support the **Green Economy** by raising awareness about environmental sustainability.

## Features

*   **📍 Global Interactive Map:** Powered by GMap.NET, users can double-click anywhere on the globe to extract exact coordinates and analyze air quality in that specific area.
*   **📊 Advanced Data Visualization:** 
    *   *Timeline Chart:* Tracks the evolution of pollutants (PM10, PM2.5, NO2) or the European AQI over time.
    *   *Scatter Plot (Correlation):* Visually demonstrates the correlation between weather variables (Temperature, Wind, Precipitation) and pollution levels.
*   **European AQI Standards:** Evaluates the air quality data strictly following the official thresholds set by the **European Environment Agency (EEA)**, calculating exactly how many days the air quality was "Poor" or "Extremely Poor".
*   **Local Caching:** To avoid redundant API calls and save bandwidth, the software saves historical data locally in JSON format. It automatically loads the cached data in less than a second unless an update is needed.
*   **⭐ Favorites Management:** Users can save custom map points with personalized names (e.g., "Hometown", "London City Center") for quick access in future sessions.
*   **🌗 Dynamic Theming:** Features a custom-built, modern Dark/Light mode engine for a clean and professional User Experience.

## 🛠️ Technologies & Libraries Used

*   **Language:** C# (.NET Framework)
*   **UI Framework:** Windows Forms (WinForms) with custom-painted borderless controls.
*   **APIs:**[Open-Meteo Historical Weather API](https://open-meteo.com/) & Air Quality API (Processing up to 13 years of hourly data).
*   **Libraries:** 
    *   `Newtonsoft.Json` (Data Serialization)
    *   `GMap.NET.WinForms` (Interactive OpenStreetMap integration)
    *   `System.Windows.Forms.DataVisualization` (Charting)

## ⚙️Architecture

The application is built with a modular architecture, separating the Front-end from the Back-end logic:
1.  **Models (`DatoAmbientale`, `StatisticheReport`):** Define the data structure for daily averages and calculated KPIs.
2.  **Services (`ApiService`, `AnalysisService`, `StorageService`):** Handle HTTP asynchronous requests, parse massive JSON payloads, perform math/statistical aggregations, and manage local file I/O.
3.  **UI (`Form1`, `ThemeManager`):** Manages the responsive layout (TableLayoutPanels) and dynamically updates charts and data grids based on the user's filters.

---
*Note: The software graphical user interface (GUI) is currently available only in Italian.*
