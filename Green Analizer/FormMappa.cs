using System;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Green_Analizer
{
    public class FormMappa : Form
    {
        private GMapControl mapControl;

        // Proprietà per salvare le coordinate scelte dall'utente
        public string LatitudineScelta { get; private set; }
        public string LongitudineScelta { get; private set; }
        public bool PuntoSelezionato { get; private set; } = false;

        public FormMappa()
        {
            this.Text = "Seleziona un punto nel mondo (Doppio Clic per confermare)";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;

            GMapProvider.UserAgent = "GreenAnalyzer";
            // Inizializza il controllo Mappa
            mapControl = new GMapControl();
            mapControl.Dock = DockStyle.Fill;
            this.Controls.Add(mapControl);

            // Configurazione della Mappa (Usa OpenStreetMap, gratis e senza chiavi)
            mapControl.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // Impostazioni vista
            mapControl.Position = new PointLatLng(45.5467, 11.5475); // Partiamo da Vicenza
            mapControl.MinZoom = 2;
            mapControl.MaxZoom = 18;
            mapControl.Zoom = 5; // Zoom sull'Europa
            mapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapControl.CanDragMap = true;
            mapControl.DragButton = MouseButtons.Left;
            mapControl.ShowCenter = false;

            // Evento: Doppio clic per scegliere il punto
            mapControl.MouseDoubleClick += MapControl_MouseDoubleClick;
        }

        private void MapControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Converte i pixel dello schermo in coordinate GPS (Lat, Lon)
                PointLatLng point = mapControl.FromLocalToLatLng(e.X, e.Y);

                // Formattiamo le coordinate con il punto (Invariant Culture) per l'API
                LatitudineScelta = point.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
                LongitudineScelta = point.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture);

                PuntoSelezionato = true;

                // Mostra un feedback visivo e chiude la finestra
                MessageBox.Show($"Punto selezionato!\nLat: {LatitudineScelta}\nLon: {LongitudineScelta}", "Coordinate acquisite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}