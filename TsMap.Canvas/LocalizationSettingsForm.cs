using System.Collections.Generic;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class LocalizationSettingsForm : Form
    {

        public delegate void UpdateLocalizationEvent(string localeName);

        public UpdateLocalizationEvent UpdateLocalization;

        public LocalizationSettingsForm(List<string> localizationList, string locale)
        {
            InitializeComponent();
            localizationComboBox1.DataSource = localizationList;
            var index = localizationList.FindIndex(x => x == locale);
            localizationComboBox1.SelectedIndex = (index != -1) ? index : 0;
        }

        private void SubmitBtn_Click(object sender, System.EventArgs e)
        {
            UpdateLocalization(localizationComboBox1.GetItemText(localizationComboBox1.SelectedItem));
        }
    }
}
