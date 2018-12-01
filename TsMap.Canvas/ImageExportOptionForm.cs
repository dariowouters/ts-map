using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class ImageExportOptionForm : Form
    {
        public delegate void ExportImageEvent(int width, int height);

        public ExportImageEvent ExportImage;
        public ImageExportOptionForm()
        {
            InitializeComponent();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            int.TryParse(WidthText.Text, out var width);
            int.TryParse(HeightText.Text, out var height);
            ExportImage(width, height);
        }
    }
}
