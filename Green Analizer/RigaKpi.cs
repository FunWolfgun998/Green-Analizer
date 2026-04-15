namespace Green_Analizer
{
    //Classe che contiene il numero di giorni per ogni livello della qualità dell'aria per AQI, PM10, PM2.5 e NO2
    public class RigaKpi
    {
        public string Pollutant { get; set; }
        public int Good { get; set; }
        public int Fair { get; set; }
        public int Mod { get; set; }
        public int Poor { get; set; }
        public int VPoor { get; set; }
        public int EPoor { get; set; }
    }
}