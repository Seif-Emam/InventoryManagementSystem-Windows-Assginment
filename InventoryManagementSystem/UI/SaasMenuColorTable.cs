using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Professional color table for styling WinForms MenuStrip to match the Navy/Slate SaaS theme.
    /// </summary>
    public class SaasMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => DesignSystem.PrimaryNavy;
        public override Color MenuStripGradientEnd => DesignSystem.PrimaryNavy;
        public override Color MenuItemSelected => DesignSystem.PrimaryBlue;
        public override Color MenuItemSelectedGradientBegin => DesignSystem.PrimaryBlue;
        public override Color MenuItemSelectedGradientEnd => DesignSystem.PrimaryBlue;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuBorder => DesignSystem.BorderColor;
        public override Color ToolStripDropDownBackground => DesignSystem.CardBackground;
        public override Color ImageMarginGradientBegin => DesignSystem.CardBackground;
        public override Color ImageMarginGradientMiddle => DesignSystem.CardBackground;
        public override Color ImageMarginGradientEnd => DesignSystem.CardBackground;
        public override Color SeparatorDark => DesignSystem.BorderColor;
        public override Color SeparatorLight => DesignSystem.BorderColor;
    }
}

