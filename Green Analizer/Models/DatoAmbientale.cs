using System;

namespace Green_Analizer.Models
{
    //classe che contiene tutti i dati che estraiamo dal csv dell'API di un giorno(volendo anche di un'ora ma per una questione di performance è la media di tutto il giorno)
    public class DatoAmbientale
    {
        public DateTime Data { get; set; }

        public double TemperaturaMedia { get; set; }
        public double PrecipitazioniTotali { get; set; }
        public double VentoMedia { get; set; }

        public double PM10 { get; set; }
        public double PM25 { get; set; }
        public double NO2 { get; set; }

        public double? AqiPm25 { get; set; }
        public double? AqiPm10 { get; set; }
        public double? AqiNo2 { get; set; }
    }
}