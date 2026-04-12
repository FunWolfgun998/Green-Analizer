using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Green_Analizer
{
    public static class ThemeManager
    {
        public static bool IsDarkMode = true;

        // Colori Dark: Contrasto netto. Bg è lo sfondo dietro, Panel è il riquadro chiaro.
        private static Color DarkBg = Color.FromArgb(20, 20, 30);       // Sfondo Form e spazi vuoti (Molto scuro)
        private static Color DarkPanel = Color.FromArgb(45, 45, 60);    // Sfondo dei 4 Quadrati (Più chiaro)
        private static Color DarkText = Color.FromArgb(248, 248, 242);  // Testo bianco sporco
        private static Color DarkAccent = Color.SeaGreen;               // Verde Green Economy

        // Colori Light
        private static Color LightBg = Color.FromArgb(230, 230, 230);
        private static Color LightPanel = Color.White;
        private static Color LightText = Color.FromArgb(50, 50, 50);
        private static Color LightAccent = Color.ForestGreen;

        public static void ApplicaTema(Form form)
        {
            Color bg = IsDarkMode ? DarkBg : LightBg;
            Color panelBg = IsDarkMode ? DarkPanel : LightPanel;
            Color text = IsDarkMode ? DarkText : LightText;
            Color accent = IsDarkMode ? DarkAccent : LightAccent;

            form.BackColor = bg;
            form.ForeColor = text;

            ApplicaAControlli(form.Controls, panelBg, text, accent, bg);
        }

        private static void ApplicaAControlli(Control.ControlCollection controlli, Color panelBg, Color text, Color accent, Color bg)
        {
            foreach (Control c in controlli)
            {
                if (c is IconHoverButton iconBtn)
                {
                    iconBtn.BackColor = Color.Transparent;
                    iconBtn.ForeColor = text; // Adatta il colore dell'emoji al tema
                    iconBtn.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    iconBtn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    iconBtn.FlatStyle = FlatStyle.Flat;
                    iconBtn.FlatAppearance.BorderSize = 0;
                    iconBtn.BackColor = Color.Transparent;
                }
                else if (c is TableLayoutPanel tlp)
                {
                    // La tabella DEVE essere trasparente o dello stesso colore del Form per far vedere gli spazi
                    tlp.BackColor = bg;
                    tlp.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                }
                else if (c is FlowLayoutPanel || (c is Panel && c.Name.StartsWith("Card")))
                {
                    // Queste sono le "Isole" (i 4 quadrati e la TopBar) -> Colore più chiaro
                    c.BackColor = panelBg;
                    c.ForeColor = text;
                    if (c is Panel pnl) pnl.BorderStyle = BorderStyle.None;
                }
                else if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;

                    if (btn.Name == "IconBtn")
                    {
                        btn.BackColor = Color.Transparent;
                        btn.ForeColor = text;
                    }
                    else // Altrimenti è un bottone normale (es. "Analizza Dati") e lo facciamo verde
                    {
                        btn.BackColor = accent;
                        btn.ForeColor = Color.White;
                    }
                }
                else if (c is ComboBox || c is DateTimePicker)
                {
                    c.BackColor = bg; // Sfondo elementi UI scuro per contrasto
                    c.ForeColor = text;
                }
                else if (c is DataGridView grid)
                {
                    grid.BorderStyle = BorderStyle.None;
                    grid.EnableHeadersVisualStyles = false;

                    // Sfondo generale della griglia uguale al quadrato
                    grid.BackgroundColor = panelBg;

                    // CELLE DELLA GRIGLIA PIU' SCURE (Come hai richiesto!)
                    grid.DefaultCellStyle.BackColor = bg;
                    grid.DefaultCellStyle.ForeColor = text;

                    // Header chiaro come la card
                    grid.ColumnHeadersDefaultCellStyle.BackColor = panelBg;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = text;
                    grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
                    grid.RowHeadersVisible = false;

                    // Colore alternato per le righe
                    int r = Math.Min(bg.R + 5, 255);
                    int g = Math.Min(bg.G + 5, 255);
                    int b = Math.Min(bg.B + 5, 255);
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(r, g, b);
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = text;
                }
                else if (c is Chart chart)
                {
                    chart.BorderlineDashStyle = ChartDashStyle.NotSet;
                    chart.BackColor = panelBg; // Grafico con sfondo uguale alla card
                    ApplicaTemaGrafico(chart, panelBg, text);
                }

                c.Invalidate();
                c.Refresh();

                if (c.HasChildren)
                {
                    ApplicaAControlli(c.Controls, panelBg, text, accent, bg);
                }
            }
        }

        private static void ApplicaTemaGrafico(Chart chart, Color bg, Color text)
        {
            foreach (var title in chart.Titles)
            {
                title.ForeColor = text;
            }
            if (chart.Legends.Count > 0)
            {
                chart.Legends[0].BackColor = bg;
                chart.Legends[0].ForeColor = text;
            }

            foreach (var area in chart.ChartAreas)
            {
                area.BackColor = bg;
                area.AxisX.LabelStyle.ForeColor = text;
                area.AxisY.LabelStyle.ForeColor = text;
                area.AxisX.LineColor = text;
                area.AxisY.LineColor = text;
                area.AxisX.MajorGrid.LineColor = Color.FromArgb(30, text);
                area.AxisY.MajorGrid.LineColor = Color.FromArgb(30, text);
            }
        }
    }
}