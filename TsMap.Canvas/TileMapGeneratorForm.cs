using System;
using System.IO;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TileMapGeneratorForm : Form
    {

        public delegate void GenerateTileMapEvent(string exportPath, int startZoomLevel, int endZoomLevel,
            bool createTiles, ExportFlags exportFlags, RenderFlags renderFlags);

        public delegate void ExportMapDataEvent(string exportPath, ExportFlags exportFlags);

        public GenerateTileMapEvent GenerateTileMap;

        public ExportMapDataEvent ExportMapData;

        public TileMapGeneratorForm()
        {
            InitializeComponent();
            folderBrowserDialog1.Description = "Select where you want the tile map files to be placed";
            folderBrowserDialog1.SelectedPath = SettingsManager.Current.Settings.TileGenerator.LastTileMapPath;

            PrefabsCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.Prefabs);
            RoadsCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.Roads);
            MapAreasCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.MapAreas);
            MapOverlaysCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.MapOverlays);
            FerryConnectionsCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.FerryConnections);
            CityNamesCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.CityNames);
            BusStopOverlayCheckBox.Checked = SettingsManager.Current.Settings.TileGenerator.RenderFlags.IsActive(RenderFlags.BusStopOverlay);

            EndZoomLevelBox.Value = SettingsManager.Current.Settings.TileGenerator.EndZoomLevel;
            StartZoomLevelBox.Value = SettingsManager.Current.Settings.TileGenerator.StartZoomLevel;
            GenTilesCheck.Checked = SettingsManager.Current.Settings.TileGenerator.GenerateTiles;

            txtMapPadding.Text = SettingsManager.Current.Settings.TileGenerator.MapPadding.ToString();
            txtTileSize.Text = SettingsManager.Current.Settings.TileGenerator.TileSize.ToString();

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

            SetExportFlags();
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
            if (triStateTreeView1.GetCheckedByNodeName("GenBusStops")) exportFlags |= ExportFlags.BusStops;
            if (triStateTreeView1.GetCheckedByNodeName("GenCargoDefs")) exportFlags |= ExportFlags.CargoDefs;

            return exportFlags;
        }

        private void SetExportFlags()
        {
            triStateTreeView1.GetNodeByName("GenTileMapInfo").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.TileMapInfo);
            triStateTreeView1.GetNodeByName("GenCityList").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CityList);
            //triStateTreeView1.GetNodeByName("GenCityDimensions").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CityDimensions);
            triStateTreeView1.GetNodeByName("GenCityLocalizedNames").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CityLocalizedNames);
            triStateTreeView1.GetNodeByName("GenCountryList").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CountryList);
            triStateTreeView1.GetNodeByName("GenCountryLocalizedNames").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CountryLocalizedNames);
            triStateTreeView1.GetNodeByName("GenOverlayList").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.OverlayList);
            triStateTreeView1.GetNodeByName("GenOverlayPNGs").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.OverlayPNGs);
            triStateTreeView1.GetNodeByName("GenBusStops").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.BusStops);
            triStateTreeView1.GetNodeByName("GenCargoDefs").Checked = SettingsManager.Current.Settings.TileGenerator.ExportFlags.IsActive(ExportFlags.CargoDefs);
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            var startZoomLevel = Convert.ToInt32(Math.Round(StartZoomLevelBox.Value, 0));
            var endZoomLevel = Convert.ToInt32(Math.Round(EndZoomLevelBox.Value, 0));

            if (startZoomLevel < 0 || endZoomLevel < 0)
            {
                MessageBox.Show("Cannot set start or end zoom level less than zero");
                return;
            }

            if (startZoomLevel > endZoomLevel)
            {
                MessageBox.Show("Cannot set start zoom level lower than end zoom level");
                return;
            }        

            if (Convert.ToInt32(txtMapPadding.Text) <= 0)
            {
                MessageBox.Show("Map padding is invalid. Must be greater than 0");
                return;
            }

            if (Convert.ToInt32(txtTileSize.Text) <= 0)
            {
                MessageBox.Show("Tile size is invalid. Must be greater than 0");
            }

           
            SettingsManager.Current.Settings.TileGenerator.ExportFlags = GetExportFlags();
            SettingsManager.Current.Settings.TileGenerator.MapPadding = Convert.ToInt32(txtMapPadding.Text);
            SettingsManager.Current.Settings.TileGenerator.TileSize = Convert.ToInt32(txtTileSize.Text);
            SettingsManager.Current.Settings.TileGenerator.RenderFlags = GetRenderFlags();
            SettingsManager.Current.Settings.TileGenerator.GenerateTiles = GenTilesCheck.Checked;
            SettingsManager.Current.Settings.TileGenerator.StartZoomLevel = startZoomLevel;
            SettingsManager.Current.Settings.TileGenerator.EndZoomLevel = endZoomLevel;

            SettingsManager.Current.SaveSettings();

            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (!Directory.Exists(folderBrowserDialog1.SelectedPath)) return;

                SettingsManager.Current.Settings.TileGenerator.LastTileMapPath = folderBrowserDialog1.SelectedPath;

                SettingsManager.Current.SaveSettings();

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

        private void ExportDataBtn_Click_1(object sender, EventArgs e)
        {
            SettingsManager.Current.Settings.TileGenerator.ExportFlags = GetExportFlags();
            SettingsManager.Current.SaveSettings();

            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (!Directory.Exists(folderBrowserDialog1.SelectedPath)) return;

                SettingsManager.Current.Settings.TileGenerator.LastTileMapPath = folderBrowserDialog1.SelectedPath;

                SettingsManager.Current.SaveSettings();

                ExportMapData(folderBrowserDialog1.SelectedPath,
                    GetExportFlags());
            }
        }
    }
}
