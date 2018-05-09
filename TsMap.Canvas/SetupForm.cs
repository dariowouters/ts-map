using System;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
            folderBrowserDialog1.Description = "Please select the game directory\nE.g. D:/Games/steamapps/common/Euro Truck Simulator 2/";
            folderBrowserDialog1.ShowNewFolderButton = false;
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                label1.Text = folderBrowserDialog1.SelectedPath;
                NextBtn.Enabled = true;
            }
        }

        private void NextBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            new TsMapCanvas(this, folderBrowserDialog1.SelectedPath).Show();
            Hide();
        }
    }
}
