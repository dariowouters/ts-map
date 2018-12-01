using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class ImageExportOptionForm : Form
    {
        public delegate void ExportImageEvent(int width, int height, MapPalette palette);

        public ExportImageEvent ExportImage;

        private MapPalette _palette;

        public ImageExportOptionForm(MapPalette palette)
        {
            InitializeComponent();
            _palette = palette;
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            int.TryParse(WidthText.Text, out var width);
            int.TryParse(HeightText.Text, out var height);
            ExportImage(width, height, _palette);
        }
    }
}
