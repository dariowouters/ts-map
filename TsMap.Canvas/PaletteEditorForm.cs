using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class PaletteEditorForm : Form
    {
        public delegate void UpdatePaletteEvent(MapPalette palette);
        public UpdatePaletteEvent UpdatePalette;

        public PaletteEditorForm()
        {
            InitializeComponent();

            txtBackground.Text = SettingsManager.Current.Settings.Palette.Background;
            txtCityNames.Text = SettingsManager.Current.Settings.Palette.CityName;
            txtError.Text = SettingsManager.Current.Settings.Palette.Error;
            txtFerryLines.Text = SettingsManager.Current.Settings.Palette.FerryLines;
            txtPrefabDark.Text = SettingsManager.Current.Settings.Palette.PrefabDark;
            txtPrefabGreen.Text = SettingsManager.Current.Settings.Palette.PrefabGreen;
            txtPrefabLight.Text = SettingsManager.Current.Settings.Palette.PrefabLight;
            txtPrefabRoad.Text = SettingsManager.Current.Settings.Palette.PrefabRoad;
            txtRoad.Text = SettingsManager.Current.Settings.Palette.Road;
        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            SettingsManager.Current.Settings.Palette.Background = txtBackground.Text;
            SettingsManager.Current.Settings.Palette.CityName = txtCityNames.Text;
            SettingsManager.Current.Settings.Palette.Error = txtBackground.Text;
            SettingsManager.Current.Settings.Palette.FerryLines = txtFerryLines.Text;
            SettingsManager.Current.Settings.Palette.PrefabDark = txtPrefabDark.Text;
            SettingsManager.Current.Settings.Palette.PrefabGreen = txtPrefabGreen.Text;
            SettingsManager.Current.Settings.Palette.PrefabLight = txtPrefabLight.Text;
            SettingsManager.Current.Settings.Palette.PrefabRoad = txtPrefabRoad.Text;
            SettingsManager.Current.Settings.Palette.Road = txtRoad.Text;

            SettingsManager.Current.SaveSettings();

            UpdatePalette(SettingsManager.Current.Settings.Palette.ToBrushPalette());

        }
    }
}
