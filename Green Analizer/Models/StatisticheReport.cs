using System;

namespace Green_Analizer.Models
{
    public class StatisticheReport
    {
        public string Citta { get; set; }
        public string NomeInquinante { get; set; }

        public double TempMedia { get; set; }
        public double InquinanteMedio { get; set; }
        public double InquinanteMax { get; set; }
        public int GiorniCritici { get; set; }

        public double PrecipitazioniTotali { get; set; }
        public double VentoMedio { get; set; }

        public double AqiMedio { get; set; }
        public string QualitaAriaAqi { get; set; }

        // Array per contare i giorni in ogni fascia [Good, Fair, Mod, Poor, VPoor, EPoor]
        public int[] GiorniFascePM25 { get; set; } = new int[6];
        public int[] GiorniFascePM10 { get; set; } = new int[6];
        public int[] GiorniFasceNO2 { get; set; } = new int[6];
    }
}