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
    // =======================================================
    // NUOVA CLASSE: IL BOTTONE ROTONDO
    // =======================================================
    public class RoundButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath grPath = new GraphicsPath();
            grPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            this.Region = new System.Drawing.Region(grPath);
            base.OnPaint(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }

    // =======================================================
    // FORM PRINCIPALE
    // =======================================================
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
        private RoundButton btnMappa1;
        private RoundButton btnMappa2;

        private StorageService _storage = new StorageService();

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

            flwFiltri = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15, 10, 0, 0), AutoSize = true };
            pnlTopBar.Controls.Add(flwFiltri);

            cmbCitta1 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCitta2 = new ComboBox { Width = 110, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };

            cmbInquinante = new ComboBox { Width = 80, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbInquinante.Items.AddRange(new string[] { "PM10", "PM2.5", "NO2" });
            cmbInquinante.SelectedIndex = 0;

            cmbVista = new ComboBox { Width = 130, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVista.Items.AddRange(new string[] { "Doppio Grafico", "Solo Andamento", "Solo Correlazione" });
            cmbVista.SelectedIndex = 0;
            cmbVista.SelectedIndexChanged += CmbVista_SelectedIndexChanged;

            dtpInizio = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            dtpFine = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100 };
            btnAnalizza = new Button { Text = "Esegui Analisi", Width = 110, Height = 28, Cursor = Cursors.Hand };
            btnTema = new Button { Text = "Tema", Width = 60, Height = 28, Cursor = Cursors.Hand };

            // --- BOTTONI ROTONDI --- (Larghezza e Altezza devono essere uguali!)
            btnMappa1 = new RoundButton { Text = "📍", Width = 30, Height = 30, Cursor = Cursors.Hand, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMappa1.FlatAppearance.BorderSize = 0;
            btnMappa1.Click += (s, e) => ScegliDaMappa(cmbCitta1);

            btnMappa2 = new RoundButton { Text = "📍", Width = 30, Height = 30, Cursor = Cursors.Hand, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMappa2.FlatAppearance.BorderSize = 0;
            btnMappa2.Click += (s, e) => ScegliDaMappa(cmbCitta2);

            btnTema.Click += BtnTema_Click;
            btnAnalizza.Click += BtnAnalizza_Click;

            flwFiltri.Controls.Add(new Label { Text = "Punto 1:", AutoSize = true, Margin = new Padding(0, 5, 2, 0) });
            flwFiltri.Controls.Add(cmbCitta1);
            flwFiltri.Controls.Add(btnMappa1);

            flwFiltri.Controls.Add(new Label { Text = "VS Punto 2:", AutoSize = true, Margin = new Padding(15, 5, 2, 0) });
            flwFiltri.Controls.Add(cmbCitta2);
            flwFiltri.Controls.Add(btnMappa2);

            flwFiltri.Controls.Add(new Label { Text = "Inquinante:", AutoSize = true, Margin = new Padding(15, 5, 2, 0) });
            flwFiltri.Controls.Add(cmbInquinante);
            flwFiltri.Controls.Add(new Label { Text = "Dal:", AutoSize = true, Margin = new Padding(15, 5, 2, 0) });
            flwFiltri.Controls.Add(dtpInizio);
            flwFiltri.Controls.Add(new Label { Text = "Al:", AutoSize = true, Margin = new Padding(5, 5, 2, 0) });
            flwFiltri.Controls.Add(dtpFine);
            flwFiltri.Controls.Add(new Label { Text = "Vista:", AutoSize = true, Margin = new Padding(15, 5, 2, 0) });
            flwFiltri.Controls.Add(cmbVista);
            flwFiltri.Controls.Add(new Label { Width = 10 });
            flwFiltri.Controls.Add(btnAnalizza);
            flwFiltri.Controls.Add(btnTema);

            chartTemporale = CreaGraficoBase("Andamento nel Tempo");
            chartCorrelazione = CreaGraficoBase("Correlazione Temperatura / Inquinante");

            gridDati = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            pnlKPI = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, BackColor = Color.Transparent };
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Font fontNome = new Font("Segoe UI", 12, FontStyle.Bold);
            Font fontVal = new Font("Segoe UI", 11, FontStyle.Regular);

            lblC1Nome = new Label { Text = "Città 1", Font = fontNome, AutoSize = true, ForeColor = Color.SeaGreen, Margin = new Padding(5, 10, 0, 10) };
            lblC1Temp = new Label { Text = "🌡️ -- °C", Font = fontVal, AutoSize = true, Margin = new Padding(5, 5, 0, 5) };
            lblC1Pol = new Label { Text = "🏭 -- µg/m³", Font = fontVal, AutoSize = true, Margin = new Padding(5, 5, 0, 5) };
            lblC1Critici = new Label { Text = "⚠️ -- Giorni Critici", Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 5, 0, 5) };

            lblC2Nome = new Label { Text = "Città 2", Font = fontNome, AutoSize = true, ForeColor = Color.OrangeRed, Margin = new Padding(5, 10, 0, 10) };
            lblC2Temp = new Label { Text = "🌡️ -- °C", Font = fontVal, AutoSize = true, Margin = new Padding(5, 5, 0, 5) };
            lblC2Pol = new Label { Text = "🏭 -- µg/m³", Font = fontVal, AutoSize = true, Margin = new Padding(5, 5, 0, 5) };
            lblC2Critici = new Label { Text = "⚠️ -- Giorni Critici", Font = fontVal, AutoSize = true, ForeColor = Color.Tomato, Margin = new Padding(5, 5, 0, 5) };

            pnlKPI.Controls.Add(lblC1Nome, 0, 0); pnlKPI.Controls.Add(lblC2Nome, 1, 0);
            pnlKPI.Controls.Add(lblC1Temp, 0, 1); pnlKPI.Controls.Add(lblC2Temp, 1, 1);
            pnlKPI.Controls.Add(lblC1Pol, 0, 2); pnlKPI.Controls.Add(lblC2Pol, 1, 2);
            pnlKPI.Controls.Add(lblC1Critici, 0, 3); pnlKPI.Controls.Add(lblC2Critici, 1, 3);

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

            Func<DatoAmbientale, double> selettore;
            if (inquinante == "PM2.5") selettore = d => d.PM25;
            else if (inquinante == "NO2") selettore = d => d.NO2;
            else selettore = d => d.PM10;

            chartTemporale.Titles[0].Text = $"Andamento {inquinante} nel Tempo";
            Series s1Temp = new Series(c1) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col1 };
            Series s2Temp = new Series(c2) { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = col2 };

            foreach (var d in d1) s1Temp.Points.AddXY(d.Data, selettore(d));
            foreach (var d in d2) s2Temp.Points.AddXY(d.Data, selettore(d));

            chartTemporale.Series.Add(s1Temp);
            chartTemporale.Series.Add(s2Temp);
            chartTemporale.ChartAreas[0].AxisX.Title = "Data";
            chartTemporale.ChartAreas[0].AxisY.Title = $"{inquinante} (µg/m³)";

            chartCorrelazione.Titles[0].Text = $"Correlazione Temperatura vs {inquinante}";
            Series s1Corr = new Series(c1) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col1 };
            Series s2Corr = new Series(c2) { ChartType = SeriesChartType.Point, MarkerSize = 5, Color = col2 };

            foreach (var d in d1) s1Corr.Points.AddXY(d.TemperaturaMedia, selettore(d));
            foreach (var d in d2) s2Corr.Points.AddXY(d.TemperaturaMedia, selettore(d));

            chartCorrelazione.Series.Add(s1Corr);
            chartCorrelazione.Series.Add(s2Corr);
            chartCorrelazione.ChartAreas[0].AxisX.Title = "Temperatura (°C)";
            chartCorrelazione.ChartAreas[0].AxisY.Title = $"{inquinante} (µg/m³)";
        }

        private void AggiornaKPI(List<DatoAmbientale> d1, string c1, List<DatoAmbientale> d2, string c2, string inquinante)
        {
            int limite = 50;
            if (inquinante == "PM2.5") limite = 25;
            if (inquinante == "NO2") limite = 40;

            Func<DatoAmbientale, double> sel;
            if (inquinante == "PM2.5") sel = d => d.PM25;
            else if (inquinante == "NO2") sel = d => d.NO2;
            else sel = d => d.PM10;

            lblC1Nome.Text = c1;
            if (d1.Count > 0)
            {
                lblC1Temp.Text = $"🌡️ {d1.Average(d => d.TemperaturaMedia):F1} °C";
                lblC1Pol.Text = $"🏭 {d1.Average(sel):F1} µg/m³";
                lblC1Critici.Text = $"⚠️ {d1.Count(d => sel(d) > limite)} Giorni Critici";
            }

            lblC2Nome.Text = c2;
            if (d2.Count > 0)
            {
                lblC2Temp.Text = $"🌡️ {d2.Average(d => d.TemperaturaMedia):F1} °C";
                lblC2Pol.Text = $"🏭 {d2.Average(sel):F1} µg/m³";
                lblC2Critici.Text = $"⚠️ {d2.Count(d => sel(d) > limite)} Giorni Critici";
            }
        }

        // =========================================================================
        // NUOVO: LOGICA MAPPA E SALVATAGGIO PREFERITI
        // =========================================================================
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