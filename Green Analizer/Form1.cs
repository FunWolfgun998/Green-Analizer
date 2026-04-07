using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Green_Analizer.Models;
using Green_Analizer.Service;

namespace Green_Analizer
{
    public partial class Form1 : Form
    {
        private Panel pnlTopBar;
        private FlowLayoutPanel flwFiltri;
        private TableLayoutPanel tblMainLayout;

        private ComboBox cmbCitta1;
        private ComboBox cmbCitta2;
        private DateTimePicker dtpInizio;
        private DateTimePicker dtpFine;
        private Button btnAnalizza;
        private Button btnTema;

        private Chart chartTemporale;
        private Chart chartCorrelazione;
        private DataGridView gridDati;
        private FlowLayoutPanel pnlKPI;

        private Label lblTempMedia;
        private Label lblPm10Media;
        private Label lblGiorniCritici;

        private Dictionary<string, (string Lat, string Lon)> _dizionarioCitta = new Dictionary<string, (string, string)>
        {
            { "Vicenza", ("45.5467", "11.5475") },
            { "Milano", ("45.4642", "9.1900") },
            { "Roma", ("41.9028", "12.4964") }
        };

        public Form1()
        {
            InitializeComponent();
            CostruisciInterfaccia();

            // Riempiamo le tendine
            cmbCitta1.DataSource = new BindingSource(_dizionarioCitta.Keys, null);
            cmbCitta2.DataSource = new BindingSource(_dizionarioCitta.Keys, null);

            // Selezioniamo due città diverse di default
            cmbCitta1.SelectedItem = "Vicenza";
            cmbCitta2.SelectedItem = "Roma";

            // Impostiamo il calendario a 1 anno fa (altrimenti carica troppi punti e diventa illeggibile!)
            dtpInizio.Value = DateTime.Today.AddYears(-1);
            dtpFine.Value = DateTime.Today;

            ThemeManager.ApplicaTema(this);
        }

        private void CostruisciInterfaccia()
        {
            this.Text = "Green Analyzer - Dashboard Ambientale";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 768);

            tblMainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, CellBorderStyle = TableLayoutPanelCellBorderStyle.None, ColumnCount = 2, RowCount = 2 };
            tblMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tblMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tblMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.Controls.Add(tblMainLayout);

            pnlTopBar = new Panel { Name = "CardTop", Dock = DockStyle.Top, Height = 60 };
            this.Controls.Add(pnlTopBar);

