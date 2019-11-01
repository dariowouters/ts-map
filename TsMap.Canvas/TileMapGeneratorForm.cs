using System;
using System.IO;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TileMapGeneratorForm : Form
    {
        public delegate void GenerateTileMapEvent(string exportPath, int startZoomLevel, int endZoomLevel,
            bool createTiles, bool exportCities, bool exportOverlays, bool createInfo, RenderFlags renderFlags);

        public GenerateTileMapEvent GenerateTileMap;
        public TileMapGeneratorForm(string lastTileMapPath, RenderFlags renderFlags)
        {
            InitializeComponent();
            folderBrowserDialog1.Description = "Select where you want the tile map files to be placed";
            folderBrowserDialog1.SelectedPath = lastTileMapPath;

            PrefabsCheckBox.Checked = (renderFlags & RenderFlags.Prefabs) == RenderFlags.Prefabs;
            RoadsCheckBox.Checked = (renderFlags & RenderFlags.Roads) == RenderFlags.Roads;
            MapAreasCheckBox.Checked = (renderFlags & RenderFlags.MapAreas) == RenderFlags.MapAreas;
            MapOverlaysCheckBox.Checked = (renderFlags & RenderFlags.MapOverlays) == RenderFlags.MapOverlays;
            FerryConnectionsCheckBox.Checked = (renderFlags & RenderFlags.FerryConnections) == RenderFlags.FerryConnections;
            CityNamesCheckBox.Checked = (renderFlags & RenderFlags.CityNames) == RenderFlags.CityNames;
        }
        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            var startZoomLevel = Convert.ToInt32(Math.Round(StartZoomLevelBox.Value, 0));
            var endZoomLevel = Convert.ToInt32(Math.Round(EndZoomLevelBox.Value, 0));

            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (!Directory.Exists(folderBrowserDialog1.SelectedPath)) return;

                RenderFlags renderFlags = 0;
                if (PrefabsCheckBox.Checked) renderFlags |= RenderFlags.Prefabs;
                if (RoadsCheckBox.Checked) renderFlags |= RenderFlags.Roads;
                if (MapAreasCheckBox.Checked) renderFlags |= RenderFlags.MapAreas;
                if (MapOverlaysCheckBox.Checked) renderFlags |= RenderFlags.MapOverlays;
                if (FerryConnectionsCheckBox.Checked) renderFlags |= RenderFlags.FerryConnections;
                if (CityNamesCheckBox.Checked) renderFlags |= RenderFlags.CityNames;

                GenerateTileMap(folderBrowserDialog1.SelectedPath, startZoomLevel, endZoomLevel, GenTilesCheck.Checked,
                    GenCityListCheck.Checked, GenOverlaysCheck.Checked, GenTileMapInfoCheck.Checked, renderFlags);
            }
        }

        private void GenTilesCheck_CheckedChanged(object sender, EventArgs e)
        {
            StartZoomLevelBox.Enabled = GenTilesCheck.Checked;
            EndZoomLevelBox.Enabled = GenTilesCheck.Checked;
            GenTileMapInfoCheck.Checked = GenTilesCheck.Checked;
        }

        private void GenCityListCheck_CheckedChanged(object sender, EventArgs e)
        {
            CityNamesCheckBox.Checked = !GenCityListCheck.Checked;
        }

        private void GenOverlaysCheck_CheckedChanged(object sender, EventArgs e)
        {
            MapOverlaysCheckBox.Checked = !GenOverlaysCheck.Checked;
        }
    }
}
