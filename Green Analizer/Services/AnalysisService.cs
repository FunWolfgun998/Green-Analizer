using System;
using System.Collections.Generic;
using System.Linq;
using Green_Analizer.Models;

namespace Green_Analizer.Service
{
    public class AnalysisService
    {
        //Metodo per ritornare le elaborazione di tutti i dati 
        public StatisticheReport CalcolaStatistiche(List<DatoAmbientale> dati, string citta, string inquinante)
        {
            StatisticheReport report = new StatisticheReport { Citta = citta, NomeInquinante = inquinante };
            if (dati == null || dati.Count == 0) return report;

            double limiteCritico = 50;
            if (inquinante == "PM2.5") limiteCritico = 25;
            if (inquinante == "NO2") limiteCritico = 120;

            Func<DatoAmbientale, double> selValore;
            Func<DatoAmbientale, double> selAqi;

            if (inquinante == "PM2.5") { selValore = d => d.PM25; selAqi = d => d.AqiPm25 ?? 0; }
            else if (inquinante == "NO2") { selValore = d => d.NO2; selAqi = d => d.AqiNo2 ?? 0; }
            else if (inquinante == "AQI (Generale)") { 
                // Se seleziona AQI, prendiamo il peggiore tra i 3 (Regola ufficiale Europea)
                selValore = d => Math.Max(d.AqiPm10 ?? 0, Math.Max(d.AqiPm25 ?? 0, d.AqiNo2 ?? 0)); 
                selAqi = selValore; 
            }
            else { selValore = d => d.PM10; selAqi = d => d.AqiPm10 ?? 0; }

            // Calcolo conteggio fasce per le tabelle
            foreach (var d in dati)
            {
                // PM2.5
                if (d.PM25 <= 10) report.GiorniFascePM25[0]++;
                else if (d.PM25 <= 20) report.GiorniFascePM25[1]++;
                else if (d.PM25 <= 25) report.GiorniFascePM25[2]++;
                else if (d.PM25 <= 50) report.GiorniFascePM25[3]++;
                else if (d.PM25 <= 75) report.GiorniFascePM25[4]++;
                else report.GiorniFascePM25[5]++;

                // PM10
                if (d.PM10 <= 20) report.GiorniFascePM10[0]++;
                else if (d.PM10 <= 40) report.GiorniFascePM10[1]++;
                else if (d.PM10 <= 50) report.GiorniFascePM10[2]++;
                else if (d.PM10 <= 100) report.GiorniFascePM10[3]++;
                else if (d.PM10 <= 150) report.GiorniFascePM10[4]++;
                else report.GiorniFascePM10[5]++;

                // NO2
                if (d.NO2 <= 40) report.GiorniFasceNO2[0]++;
                else if (d.NO2 <= 90) report.GiorniFasceNO2[1]++;
                else if (d.NO2 <= 120) report.GiorniFasceNO2[2]++;
                else if (d.NO2 <= 230) report.GiorniFasceNO2[3]++;
                else if (d.NO2 <= 340) report.GiorniFasceNO2[4]++;
                else report.GiorniFasceNO2[5]++;

                double aqiGiorno = Math.Max(d.AqiPm10 ?? 0, Math.Max(d.AqiPm25 ?? 0, d.AqiNo2 ?? 0));

                if (aqiGiorno <= 20) report.GiorniFasceAQI[0]++;
                else if (aqiGiorno <= 40) report.GiorniFasceAQI[1]++;
                else if (aqiGiorno <= 60) report.GiorniFasceAQI[2]++;
                else if (aqiGiorno <= 80) report.GiorniFasceAQI[3]++;
                else if (aqiGiorno <= 100) report.GiorniFasceAQI[4]++;
                else report.GiorniFasceAQI[5]++;
            }

            double valMedio = Math.Round(dati.Average(selValore), 1);
            double aqiMedio = Math.Round(dati.Average(selAqi), 1);

            report.TempMedia = Math.Round(dati.Average(d => d.TemperaturaMedia), 1);
            report.InquinanteMedio = valMedio;
            report.InquinanteMax = Math.Round(dati.Max(selValore), 1);
            report.GiorniCritici = dati.Count(d => selValore(d) > limiteCritico);
            report.PrecipitazioniTotali = Math.Round(dati.Sum(d => d.PrecipitazioniTotali), 1);
            report.VentoMedio = Math.Round(dati.Average(d => d.VentoMedia), 1);
            report.AqiMedio = aqiMedio;
            report.QualitaAriaAqi = ValutaAqiUfficiale(aqiMedio);

            return report;
        }

        private string ValutaAqiUfficiale(double aqi)
        {
            if (aqi <= 20) return "Good";
            if (aqi <= 40) return "Fair";
            if (aqi <= 60) return "Moderate";
            if (aqi <= 80) return "Poor";
            if (aqi <= 100) return "Very Poor";
            return "Extremely Poor";
        }
    }
}