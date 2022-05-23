using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class ItemVisibilityForm : Form
    {
        public ItemVisibilityForm(RenderFlags renderFlags)
        {
            InitializeComponent();
            PrefabsCheckBox.Checked = renderFlags.IsActive(RenderFlags.Prefabs);
            RoadsCheckBox.Checked = renderFlags.IsActive(RenderFlags.Roads);
            SecretRoadsCheckBox.Checked = renderFlags.IsActive(RenderFlags.SecretRoads);
            MapAreasCheckBox.Checked = renderFlags.IsActive(RenderFlags.MapAreas);
            MapOverlaysCheckBox.Checked = renderFlags.IsActive(RenderFlags.MapOverlays);
            FerryConnectionsCheckBox.Checked = renderFlags.IsActive(RenderFlags.FerryConnections);
            CityNamesCheckBox.Checked = renderFlags.IsActive(RenderFlags.CityNames);
        }

        public delegate void UpdateItemVisibilityEvent(RenderFlags renderFlags);

        public UpdateItemVisibilityEvent UpdateItemVisibility;

        private void CheckChanged(object sender, EventArgs e) // Gets called if any checkbox is changed
        {
            RenderFlags renderFlags = 0;
            if (PrefabsCheckBox.Checked) renderFlags |= RenderFlags.Prefabs;
            if (RoadsCheckBox.Checked) renderFlags |= RenderFlags.Roads;
            if (SecretRoadsCheckBox.Checked) renderFlags |= RenderFlags.SecretRoads;
            if (MapAreasCheckBox.Checked) renderFlags |= RenderFlags.MapAreas;
            if (MapOverlaysCheckBox.Checked) renderFlags |= RenderFlags.MapOverlays;
            if (FerryConnectionsCheckBox.Checked) renderFlags |= RenderFlags.FerryConnections;
            if (CityNamesCheckBox.Checked) renderFlags |= RenderFlags.CityNames;
            UpdateItemVisibility?.Invoke(renderFlags);
        }
    }
}
