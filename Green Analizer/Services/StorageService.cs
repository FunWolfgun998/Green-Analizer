using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Green_Analizer.Models;

namespace Green_Analizer.Service
{
    public class StorageService
    {
        // Ora il metodo richiede il NOME della città per creare file separati
        public void SalvaDati(string nomeCitta, List<DatoAmbientale> dati)
        {
            string percorsoFile = $"dati_storici_{nomeCitta.ToLower()}.json";
            string json = JsonConvert.SerializeObject(dati, Formatting.Indented);
            File.WriteAllText(percorsoFile, json);
        }

        public List<DatoAmbientale> CaricaDati(string nomeCitta)
        {
            string percorsoFile = $"dati_storici_{nomeCitta.ToLower()}.json";

            if (!File.Exists(percorsoFile))
                return new List<DatoAmbientale>();

            string json = File.ReadAllText(percorsoFile);
            return JsonConvert.DeserializeObject<List<DatoAmbientale>>(json) ?? new List<DatoAmbientale>();
        }
    }
}