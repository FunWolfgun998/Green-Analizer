using System;
using System.Diagnostics; // Necessario per Debug.WriteLine
using System.Threading.Tasks;
using System.Windows.Forms;
using Green_Analizer.Service;
using System.Windows.Forms.DataVisualization.Charting;

namespace Green_Analizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //Test BackEnd funzionante
        //static void Main()
        //{
        //    // Avviamo il test
        //    TestApiService().GetAwaiter().GetResult();
        //}
        //static async Task TestApiService()
        //{
        //    Debug.WriteLine("Avvio test API...");

        //    ApiService api = new ApiService();
        //    try
        //    {
        //        var dati = await api.ScaricaDatiAsync("45.5467", "11.5475");

        //        Debug.WriteLine($"Test completato! Dati scaricati: {dati.Count} giorni.");

        //        if (dati.Count > 0)
        //        {
        //            var ultimo = dati[dati.Count - 1];
        //            Debug.WriteLine($"Ultimo dato: {ultimo.Data.ToShortDateString()} - Temp: {ultimo.TemperaturaMedia}°C - PM10: {ultimo.PM10}");

        //            // Mostriamo un avviso grafico così vedi che ha funzionato
        //            MessageBox.Show($"Test superato! Scaricati {dati.Count} record.", "Successo");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"ERRORE: {ex.Message}");
        //        MessageBox.Show($"Errore nel test: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
    }
}