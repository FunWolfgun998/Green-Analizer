using Green_Analizer.Models;
using Green_Analizer.Service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Green_Analizer
{
    public partial class Form1 : Form
    {
        private Panel pnlTopBar;
        private FlowLayoutPanel flwFiltri;
        private TableLayoutPanel tblMainLayout;

        private ComboBox cmbCitta1;
        private ComboBox cmbCitta2;
        private ComboBox cmbInquinante;
        private ComboBox cmbVista;
        private DateTimePicker dtpInizio;
        private DateTimePicker dtpFine;
        private Button btnAnalizza;
        private Button btnTema;

        private Chart chartTemporale;
        private Chart chartCorrelazione;
        private DataGridView gridDati;
        private TableLayoutPanel pnlKPI;
        private Label lblC1Nome, lblC1Temp, lblC1Pol, lblC1Critici;
        private Label lblC2Nome, lblC2Temp, lblC2Pol, lblC2Critici;

        // Usiamo la nostra nuova classe RoundButton
        private IconHoverButton btnMappa1;
        private IconHoverButton btnMappa2;

        private ComboBox cmbCorrelazioneX;
        private ToolTip ttInfo;
        private Label lblInfo;

        private Label lblC1Meteo, lblC1Aqi;
        private Label lblC2Meteo, lblC2Aqi;

        private StorageService _storage = new StorageService();
        private AnalysisService _analizzatore = new AnalysisService();


        // Dizionario di base
        private Dictionary<string, (string Lat, string Lon)> _dizionarioCitta = new Dictionary<string, (string, string)>
        {
            { "Vicenza", ("45.5467", "11.5475") },
            { "Milano", ("45.4642", "9.1900") },
            { "Roma", ("41.9028", "12.4964") },
            { "Napoli", ("40.8518", "14.2681") },
            { "Torino", ("45.0703", "7.6869") }
        };

        public Form1()
        {
            InitializeComponent();
            CaricaCittaSalvate(); // Carica i preferiti salvati in precedenza
            CostruisciInterfaccia();

            cmbCitta1.DataSource = new BindingSource(_dizionarioCitta.Keys, null);
            cmbCitta2.DataSource = new BindingSource(_dizionarioCitta.Keys, null);
            cmbCitta1.SelectedItem = "Vicenza";
            cmbCitta2.SelectedItem = "Roma";

            dtpInizio.Value = DateTime.Today.AddYears(-1);
            dtpFine.Value = DateTime.Today;

            ThemeManager.ApplicaTema(this);
        }

        // Carica la lista di città "Preferite" aggiunte in passato
        private void CaricaCittaSalvate()
        {
            var cittaSalvate = _storage.CaricaListaCitta();
            foreach (var citta in cittaSalvate)
            {
                if (!_dizionarioCitta.ContainsKey(citta.Key))
                {
                    _dizionarioCitta.Add(citta.Key, citta.Value);
                }
            }
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

            TableLayoutPanel tblTop = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 19, // 19 colonne per 19 elementi
                RowCount = 1,
                BackColor = Color.Transparent
            };
            pnlTopBar.Controls.Add(tblTop);

            // Piccola funzione interna per centrare gli elementi nella tabella
            void AddToTop(Control c, int col)
            {
                c.Anchor = AnchorStyles.None;
                c.Margin = new Padding(2, 0, 2, 0);
                tblTop.Controls.Add(c, col, 0);
            }

            //Creazione elementi top bar
            cmbCitta1 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCitta2 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };

            cmbInquinante = new ComboBox { Width = 80, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbInquinante.Items.AddRange(new string[] { "PM10", "PM2.5", "NO2" });
            cmbInquinante.SelectedIndex = 0;

            ToolTip ttInfo = new ToolTip { AutoPopDelay = 20000, InitialDelay = 200, ReshowDelay = 200, IsBalloon = true, ToolTipTitle = "Soglie Ufficiali Qualità Aria (µg/m³)", ToolTipIcon = ToolTipIcon.Info };
            string testoTabella =
                "--- PM2.5 (Medie 24h) ---\n" +
                "Buono: 0-10 | Discreto: 10-20 | Moderato: 20-25\nScadente: 25-50 | Molto Scadente: 50-75 | Pessimo: > 75\n\n" +
                "--- PM10 (Medie 24h) ---\n" +
                "Buono: 0-20 | Discreto: 20-40 | Moderato: 40-50\nScadente: 50-100 | Molto Scadente: 100-150 | Pessimo: > 150\n\n" +
                "--- NO2 (Medie Orarie) ---\n" +
                "Buono: 0-40 | Discreto: 40-90 | Moderato: 90-120\nScadente: 120-230 | Molto Scadente: 230-340 | Pessimo: > 340";

            Label lblInfo = new Label { Text = "ℹ️", AutoSize = true, Cursor = Cursors.Help, Font = new Font("Segoe UI Emoji", 14) };
            ttInfo.SetToolTip(lblInfo, testoTabella);

            cmbCorrelazioneX = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCorrelazioneX.Items.AddRange(new string[] { "Temperatura", "Precipitazioni", "Vento" });
            cmbCorrelazioneX.SelectedIndex = 0;

            cmbVista = new ComboBox { Width = 130, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVista.Items.AddRange(new string[] { "Doppio Grafico", "Solo Andamento", "Solo Correlazione" });
            cmbVista.SelectedIndex = 0;
            cmbVista.SelectedIndexChanged += CmbVista_SelectedIndexChanged;

            dtpInizio = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            dtpFine = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            btnAnalizza = new Button { Text = "Esegui Analisi", Width = 110, Height = 28, Cursor = Cursors.Hand };
            btnTema = new Button { Text = "Tema", Width = 60, Height = 28, Cursor = Cursors.Hand };

            // --- NUOVI BOTTONI MAPPA (Stile Icona Trasparente) ---
            btnMappa1 = new IconHoverButton { Text = "🌍" };
            btnMappa1.Click += (s, e) => ScegliDaMappa(cmbCitta1);

            btnMappa2 = new IconHoverButton { Text = "🌍" };
            btnMappa2.Click += (s, e) => ScegliDaMappa(cmbCitta2);

            btnTema.Click += BtnTema_Click;
            btnAnalizza.Click += BtnAnalizza_Click;

            // Inserimento parte superiore (usa la funzione AddToTop per allineare tutto)
            AddToTop(new Label { Text = "Punto 1:", AutoSize = true }, 0);
            AddToTop(cmbCitta1, 1);
            AddToTop(btnMappa1, 2);

            AddToTop(new Label { Text = "VS Punto 2:", AutoSize = true }, 3);
            AddToTop(cmbCitta2, 4);
            AddToTop(btnMappa2, 5);

            AddToTop(new Label { Text = "Inquinante:", AutoSize = true }, 6);
            AddToTop(cmbInquinante, 7);
            AddToTop(lblInfo, 8); // L'icona della "i"

            AddToTop(new Label { Text = "vs Meteo:", AutoSize = true }, 9);
            AddToTop(cmbCorrelazioneX, 10);

            AddToTop(new Label { Text = "Dal:", AutoSize = true }, 11);
            AddToTop(dtpInizio, 12);

            AddToTop(new Label { Text = "Al:", AutoSize = true }, 13);
            AddToTop(dtpFine, 14);

            AddToTop(new Label { Text = "Vista:", AutoSize = true }, 15);
            AddToTop(cmbVista, 16);

            // Spazio vuoto opzionale nella colonna 17 (se la tabella avesse più colonne), altrimenti mettiamo direttamente i bottoni
            AddToTop(btnAnalizza, 17);
            AddToTop(btnTema, 18);

            // Parte centrale
            chartTemporale = CreaGraficoBase("Andamento nel Tempo");
            chartCorrelazione = CreaGraficoBase("Correlazione Temperatura / Inquinante");

            gridDati = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            // LAYOUT DOPPIO PER LE STATISTICHE
            pnlKPI = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, BackColor = Color.Transparent };
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Font fontNome = new Font("Segoe UI", 12, FontStyle.Bold);
            Font fontVal = new Font("Segoe UI", 10, FontStyle.Regular);

            // Etichette Città 1
            lblC1Nome = new Label { Text = "Città 1", Font = fontNome, AutoSize = true, ForeColor = Color.SeaGreen, Margin = new Padding(5, 5, 0, 5) };
            lblC1Temp = new Label { Text = "🌡️ -- °C", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Meteo = new Label { Text = "🌧️ -- mm  |  💨 -- km/h", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Pol = new Label { Text = "🏭 -- µg/m³", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Critici = new Label { Text = "⚠️ -- Critici", Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 2, 0, 2) };
            lblC1Aqi = new Label { Text = "Indice AQI: --", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(5, 2, 0, 2) };

            // Etichette Città 2
            lblC2Nome = new Label { Text = "Città 2", Font = fontNome, AutoSize = true, ForeColor = Color.OrangeRed, Margin = new Padding(5, 5, 0, 5) };
            lblC2Temp = new Label { Text = "🌡️ -- °C", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Meteo = new Label { Text = "🌧️ -- mm  |  💨 -- km/h", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Pol = new Label { Text = "🏭 -- µg/m³", Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Critici = new Label { Text = "⚠️ -- Critici", Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 2, 0, 2) };
            lblC2Aqi = new Label { Text = "Indice AQI: --", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(5, 2, 0, 2) };

            // Aggiunta alla Tabella KPI
            pnlKPI.Controls.Add(lblC1Nome, 0, 0); pnlKPI.Controls.Add(lblC2Nome, 1, 0);
            pnlKPI.Controls.Add(lblC1Temp, 0, 1); pnlKPI.Controls.Add(lblC2Temp, 1, 1);
            pnlKPI.Controls.Add(lblC1Meteo, 0, 2); pnlKPI.Controls.Add(lblC2Meteo, 1, 2);
            pnlKPI.Controls.Add(lblC1Pol, 0, 3); pnlKPI.Controls.Add(lblC2Pol, 1, 3);
            pnlKPI.Controls.Add(lblC1Critici, 0, 4); pnlKPI.Controls.Add(lblC2Critici, 1, 4);
            pnlKPI.Controls.Add(lblC1Aqi, 0, 5); pnlKPI.Controls.Add(lblC2Aqi, 1, 5);
            //4 aree dei grafici e dati
            Padding cardMargin = new Padding(10);
            Panel card1 = new Panel { Name = "Card1", Dock = DockStyle.Fill, Margin = cardMargin }; card1.Controls.Add(chartTemporale); tblMainLayout.Controls.Add(card1, 0, 0);
            Panel card2 = new Panel { Name = "Card2", Dock = DockStyle.Fill, Margin = cardMargin }; card2.Controls.Add(chartCorrelazione); tblMainLayout.Controls.Add(card2, 0, 1);
            Panel card3 = new Panel { Name = "Card3", Dock = DockStyle.Fill, Margin = cardMargin, Padding = new Padding(10) }; card3.Controls.Add(gridDati); tblMainLayout.Controls.Add(card3, 1, 0);
            Panel card4 = new Panel { Name = "Card4", Dock = DockStyle.Fill, Margin = cardMargin, Padding = new Padding(10) }; card4.Controls.Add(pnlKPI); tblMainLayout.Controls.Add(card4, 1, 1);
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
            lblC1Nome.ForeColor = ThemeManager.IsDarkMode ? Color.SpringGreen : Color.SeaGreen;
            lblC2Nome.ForeColor = ThemeManager.IsDarkMode ? Color.Tomato : Color.OrangeRed;
            if (chartTemporale.Series.Count > 0) btnAnalizza.PerformClick();
        }

        private void CmbVista_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVista.SelectedIndex == 0)
            {
                tblMainLayout.RowStyles[0] = new RowStyle(SizeType.Percent, 50F);
                tblMainLayout.RowStyles[1] = new RowStyle(SizeType.Percent, 50F);
            }
            else if (cmbVista.SelectedIndex == 1)
            {
                tblMainLayout.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
                tblMainLayout.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
            }
            else
            {
                tblMainLayout.RowStyles[0] = new RowStyle(SizeType.Percent, 0F);
                tblMainLayout.RowStyles[1] = new RowStyle(SizeType.Percent, 100F);
            }
        }

        private async void BtnAnalizza_Click(object sender, EventArgs e)
        {
            try
            {
                btnAnalizza.Enabled = false;
                btnAnalizza.Text = "Elaborazione...";

                string c1 = cmbCitta1.SelectedItem.ToString();
                string c2 = cmbCitta2.SelectedItem.ToString();
                string inqSelezionato = cmbInquinante.SelectedItem.ToString();

                var datiC1 = await OttieniDatiIntelligente(c1);
                var datiC2 = await OttieniDatiIntelligente(c2);

                var filtriC1 = datiC1.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();
                var filtriC2 = datiC2.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();

                gridDati.DataSource = filtriC1;

                AggiornaGrafici(filtriC1, c1, filtriC2, c2, inqSelezionato);
                AggiornaKPI(filtriC1, c1, filtriC2, c2, inqSelezionato);
            }
            catch (Exception ex) { MessageBox.Show("Errore: " + ex.Message); }
            finally { btnAnalizza.Enabled = true; btnAnalizza.Text = "Esegui Analisi"; }
        }

        private async Task<List<DatoAmbientale>> OttieniDatiIntelligente(string citta)
        {
            var dati = _storage.CaricaDati(citta);
            if (dati.Count == 0 || dati.Last().Data < DateTime.Today.AddDays(-3))
            {
                ApiService api = new ApiService();
                var coord = _dizionarioCitta[citta];
                dati = await api.ScaricaDatiAsync(coord.Lat, coord.Lon);
                _storage.SalvaDati(citta, dati);
            }
            return dati;
        }

        private void AggiornaGrafici(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2, string inquinante)
        {
            chartTemporale.Series.Clear();
            chartCorrelazione.Series.Clear();

            Color col1 = ThemeManager.IsDarkMode ? Color.SpringGreen : Color.SeaGreen;
            Color col2 = ThemeManager.IsDarkMode ? Color.Tomato : Color.OrangeRed;

            // Asse Y (Inquinante)
            Func<DatoAmbientale, double> selY;
            if (inquinante == "PM2.5") selY = d => d.PM25;
            else if (inquinante == "NO2") selY = d => d.NO2;
            else selY = d => d.PM10;

            // Asse X (Variabile Meteo selezionata)
            string varMeteo = cmbCorrelazioneX.SelectedItem.ToString();
            Func<DatoAmbientale, double> selX;
            string unitaMisuraX = "°C";

            if (varMeteo == "Precipitazioni") { selX = d => d.PrecipitazioniTotali; unitaMisuraX = "mm"; }
            else if (varMeteo == "Vento") { selX = d => d.VentoMedia; unitaMisuraX = "km/h"; }
            else { selX = d => d.TemperaturaMedia; unitaMisuraX = "°C"; }

            // --- GRAFICO TEMPORALE ---
            chartTemporale.Titles[0].Text = $"Andamento {inquinante} nel Tempo";
            Series s1Temp = new Series(c1) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col1 };
            Series s2Temp = new Series(c2) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col2 };

            foreach (var d in d1) s1Temp.Points.AddXY(d.Data, selY(d));
            foreach (var d in d2) s2Temp.Points.AddXY(d.Data, selY(d));

            chartTemporale.Series.Add(s1Temp);
            chartTemporale.Series.Add(s2Temp);
            chartTemporale.ChartAreas[0].AxisX.Title = "Data";
            chartTemporale.ChartAreas[0].AxisY.Title = $"{inquinante} (µg/m³)";

            // --- GRAFICO CORRELAZIONE DINAMICO ---
            chartCorrelazione.Titles[0].Text = $"Correlazione {varMeteo} vs {inquinante}";
            Series s1Corr = new Series(c1) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col1 };
            Series s2Corr = new Series(c2) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col2 };

            // Disegna usando selX per l'asse orizzontale e selY per il verticale
            foreach (var d in d1) { if (selX(d) > 0 || varMeteo == "Temperatura") s1Corr.Points.AddXY(selX(d), selY(d)); }
            foreach (var d in d2) { if (selX(d) > 0 || varMeteo == "Temperatura") s2Corr.Points.AddXY(selX(d), selY(d)); }

            chartCorrelazione.Series.Add(s1Corr);
            chartCorrelazione.Series.Add(s2Corr);
            chartCorrelazione.ChartAreas[0].AxisX.Title = $"{varMeteo} ({unitaMisuraX})";
            chartCorrelazione.ChartAreas[0].AxisY.Title = $"{inquinante} (µg/m³)";
        }

        private void AggiornaKPI(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2, string inquinante)
        {
            // Il backend si occupa di tutto!
            StatisticheReport rep1 = _analizzatore.CalcolaStatistiche(d1, c1, inquinante);
            StatisticheReport rep2 = _analizzatore.CalcolaStatistiche(d2, c2, inquinante);

            lblC1Nome.Text = rep1.Citta;
            if (d1.Count > 0)
            {
                lblC1Temp.Text = $"🌡️ Temp Media: {rep1.TempMedia} °C";
                lblC1Meteo.Text = $"🌧️ Pioggia Tot: {rep1.PrecipitazioniTotali} mm \n💨 Vento: {rep1.VentoMedio} km/h";
                lblC1Pol.Text = $"🏭 {inquinante} Medio: {rep1.InquinanteMedio} µg/m³";
                lblC1Critici.Text = $"⚠️ {rep1.GiorniCritici} Giorni Fuori Limite";

                // MOSTRA DIRETTAMENTE IL TESTO GENERATO DAL BACKEND (Es: "Moderata (🟡)")
                lblC1Aqi.Text = $"Qualità Media: {rep1.QualitaAriaAqi}";
            }

            lblC2Nome.Text = rep2.Citta;
            if (d2.Count > 0)
            {
                lblC2Temp.Text = $"🌡️ Temp Media: {rep2.TempMedia} °C";
                lblC2Meteo.Text = $"🌧️ Pioggia Tot: {rep2.PrecipitazioniTotali} mm \n💨 Vento: {rep2.VentoMedio} km/h";
                lblC2Pol.Text = $"🏭 {inquinante} Medio: {rep2.InquinanteMedio} µg/m³";
                lblC2Critici.Text = $"⚠️ {rep2.GiorniCritici} Giorni Fuori Limite";
                lblC2Aqi.Text = $"Qualità Media: {rep2.QualitaAriaAqi}";
            }
        }
        private void ScegliDaMappa(ComboBox targetCombo)
        {
            using (FormMappa frmMappa = new FormMappa())
            {
                if (frmMappa.ShowDialog() == DialogResult.OK && frmMappa.PuntoSelezionato)
                {
                    double lat = Math.Round(double.Parse(frmMappa.LatitudineScelta, System.Globalization.CultureInfo.InvariantCulture), 2);
                    double lon = Math.Round(double.Parse(frmMappa.LongitudineScelta, System.Globalization.CultureInfo.InvariantCulture), 2);

                    var risultato = ChiediNomeCitta(lat, lon);
                    string nomeVisualizzato = string.IsNullOrWhiteSpace(risultato.nome)
                                              ? $"Lat: {lat}, Lon:{lon}"
                                              : risultato.nome;

                    // Aggiorniamo il dizionario (Sessione temporanea)
                    if (!_dizionarioCitta.ContainsKey(nomeVisualizzato))
                    {
                        _dizionarioCitta[nomeVisualizzato] = (lat.ToString(), lon.ToString());
                    }

                    // SALVATAGGIO PERMANENTE: Solo se l'utente ha scritto un nome ed è reale
                    if (risultato.salva && !string.IsNullOrWhiteSpace(risultato.nome))
                    {
                        _storage.SalvaListaCitta(_dizionarioCitta);
                    }

                    // Aggiorniamo le tendine
                    string selected1 = cmbCitta1.SelectedItem?.ToString();
                    string selected2 = cmbCitta2.SelectedItem?.ToString();

                    cmbCitta1.DataSource = new BindingSource(_dizionarioCitta.Keys, null);
                    cmbCitta2.DataSource = new BindingSource(_dizionarioCitta.Keys, null);

                    cmbCitta1.SelectedItem = (targetCombo == cmbCitta1) ? nomeVisualizzato : selected1;
                    cmbCitta2.SelectedItem = (targetCombo == cmbCitta2) ? nomeVisualizzato : selected2;
                }
            }
        }
        // Metodo per creare una finestrella di dialogo personalizzata
        private (string nome, bool salva) ChiediNomeCitta(double lat, double lon)
        {
            Form prompt = new Form() { Width = 400, Height = 200, Text = "Opzioni Punto", StartPosition = FormStartPosition.CenterParent };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
            Button btnSalva = new Button() { Text = "Salva nei Preferiti", Left = 20, Top = 100, Width = 160 };
            Button btnSoloOra = new Button() { Text = "Usa solo ora", Left = 190, Top = 100, Width = 160 };

            string nomeInput = "";
            bool deveSalvare = false;

            btnSalva.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Inserisci un nome per salvare!");
                }
                else
                {
                    nomeInput = textBox.Text; deveSalvare = true; prompt.DialogResult = DialogResult.OK; prompt.Close();
                }
            };

            btnSoloOra.Click += (s, e) => {
                nomeInput = textBox.Text; deveSalvare = false; prompt.DialogResult = DialogResult.OK; prompt.Close();
            };

            prompt.Controls.AddRange(new Control[] { new Label { Text = "Nome (opzionale):", Left = 20, Top = 30 }, textBox, btnSalva, btnSoloOra });
            prompt.ShowDialog();

            return (nomeInput, deveSalvare);
        }
        private void PulisciFileTemporanei()
        {
            // Prendi tutti i file che iniziano con "dati_"
            string[] fileTemporanei = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "dati_*.json");

            // Prendi i nomi delle città salvate nel file dei preferiti
            var preferiti = _storage.CaricaListaCitta().Keys.Select(k => $"dati_{new string(k.Where(char.IsLetterOrDigit).ToArray()).ToLower()}.json").ToList();

            foreach (var file in fileTemporanei)
            {
                string nomeFile = Path.GetFileName(file);

                // Se il file non è nei preferiti, eliminalo!
                // (Assicurati che non elimini il file delle città preferite stesso!)
                if (!preferiti.Contains(nomeFile) && nomeFile != "citta_preferite.json")
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
    }
}