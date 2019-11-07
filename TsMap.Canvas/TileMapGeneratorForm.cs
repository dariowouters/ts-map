using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TileMapGeneratorForm : Form
    {
        public delegate void GenerateTileMapEvent(int zoomLevel);

        public GenerateTileMapEvent GenerateTileMap;
        public TileMapGeneratorForm()
        {
            InitializeComponent();
        }
        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            int.TryParse(TileZoomLevelText.Text, out var zoomLevel);
            GenerateTileMap(zoomLevel);
        }
    }
}
