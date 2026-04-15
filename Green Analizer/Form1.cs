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
        // SERVIZI E LOGICA BACKEND
        private StorageService _storage = new StorageService();
        private AnalysisService _analizzatore = new AnalysisService();

        // Dizionario di base che attraverso il ServiceStorage aggiuge le posizioni preferite
        private Dictionary<string, (string Lat, string Lon)> _dizionarioCitta = new Dictionary<string, (string, string)>
        {
            { "Vicenza", ("45.5467", "11.5475") },
            { "Milano", ("45.4642", "9.1900") },
            { "Roma", ("41.9028", "12.4964") },
            { "Napoli", ("40.8518", "14.2681") },
            { "Torino", ("45.0703", "7.6869") }
        };

        //contenitori principali
        private Panel pnlTopBar;
        private TableLayoutPanel tblMainLayout;
        private TableLayoutPanel pnlKPI;

        //Elementi per il controlli Utente
        private ComboBox cmbCitta1;
        private ComboBox cmbCitta2;
        private ComboBox cmbInquinante;
        private ComboBox cmbVista;
        private ComboBox cmbCorrelazioneX;
        private DateTimePicker dtpInizio;
        private DateTimePicker dtpFine;
        private Button btnAnalizza;
        private Button btnTema;
        private IconHoverButton btnMappa1;
        private IconHoverButton btnMappa2;

        //Elementi grafici
        private Chart chartTemporale;
        private Chart chartCorrelazione;
        private DataGridView gridDati1;
        private DataGridView gridDati2;
        private DataGridView gridKpi1;
        private DataGridView gridKpi2;
        private Label lblTitoloGrid1;
        private Label lblTitoloGrid2;

        //Vari Label e Tooltip AQI
        private ToolTip ttInfo;
        private Label lblInfo;
        private Label lblC1Nome, lblC1Temp, lblC1Pol, lblC1Critici, lblC1Meteo, lblC1Aqi;
        private Label lblC2Nome, lblC2Temp, lblC2Pol, lblC2Critici, lblC2Meteo, lblC2Aqi;


        public Form1()
        {
            InitializeComponent();
            CaricaCittaSalvate(); // Carica le posizioni preferite salvati in precedenza
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
        // Construisce tutta la struttura grafica del programma
        private void CostruisciInterfaccia()
        {
            // imposta tutti i elementi grafici nodo corretto
            this.Text = "Green Analyzer - Dashboard Ambientale";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 768);
            //Panel principale che contiene tutti i elementi grafici.
            //Questo oggetto poi viene passato come parametro al ThemeManager per impostare correttamnete il tema.
            tblMainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, CellBorderStyle = TableLayoutPanelCellBorderStyle.None, ColumnCount = 2, RowCount = 2 };
            tblMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tblMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tblMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.Controls.Add(tblMainLayout);
            //Panello superiore che tutti parametri per l'analisi dati
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

            //Creazione di tutti i elementi della top bar
            //ComboXox per selezionare le posizioni da comparare
            cmbCitta1 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCitta2 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            //ComboBox per selezionare il tipo di valore inquinante analizzare
            cmbInquinante = new ComboBox { Width = 80, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbInquinante.Items.AddRange(new string[] { "PM10", "PM2.5", "NO2", "AQI (Generale)" });
            cmbInquinante.SelectedIndex = 0;
            //ToolTip per visualizzare tabella dell'AQI
            ToolTip ttInfo = new ToolTip { AutoPopDelay = 20000, InitialDelay = 200, ReshowDelay = 200, IsBalloon = true, ToolTipTitle = "Soglie Ufficiali Qualità Aria (µg/m³)", ToolTipIcon = ToolTipIcon.Info };
            string testoTabella =
                "--- PM2.5 (Medie 24h) ---\n" +
                "Buono: 0-10 | Discreto: 10-20 | Moderato: 20-25\nScadente: 25-50 | Molto Scadente: 50-75 | Pessimo: > 75\n\n" +
                "--- PM10 (Medie 24h) ---\n" +
                "Buono: 0-20 | Discreto: 20-40 | Moderato: 40-50\nScadente: 50-100 | Molto Scadente: 100-150 | Pessimo: > 150\n\n" +
                "--- NO2 (Medie Orarie) ---\n" +
                "Buono: 0-40 | Discreto: 40-90 | Moderato: 90-120\nScadente: 120-230 | Molto Scadente: 230-340 | Pessimo: > 340";
            //Label per il ToolTip
            Label lblInfo = new Label { Text = "ℹ️", AutoSize = true, Cursor = Cursors.Help, Font = new Font("Segoe UI Emoji", 14) };
            ttInfo.SetToolTip(lblInfo, testoTabella);
            //ComboBox per selezionare con quale valore vogliamo fare una correllazione alla temperatura
            cmbCorrelazioneX = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCorrelazioneX.Items.AddRange(new string[] { "Temperatura", "Precipitazioni", "Vento" });
            cmbCorrelazioneX.SelectedIndex = 0;
            //ComboBox per selezionare come visualizzare i grafici
            cmbVista = new ComboBox { Width = 130, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVista.Items.AddRange(new string[] { "Doppio Grafico", "Solo Andamento", "Solo Correlazione" });
            cmbVista.SelectedIndex = 0;
            cmbVista.SelectedIndexChanged += CmbVista_SelectedIndexChanged;
            //DateTimePicker per selezionare il lasso di tempo da analizzare
            dtpInizio = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            dtpFine = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            btnAnalizza = new Button { Text = "Esegui Analisi", Width = 110, Height = 28, Cursor = Cursors.Hand };
            btnTema = new Button { Text = "Tema", Width = 60, Height = 28, Cursor = Cursors.Hand };

            //Pulsanti per aprire la mappa e selezionare nuove posizioni
            btnMappa1 = new IconHoverButton { Text = "🌍" };
            btnMappa1.Click += (s, e) => ScegliDaMappa(cmbCitta1);

            btnMappa2 = new IconHoverButton { Text = "🌍" };
            btnMappa2.Click += (s, e) => ScegliDaMappa(cmbCitta2);

            btnTema.Click += BtnTema_Click;
            btnAnalizza.Click += BtnAnalizza_Click;

            // Inserimento parte superiore (uso dell funzione AddToTop interna per allineare tutto)
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

            AddToTop(btnAnalizza, 17);
            AddToTop(btnTema, 18);

            // Parte centrale con i 2 grafici: quello in relazione al tempo e quelo in relazione alla temperatura
            chartTemporale = CreaGraficoBase("Andamento nel Tempo");
            chartCorrelazione = CreaGraficoBase("Correlazione Temperatura / Inquinante");
            //card 3
            gridDati1 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells };
            gridDati2 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells };

            lblTitoloGrid1 = new Label { Text = "Dati Città 1", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 0, 0, 5) };
            lblTitoloGrid2 = new Label { Text = "Dati Città 2", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 0, 0, 5) };

            TableLayoutPanel tblGriglie = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tblGriglie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblGriglie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblGriglie.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblGriglie.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tblGriglie.Controls.Add(lblTitoloGrid1, 0, 0); tblGriglie.Controls.Add(lblTitoloGrid2, 1, 0);
            tblGriglie.Controls.Add(gridDati1, 0, 1); tblGriglie.Controls.Add(gridDati2, 1, 1);

            // CARD 4: KPI E TABELLE DI DISTRIBUZIONE
            TableLayoutPanel pnlKPIContainer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            pnlKPIContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 45F)); // Spazio ai KPI testo
            pnlKPIContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 55F)); // Spazio alle tabelle

            pnlKPI = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, BackColor = Color.Transparent };
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Font fontNome = new Font("Segoe UI", 12, FontStyle.Bold);
            Font fontVal = new Font("Segoe UI", 10, FontStyle.Regular);

            lblC1Nome = new Label { Font = fontNome, AutoSize = true, ForeColor = Color.SeaGreen, Margin = new Padding(5, 5, 0, 5) };
            lblC1Temp = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Meteo = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Pol = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC1Critici = new Label { Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 2, 0, 2) };
            lblC1Aqi = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(5, 2, 0, 2) };

            lblC2Nome = new Label { Font = fontNome, AutoSize = true, ForeColor = Color.OrangeRed, Margin = new Padding(5, 5, 0, 5) };
            lblC2Temp = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Meteo = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Pol = new Label { Font = fontVal, AutoSize = true, Margin = new Padding(5, 2, 0, 2) };
            lblC2Critici = new Label { Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 2, 0, 2) };
            lblC2Aqi = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(5, 2, 0, 2) };

            pnlKPI.Controls.Add(lblC1Nome, 0, 0); pnlKPI.Controls.Add(lblC2Nome, 1, 0);
            pnlKPI.Controls.Add(lblC1Temp, 0, 1); pnlKPI.Controls.Add(lblC2Temp, 1, 1);
            pnlKPI.Controls.Add(lblC1Meteo, 0, 2); pnlKPI.Controls.Add(lblC2Meteo, 1, 2);
            pnlKPI.Controls.Add(lblC1Pol, 0, 3); pnlKPI.Controls.Add(lblC2Pol, 1, 3);
            pnlKPI.Controls.Add(lblC1Critici, 0, 4); pnlKPI.Controls.Add(lblC2Critici, 1, 4);
            pnlKPI.Controls.Add(lblC1Aqi, 0, 5); pnlKPI.Controls.Add(lblC2Aqi, 1, 5);

            gridKpi1 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridKpi2 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            TableLayoutPanel tblKpiGrids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tblKpiGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblKpiGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblKpiGrids.Controls.Add(gridKpi1, 0, 0);
            tblKpiGrids.Controls.Add(gridKpi2, 1, 0);

            pnlKPIContainer.Controls.Add(pnlKPI, 0, 0);
            pnlKPIContainer.Controls.Add(tblKpiGrids, 0, 1);

            // unione dei vari pannella cards
            Padding cardMargin = new Padding(10);
            Panel card1 = new Panel { Name = "Card1", Dock = DockStyle.Fill, Margin = cardMargin }; card1.Controls.Add(chartTemporale); tblMainLayout.Controls.Add(card1, 0, 0);
            Panel card2 = new Panel { Name = "Card2", Dock = DockStyle.Fill, Margin = cardMargin }; card2.Controls.Add(chartCorrelazione); tblMainLayout.Controls.Add(card2, 0, 1);
            Panel card3 = new Panel { Name = "Card3", Dock = DockStyle.Fill, Margin = cardMargin, Padding = new Padding(10) }; card3.Controls.Add(tblGriglie); tblMainLayout.Controls.Add(card3, 1, 0);
            Panel card4 = new Panel { Name = "Card4", Dock = DockStyle.Fill, Margin = cardMargin, Padding = new Padding(10) }; card4.Controls.Add(pnlKPIContainer); tblMainLayout.Controls.Add(card4, 1, 1);

            tblMainLayout.Controls.Add(card1, 0, 0);
            tblMainLayout.Controls.Add(card2, 0, 1);
            tblMainLayout.Controls.Add(card3, 1, 0);
            tblMainLayout.Controls.Add(card4, 1, 1);
        }
        //metodo per inizializare i grafici
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
                btnAnalizza.Enabled = false; btnAnalizza.Text = "Elaborazione...";

                string c1 = cmbCitta1.SelectedItem.ToString();
                string c2 = cmbCitta2.SelectedItem.ToString();
                string inqSelezionato = cmbInquinante.SelectedItem.ToString();

                var datiC1 = await OttieniDatiIntelligente(c1);
                var datiC2 = await OttieniDatiIntelligente(c2);

                var filtriC1 = datiC1.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();
                var filtriC2 = datiC2.Where(d => d.Data.Date >= dtpInizio.Value.Date && d.Data.Date <= dtpFine.Value.Date).ToList();

                //aggiornamneto delle 2 tabelle separatamente
                lblTitoloGrid1.Text = $"Dati {c1}";
                lblTitoloGrid2.Text = $"Dati {c2}";
                gridDati1.DataSource = filtriC1;
                gridDati2.DataSource = filtriC2;

                AggiornaGrafici(filtriC1, c1, filtriC2, c2, inqSelezionato);
                AggiornaKPI(filtriC1, c1, filtriC2, c2, inqSelezionato);
            }
            catch (Exception ex) { MessageBox.Show("Errore: " + ex.Message); }
            finally { btnAnalizza.Enabled = true; btnAnalizza.Text = "Esegui Analisi"; }
        }
        //Metodo che aggiorna i grafici
        private void AggiornaGrafici(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2, string inquinante)
        {
            chartTemporale.Series.Clear(); chartCorrelazione.Series.Clear();

            Color col1 = ThemeManager.IsDarkMode ? Color.SpringGreen : Color.SeaGreen;
            Color col2 = ThemeManager.IsDarkMode ? Color.Tomato : Color.OrangeRed;

            // Y = Inquinante Grezzo (µg/m³) o AQI Generale
            Func<DatoAmbientale, double> selY;
            string unitaY = "(µg/m³)";

            if (inquinante == "PM2.5") selY = d => d.PM25;
            else if (inquinante == "NO2") selY = d => d.NO2;
            else if (inquinante == "AQI (Generale)")
            {
                selY = d => Math.Max(d.AqiPm10 ?? 0, Math.Max(d.AqiPm25 ?? 0, d.AqiNo2 ?? 0));
                unitaY = "(Indice)";
            }
            else selY = d => d.PM10;

            // X = Variabile Meteo
            string varMeteo = cmbCorrelazioneX.SelectedItem.ToString();
            Func<DatoAmbientale, double> selX;
            string unitaMisuraX = "";

            if (varMeteo == "Precipitazioni") { selX = d => d.PrecipitazioniTotali; unitaMisuraX = "mm"; }
            else if (varMeteo == "Vento") { selX = d => d.VentoMedia; unitaMisuraX = "km/h"; }
            else { selX = d => d.TemperaturaMedia; unitaMisuraX = "°C"; }

            // Disegno Grafico Temporale
            chartTemporale.Titles[0].Text = $"Andamento {inquinante} nel Tempo";
            Series s1Temp = new Series(c1) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col1 };
            Series s2Temp = new Series(c2) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col2 };
            foreach (var d in d1) s1Temp.Points.AddXY(d.Data, selY(d));
            foreach (var d in d2) s2Temp.Points.AddXY(d.Data, selY(d));
            chartTemporale.Series.Add(s1Temp); chartTemporale.Series.Add(s2Temp);
            chartTemporale.ChartAreas[0].AxisY.Title = $"{inquinante} {unitaY}";

            // Disegno Grafico Correlazione
            chartCorrelazione.Titles[0].Text = $"Correlazione: {varMeteo} vs {inquinante}";
            Series s1Corr = new Series(c1) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col1 };
            Series s2Corr = new Series(c2) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col2 };
            foreach (var d in d1) { if (selX(d) > 0 || varMeteo == "Temperatura") s1Corr.Points.AddXY(selX(d), selY(d)); }
            foreach (var d in d2) { if (selX(d) > 0 || varMeteo == "Temperatura") s2Corr.Points.AddXY(selX(d), selY(d)); }
            chartCorrelazione.Series.Add(s1Corr); chartCorrelazione.Series.Add(s2Corr);
            chartCorrelazione.ChartAreas[0].AxisX.Title = $"{varMeteo} {unitaMisuraX}";
            chartCorrelazione.ChartAreas[0].AxisY.Title = $"{inquinante} {unitaY}";
            if (inquinante == "AQI (Generale)")
            {
                chartTemporale.ChartAreas[0].AxisY.Maximum = 150; // Range fisso per AQI
                chartTemporale.ChartAreas[0].AxisY.Minimum = 0;
            }
            else
            {
                chartTemporale.ChartAreas[0].AxisY.Maximum = double.NaN; // Auto-scaling per altri inquinanti
            }
        }
        //metodo che aggiorna la parte di rielaborazione dati
        private void AggiornaKPI(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2, string inquinante)
        {
            StatisticheReport rep1 = _analizzatore.CalcolaStatistiche(d1, c1, inquinante);
            StatisticheReport rep2 = _analizzatore.CalcolaStatistiche(d2, c2, inquinante);

            if (d1.Count > 0)
            {
                lblC1Nome.Text = rep1.Citta;
                lblC1Temp.Text = $"🌡️ Temp: {rep1.TempMedia}°C";
                lblC1Meteo.Text = $"🌧️ {rep1.PrecipitazioniTotali}mm | 💨 {rep1.VentoMedio}km/h";
                lblC1Pol.Text = $"🏭 {inquinante}: Med {rep1.InquinanteMedio} | Max {rep1.InquinanteMax}";
                lblC1Critici.Text = $"⚠️ {rep1.GiorniCritici} Giorni Critici (>{(inquinante == "PM2.5" ? 25 : (inquinante == "NO2" ? 120 : 50))})";
                lblC1Aqi.Text = $"🌍 AQI Medio: {rep1.AqiMedio} ({rep1.QualitaAriaAqi})";

                // Popola tabellina fasce C1
                gridKpi1.DataSource = new List<RigaKpi> {
            new RigaKpi { Pollutant = "PM10", Good=rep1.GiorniFascePM10[0], Fair=rep1.GiorniFascePM10[1], Mod=rep1.GiorniFascePM10[2], Poor=rep1.GiorniFascePM10[3], VPoor=rep1.GiorniFascePM10[4], EPoor=rep1.GiorniFascePM10[5] },
            new RigaKpi { Pollutant = "PM2.5", Good=rep1.GiorniFascePM25[0], Fair=rep1.GiorniFascePM25[1], Mod=rep1.GiorniFascePM25[2], Poor=rep1.GiorniFascePM25[3], VPoor=rep1.GiorniFascePM25[4], EPoor=rep1.GiorniFascePM25[5] },
            new RigaKpi { Pollutant = "NO2", Good=rep1.GiorniFasceNO2[0], Fair=rep1.GiorniFasceNO2[1], Mod=rep1.GiorniFasceNO2[2], Poor=rep1.GiorniFasceNO2[3], VPoor=rep1.GiorniFasceNO2[4], EPoor=rep1.GiorniFasceNO2[5] },
            new RigaKpi { Pollutant = "AQI Tot", Good=rep1.GiorniFasceAQI[0], Fair=rep1.GiorniFasceAQI[1], Mod=rep1.GiorniFasceAQI[2], Poor=rep1.GiorniFasceAQI[3], VPoor=rep1.GiorniFasceAQI[4], EPoor=rep1.GiorniFasceAQI[5] }
            }
            ;
            }

            if (d2.Count > 0)
            {
                lblC2Nome.Text = rep2.Citta;
                lblC2Temp.Text = $"🌡️ Temp: {rep2.TempMedia}°C";
                lblC2Meteo.Text = $"🌧️ {rep2.PrecipitazioniTotali}mm | 💨 {rep2.VentoMedio}km/h";
                lblC2Pol.Text = $"🏭 {inquinante}: Med {rep2.InquinanteMedio} | Max {rep2.InquinanteMax}";
                lblC2Critici.Text = $"⚠️ {rep2.GiorniCritici} Giorni Critici";
                lblC2Aqi.Text = $"🌍 AQI Medio: {rep2.AqiMedio} ({rep2.QualitaAriaAqi})";

                // Popola tabellina fasce C2
                gridKpi2.DataSource = new List<RigaKpi> {
            new RigaKpi { Pollutant = "PM10", Good=rep2.GiorniFascePM10[0], Fair=rep2.GiorniFascePM10[1], Mod=rep2.GiorniFascePM10[2], Poor=rep2.GiorniFascePM10[3], VPoor=rep2.GiorniFascePM10[4], EPoor=rep2.GiorniFascePM10[5] },
            new RigaKpi { Pollutant = "PM2.5", Good=rep2.GiorniFascePM25[0], Fair=rep2.GiorniFascePM25[1], Mod=rep2.GiorniFascePM25[2], Poor=rep2.GiorniFascePM25[3], VPoor=rep2.GiorniFascePM25[4], EPoor=rep2.GiorniFascePM25[5] },
            new RigaKpi { Pollutant = "NO2", Good=rep2.GiorniFasceNO2[0], Fair=rep2.GiorniFasceNO2[1], Mod=rep2.GiorniFasceNO2[2], Poor=rep2.GiorniFasceNO2[3], VPoor=rep2.GiorniFasceNO2[4], EPoor=rep2.GiorniFasceNO2[5] },
            new RigaKpi { Pollutant = "AQI Tot", Good=rep1.GiorniFasceAQI[0], Fair=rep1.GiorniFasceAQI[1], Mod=rep1.GiorniFasceAQI[2], Poor=rep1.GiorniFasceAQI[3], VPoor=rep1.GiorniFasceAQI[4], EPoor=rep1.GiorniFasceAQI[5] }
                };
            }
            ThemeManager.ColoraCelleGriglia(gridKpi1);
            ThemeManager.ColoraCelleGriglia(gridKpi2);
        }

        private async Task<List<DatoAmbientale>> OttieniDatiIntelligente(string citta)
        {
            //metodo per ritornare i dati dai file JSON
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

        //Metodo per scegliere le coordinate tramite mappa e aggiugerlo alle opzioni
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

                    // Salvataggio permanente
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
        // Metodo per creare una finestrella di dialogo per salvare le coordinate scelte
        private (string nome, bool salva) ChiediNomeCitta(double lat, double lon)
        {
            //metodo per salvare la posizione ai preferiti
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
    }
}