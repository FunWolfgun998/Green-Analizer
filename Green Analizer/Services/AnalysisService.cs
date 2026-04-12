using System;
using System.Collections.Generic;
using System.Linq;
using Green_Analizer.Models;

namespace Green_Analizer.Service
{
    public class AnalysisService
    {
        public StatisticheReport CalcolaStatistiche(List<DatoAmbientale> dati, string citta, string inquinante)
        {
            if (dati == null || dati.Count == 0) return new StatisticheReport { Citta = citta, NomeInquinante = inquinante };

            //variabile per considerare un'aria "critia"
            double limiteCritico = 50;
            if (inquinante == "PM2.5") limiteCritico = 25;
            if (inquinante == "NO2") limiteCritico = 120;

            //Tipo di inquinate che staimo andando a leggere
            Func<DatoAmbientale, double> sel;
            if (inquinante == "PM2.5") sel = d => d.PM25;
            else if (inquinante == "NO2") sel = d => d.NO2;
            else sel = d => d.PM10;

            double inquinanteMedio = Math.Round(dati.Average(sel), 1);

            return new StatisticheReport
            {
                Citta = citta,
                NomeInquinante = inquinante,
                TempMedia = Math.Round(dati.Average(d => d.TemperaturaMedia), 1),
                InquinanteMedio = inquinanteMedio,
                InquinanteMax = Math.Round(dati.Max(sel), 1),

                // Un giorno è critico se supera la fascia "Moderata" ed entra in "Poor"
                GiorniCritici = dati.Count(d => sel(d) > limiteCritico),

                PrecipitazioniTotali = Math.Round(dati.Sum(d => d.PrecipitazioniTotali), 1),
                VentoMedio = Math.Round(dati.Average(d => d.VentoMedia), 1),

                // Valutazione precisa in base alla tabella ufficiale
                QualitaAriaAqi = ValutaQualita(inquinante, inquinanteMedio)
            };
        }

        private string ValutaQualita(string inquinante, double valore)
        {
            if (inquinante == "PM2.5")
            {
                if (valore <= 10) return "Buona ";
                if (valore <= 20) return "Discreta ";
                if (valore <= 25) return "Moderata ";
                if (valore <= 50) return "Scadente ";
                if (valore <= 75) return "Molto Scadente ";
                return "Estremamente Scadente ";
            }
            else if (inquinante == "NO2")
            {
                if (valore <= 40) return "Buona ";
                if (valore <= 90) return "Discreta ";
                if (valore <= 120) return "Moderata ";
                if (valore <= 230) return "Scadente ";
                if (valore <= 340) return "Molto Scadente ";
                return "Estremamente Scadente ";
            }
            else // PM10
            {
                if (valore <= 20) return "Buona ";
                if (valore <= 40) return "Discreta ";
                if (valore <= 50) return "Moderata ";
                if (valore <= 100) return "Scadente ";
                if (valore <= 150) return "Molto Scadente ";
                return "Estremamente Scadente ";
            }
        }
    }
}