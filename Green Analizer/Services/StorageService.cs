using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Green_Analizer.Models;

namespace Green_Analizer.Service
{
    public class StorageService
    {
        private readonly string _fileCittaSalvate = "citta_preferite.json";

        // --- GESTIONE DEI DATI METEO ---
        public void SalvaDati(string nomeCitta, List<DatoAmbientale> dati)
        {
            string nomePulito = new string(nomeCitta.Where(c => char.IsLetterOrDigit(c)).ToArray());
            if (string.IsNullOrEmpty(nomePulito)) nomePulito = "temp_point";

            string percorsoFile = $"dati_{nomePulito.ToLower()}.json";
            string json = JsonConvert.SerializeObject(dati, Formatting.Indented);
            File.WriteAllText(percorsoFile, json);
        }

        public List<DatoAmbientale> CaricaDati(string nomeCitta)
        {
            string nomePulito = new string(nomeCitta.Where(c => char.IsLetterOrDigit(c)).ToArray());
            if (string.IsNullOrEmpty(nomePulito)) nomePulito = "temp_point";

            string percorsoFile = $"dati_{nomePulito.ToLower()}.json";

            if (!File.Exists(percorsoFile))
                return new List<DatoAmbientale>();

            string json = File.ReadAllText(percorsoFile);
            return JsonConvert.DeserializeObject<List<DatoAmbientale>>(json) ?? new List<DatoAmbientale>();
        }

        // --- GESTIONE DELLE CITTA' PREFERITE DALLA MAPPA ---
        public void SalvaListaCitta(Dictionary<string, (string Lat, string Lon)> dizionario)
        {
            string json = JsonConvert.SerializeObject(dizionario, Formatting.Indented);
            File.WriteAllText(_fileCittaSalvate, json);
        }

        public Dictionary<string, (string Lat, string Lon)> CaricaListaCitta()
        {
            if (!File.Exists(_fileCittaSalvate))
                return new Dictionary<string, (string, string)>();

            string json = File.ReadAllText(_fileCittaSalvate);
            return JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(json) ?? new Dictionary<string, (string, string)>();
        }
    }
}