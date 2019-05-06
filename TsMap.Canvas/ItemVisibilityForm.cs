using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class ItemVisibilityForm : Form
    {
        public ItemVisibilityForm(RenderFlags renderFlags)
        {
            InitializeComponent();
            PrefabsCheckBox.Checked = (renderFlags & RenderFlags.Prefabs) == RenderFlags.Prefabs;
            RoadsCheckBox.Checked = (renderFlags & RenderFlags.Roads) == RenderFlags.Roads;
            MapAreasCheckBox.Checked = (renderFlags & RenderFlags.MapAreas) == RenderFlags.MapAreas;
            MapOverlaysCheckBox.Checked = (renderFlags & RenderFlags.MapOverlays) == RenderFlags.MapOverlays;
            FerryConnectionsCheckBox.Checked = (renderFlags & RenderFlags.FerryConnections) == RenderFlags.FerryConnections;
            CityNamesCheckBox.Checked = (renderFlags & RenderFlags.CityNames) == RenderFlags.CityNames;
        }

        public delegate void UpdateItemVisibilityEvent(RenderFlags renderFlags);

        public UpdateItemVisibilityEvent UpdateItemVisibility;

        private void CheckChanged(object sender, System.EventArgs e) // Gets called if any checkbox is changed
        {
            RenderFlags renderFlags = 0;
            if (PrefabsCheckBox.Checked) renderFlags |= RenderFlags.Prefabs;
            if (RoadsCheckBox.Checked) renderFlags |= RenderFlags.Roads;
            if (MapAreasCheckBox.Checked) renderFlags |= RenderFlags.MapAreas;
            if (MapOverlaysCheckBox.Checked) renderFlags |= RenderFlags.MapOverlays;
            if (FerryConnectionsCheckBox.Checked) renderFlags |= RenderFlags.FerryConnections;
            if (CityNamesCheckBox.Checked) renderFlags |= RenderFlags.CityNames;
            UpdateItemVisibility?.Invoke(renderFlags);
        }
    }
}
