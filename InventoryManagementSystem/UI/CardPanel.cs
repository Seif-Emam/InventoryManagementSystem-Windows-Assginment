using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Custom Card Panel with clean rounded corners, border strokes, and anti-aliased GDI+ painting.
    /// </summary>
    public class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = DesignSystem.CardBackground;
            Padding = new Padding(20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DesignSystem.PaintCard(e.Graphics, ClientRectangle, 8);
        }
    }
}

