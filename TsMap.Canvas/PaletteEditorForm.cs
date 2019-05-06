using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class PaletteEditorForm : Form
    {
        public delegate void UpdatePaletteEvent(MapPalette palette);
        public UpdatePaletteEvent UpdatePalette;

        public PaletteEditorForm(MapPalette palette)
        {
            InitializeComponent();
        }
    }
}
