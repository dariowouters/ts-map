using System.Collections.Generic;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class LocalizationSettingsForm : Form
    {

        public delegate void UpdateLocalizationEvent(int locIndex);

        public UpdateLocalizationEvent UpdateLocalization;

        public LocalizationSettingsForm(List<string> localizationList, int locIndex)
        {
            InitializeComponent();
            localizationComboBox1.DataSource = localizationList;
            if (locIndex < localizationList.Count) localizationComboBox1.SelectedIndex = locIndex;
        }

        private void SubmitBtn_Click(object sender, System.EventArgs e)
        {
            UpdateLocalization(localizationComboBox1.SelectedIndex);
        }
    }
}
