using System;
using System.IO;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TileMapGeneratorForm : Form
    {
        public delegate void GenerateTileMapEvent(string exportPath, int startZoomLevel, int endZoomLevel,
            bool createTiles, ExportFlags exportFlags, RenderFlags renderFlags);

        public GenerateTileMapEvent GenerateTileMap;
        public TileMapGeneratorForm(string lastTileMapPath, RenderFlags renderFlags)
        {
            InitializeComponent();
            folderBrowserDialog1.Description = "Select where you want the tile map files to be placed";
            folderBrowserDialog1.SelectedPath = lastTileMapPath;

            PrefabsCheckBox.Checked = renderFlags.IsActive(RenderFlags.Prefabs);
            RoadsCheckBox.Checked = renderFlags.IsActive(RenderFlags.Roads);
            RoadsCheckBox.Checked = renderFlags.IsActive(RenderFlags.SecretRoads);
            MapAreasCheckBox.Checked = renderFlags.IsActive(RenderFlags.MapAreas);
            MapOverlaysCheckBox.Checked = renderFlags.IsActive(RenderFlags.MapOverlays);
            FerryConnectionsCheckBox.Checked = renderFlags.IsActive(RenderFlags.FerryConnections);
            CityNamesCheckBox.Checked = renderFlags.IsActive(RenderFlags.CityNames);
            BusStopOverlayCheckBox.Checked = renderFlags.IsActive(RenderFlags.BusStopOverlay);

            triStateTreeView1.ItemChecked += (TreeNode node) =>
            {
                if (node.Name == "GenCityList")
                {
                    CityNamesCheckBox.Checked = !node.Checked;
                    triStateTreeView1.GetNodeByName("GenCountryList").Checked = node.Checked;
                    triStateTreeView1.GetNodeByName("GenCountryLocalizedNames").Checked = node.Checked;
                }
                else if (node.Name == "GenOverlayList")
                {
                    MapOverlaysCheckBox.Checked = !node.Checked;
                    BusStopOverlayCheckBox.Checked = !node.Checked;
                }
            };
        }

        private RenderFlags GetRenderFlags()
        {
            RenderFlags renderFlags = 0;
            if (PrefabsCheckBox.Checked) renderFlags |= RenderFlags.Prefabs;
            if (RoadsCheckBox.Checked) renderFlags |= RenderFlags.Roads;
            if (SecretRoadsCheckBox.Checked) renderFlags |= RenderFlags.SecretRoads;
            if (MapAreasCheckBox.Checked) renderFlags |= RenderFlags.MapAreas;
            if (MapOverlaysCheckBox.Checked) renderFlags |= RenderFlags.MapOverlays;
            if (FerryConnectionsCheckBox.Checked) renderFlags |= RenderFlags.FerryConnections;
            if (CityNamesCheckBox.Checked) renderFlags |= RenderFlags.CityNames;
            if (BusStopOverlayCheckBox.Checked) renderFlags |= RenderFlags.BusStopOverlay;
            return renderFlags;
        }

        private ExportFlags GetExportFlags()
        {
            ExportFlags exportFlags = 0;

            if (triStateTreeView1.GetCheckedByNodeName("GenTileMapInfo")) exportFlags |= ExportFlags.TileMapInfo;
            if (triStateTreeView1.GetCheckedByNodeName("GenCityList")) exportFlags |= ExportFlags.CityList;
            if (triStateTreeView1.GetCheckedByNodeName("GenCityDimensions")) exportFlags |= ExportFlags.CityDimensions; // TODO: add
            if (triStateTreeView1.GetCheckedByNodeName("GenCityLocalizedNames")) exportFlags |= ExportFlags.CityLocalizedNames;
            if (triStateTreeView1.GetCheckedByNodeName("GenCountryList")) exportFlags |= ExportFlags.CountryList;
            if (triStateTreeView1.GetCheckedByNodeName("GenCountryLocalizedNames")) exportFlags |= ExportFlags.CountryLocalizedNames;
            if (triStateTreeView1.GetCheckedByNodeName("GenOverlayList")) exportFlags |= ExportFlags.OverlayList;
            if (triStateTreeView1.GetCheckedByNodeName("GenOverlayPNGs")) exportFlags |= ExportFlags.OverlayPNGs;

            return exportFlags;
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            var startZoomLevel = Convert.ToInt32(Math.Round(StartZoomLevelBox.Value, 0));
            var endZoomLevel = Convert.ToInt32(Math.Round(EndZoomLevelBox.Value, 0));

            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (!Directory.Exists(folderBrowserDialog1.SelectedPath)) return;

                GenerateTileMap(folderBrowserDialog1.SelectedPath, startZoomLevel, endZoomLevel, GenTilesCheck.Checked,
                    GetExportFlags(), GetRenderFlags());
            }
        }

        private void GenTilesCheck_CheckedChanged(object sender, EventArgs e)
        {
            StartZoomLevelBox.Enabled = GenTilesCheck.Checked;
            EndZoomLevelBox.Enabled = GenTilesCheck.Checked;
            triStateTreeView1.GetNodeByName("GenTileMapInfo").Checked = GenTilesCheck.Checked;
        }

        private void MapOverlaysCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!MapOverlaysCheckBox.Checked) BusStopOverlayCheckBox.Checked = false;
        }

        private void BusStopOverlayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (BusStopOverlayCheckBox.Checked) MapOverlaysCheckBox.Checked = true;
        }
    }
}
