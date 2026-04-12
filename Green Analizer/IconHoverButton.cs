using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Green_Analizer
{
    public class IconHoverButton : Button
{
    private bool isHovered = false;

    public IconHoverButton()
    {
        this.DoubleBuffered = true;
        this.Cursor = Cursors.Hand;
        this.FlatStyle = FlatStyle.Flat;
        this.FlatAppearance.BorderSize = 0; // Fondamentale per eliminare il quadrato
        this.FlatAppearance.MouseOverBackColor = Color.Transparent;
        this.FlatAppearance.MouseDownBackColor = Color.Transparent;
        this.BackColor = Color.Transparent; // Trasparente
        this.Size = new Size(35, 30);
        this.Font = new Font("Segoe UI Emoji", 18);
    }

    protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 1. Sfondo trasparente
            if (Parent != null)
            {
                using (SolidBrush parentBrush = new SolidBrush(Parent.BackColor))
                {
                    e.Graphics.FillRectangle(parentBrush, ClientRectangle);
                }
            }

            // 2. Disegna il cerchio (Il segreto è usare Width e Height uguali)
            if (isHovered)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Definiamo un margine interno (es. 4 pixel)
                int margin = 4;
                // Calcoliamo un diametro che mantenga il margine
                int diameter = Math.Min(Width, Height) - (margin * 2);
                // Calcoliamo la posizione per centrare il cerchio
                int x = (Width - diameter) / 2;
                int y = (Height - diameter) / 2;

                using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(60, 200, 200, 200)))
                {
                    e.Graphics.FillEllipse(hoverBrush, x, y, diameter, diameter);
                }
            }

            // 3. Disegna l'emoji centrata
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