            flwFiltri = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15, 15, 0, 0), AutoSize = true };
            pnlTopBar.Controls.Add(flwFiltri);

            cmbCitta1 = new ComboBox { Width = 150, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCitta2 = new ComboBox { Width = 150, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            dtpInizio = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            dtpFine = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            btnAnalizza = new Button { Text = "Esegui Analisi", Width = 130, Height = 30, Cursor = Cursors.Hand };
            btnTema = new Button { Text = "Tema", Width = 80, Height = 30, Cursor = Cursors.Hand };

            btnTema.Click += BtnTema_Click;
            btnAnalizza.Click += BtnAnalizza_Click;

            flwFiltri.Controls.Add(new Label { Text = "Città 1:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            flwFiltri.Controls.Add(cmbCitta1);
            flwFiltri.Controls.Add(new Label { Text = "Città 2:", AutoSize = true, Margin = new Padding(20, 5, 5, 0) });
            flwFiltri.Controls.Add(cmbCitta2);
            flwFiltri.Controls.Add(new Label { Text = "Dal:", AutoSize = true, Margin = new Padding(20, 5, 5, 0) });
            flwFiltri.Controls.Add(dtpInizio);
            flwFiltri.Controls.Add(new Label { Text = "Al:", AutoSize = true, Margin = new Padding(10, 5, 5, 0) });
            flwFiltri.Controls.Add(dtpFine);
            flwFiltri.Controls.Add(new Label { Width = 30 });
            flwFiltri.Controls.Add(btnAnalizza);
            flwFiltri.Controls.Add(btnTema);

            chartTemporale = CreaGraficoBase("Andamento PM10 nel Tempo");
            chartCorrelazione = CreaGraficoBase("Correlazione Temperatura / PM10");

            gridDati = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            pnlKPI = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20) };
            Font fontKPI = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTempMedia = new Label { Text = "🌡️ Temp Media: -- °C", AutoSize = true, Font = fontKPI, Margin = new Padding(10) };
            lblPm10Media = new Label { Text = "🏭 PM10 Medio: -- µg/m³", AutoSize = true, Font = fontKPI, Margin = new Padding(10) };
            lblGiorniCritici = new Label { Text = "⚠️ Giorni Fuori Legge: --", AutoSize = true, Font = fontKPI, ForeColor = Color.Tomato, Margin = new Padding(10) };
            pnlKPI.Controls.Add(new Label { Text = "STATISTICHE CITTA' 1", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Italic), ForeColor = Color.Gray });
            pnlKPI.Controls.Add(lblTempMedia);
            pnlKPI.Controls.Add(lblPm10Media);
            pnlKPI.Controls.Add(lblGiorniCritici);

            Padding cardMargin = new Padding(10);
            Panel card1 = new Panel { Name = "Card1", Dock = DockStyle.Fill, Margin = cardMargin }; card1.Controls.Add(chartTemporale); tblMainLayout.Controls.Add(card1, 0, 0);
            Panel card2 = new Panel { Name = "Card2", Dock = DockStyle.Fill, Margin = cardMargin }; card2.Controls.Add(chartCorrelazione); tblMainLayout.Controls.Add(card2, 0, 1);
            Panel card3 = new Panel { Name = "Card3", Dock = DockStyle.Fill, Margin = cardMargin, Padding = new Padding(10) }; card3.Controls.Add(gridDati); tblMainLayout.Controls.Add(card3, 1, 0);
            Panel card4 = new Panel { Name = "Card4", Dock = DockStyle.Fill, Margin = cardMargin }; card4.Controls.Add(pnlKPI); tblMainLayout.Controls.Add(card4, 1, 1);
        }

        private Chart CreaGraficoBase(string titolo)
        {
            Chart chart = new Chart { Dock = DockStyle.Fill };
            ChartArea area = new ChartArea();
            chart.ChartAreas.Add(area);
            chart.Legends.Add(new Legend("DefaultLegend") { BackColor = Color.Transparent });
            chart.Titles.Add(new Title(titolo) { Font = new Font("Segoe UI", 12, FontStyle.Bold) });
            return chart;
        }

        private void BtnTema_Click(object sender, EventArgs e)
        {
            ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode;
            ThemeManager.ApplicaTema(this);
            // Forza l'aggiornamento dei colori personalizzati dei grafici se i dati sono caricati
            if (chartTemporale.Series.Count > 0) btnAnalizza.PerformClick();
        }

        // =========================================================================
        // IL MOTORE: LOGICA DI CARICAMENTO INTELLIGENTE E DISEGNO GRAFICI
        // =========================================================================

        private async void BtnAnalizza_Click(object sender, EventArgs e)
        {
            try
            {
                btnAnalizza.Enabled = false;
                btnAnalizza.Text = "Elaborazione...";

                string c1 = cmbCitta1.SelectedItem.ToString();
                string c2 = cmbCitta2.SelectedItem.ToString();

                // 1. SCARICA (O CARICA DA FILE) I DATI DELLE DUE CITTA'
                var datiC1 = await OttieniDatiIntelligente(c1);
                var datiC2 = await OttieniDatiIntelligente(c2);

                // 2. FILTRA PER DATA
                var filtriC1 = datiC1.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();
                var filtriC2 = datiC2.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();

                // 3. AGGIORNA INTERFACCIA
                gridDati.DataSource = filtriC1; // Mettiamo nella griglia la Città 1
                AggiornaGrafici(filtriC1, c1, filtriC2, c2);
                AggiornaKPI(filtriC1, c1);
            }
            catch (Exception ex) { MessageBox.Show("Errore: " + ex.Message); }
            finally { btnAnalizza.Enabled = true; btnAnalizza.Text = "Esegui Analisi"; }
        }

        // Sistema di "Cache": Legge dal JSON. Se manca o è vecchio, usa l'API.
        private async Task<List<DatoAmbientale>> OttieniDatiIntelligente(string citta)
        {
            StorageService storage = new StorageService();
            var dati = storage.CaricaDati(citta);

            // Se la lista è vuota oppure l'ultimo dato risale a più di 3 giorni fa -> SCARICA!
            if (dati.Count == 0 || dati.Last().Data < DateTime.Today.AddDays(-3))
            {
                ApiService api = new ApiService();
                var coord = _dizionarioCitta[citta];
                dati = await api.ScaricaDatiAsync(coord.Lat, coord.Lon);
                storage.SalvaDati(citta, dati); // Salva il nuovo file per la prossima volta
            }

            return dati;
        }

        private void AggiornaGrafici(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2)
        {
            chartTemporale.Series.Clear();
            chartCorrelazione.Series.Clear();

            // Colori dinamici in base al tema
            Color col1 = ThemeManager.IsDarkMode ? Color.SpringGreen : Color.SeaGreen;
            Color col2 = ThemeManager.IsDarkMode ? Color.Tomato : Color.OrangeRed;

            // --- GRAFICO TEMPORALE ---
            Series s1Temp = new Series(c1) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col1 };
            Series s2Temp = new Series(c2) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col2 };

            foreach (var d in d1) s1Temp.Points.AddXY(d.Data, d.PM10);
            foreach (var d in d2) s2Temp.Points.AddXY(d.Data, d.PM10);

            chartTemporale.Series.Add(s1Temp);
            chartTemporale.Series.Add(s2Temp);
            chartTemporale.ChartAreas[0].AxisX.Title = "Data";
            chartTemporale.ChartAreas[0].AxisY.Title = "Livello PM10 (µg/m³)";

            // --- GRAFICO CORRELAZIONE ---
            Series s1Corr = new Series(c1) { ChartType = SeriesChartType.Point, MarkerSize = 6, Color = col1 };
            Series s2Corr = new Series(c2) { ChartType = SeriesChartType.Point, MarkerSize = 6, Color = col2 };

            foreach (var d in d1) s1Corr.Points.AddXY(d.TemperaturaMedia, d.PM10);
            foreach (var d in d2) s2Corr.Points.AddXY(d.TemperaturaMedia, d.PM10);

            chartCorrelazione.Series.Add(s1Corr);
            chartCorrelazione.Series.Add(s2Corr);
            chartCorrelazione.ChartAreas[0].AxisX.Title = "Temperatura (°C)";
            chartCorrelazione.ChartAreas[0].AxisY.Title = "Livello PM10 (µg/m³)";
        }

        private void AggiornaKPI(List<DatoAmbientale> d1, string c1)
        {
            if (d1.Count == 0) return;

            double tempMedia = d1.Average(d => d.TemperaturaMedia);
            double pm10Media = d1.Average(d => d.PM10);
            int giorniCritici = d1.Count(d => d.PM10 > 50);

            lblTempMedia.Text = $"🌡️ Temp Media: {tempMedia:F1} °C";
            lblPm10Media.Text = $"🏭 PM10 Medio: {pm10Media:F1} µg/m³";
            lblGiorniCritici.Text = $"⚠️ Giorni Fuori Legge: {giorniCritici}";
        }
    }
}