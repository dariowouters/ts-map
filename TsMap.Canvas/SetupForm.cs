using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class SetupForm : Form
    {
        private string gamePath;
        private string modPath;
        private List<Mod> _mods = new List<Mod>();

        public SetupForm()
        {
            InitializeComponent();
            folderBrowserDialog1.Description = "Please select the game directory\nE.g. D:/Games/steamapps/common/Euro Truck Simulator 2/";
            folderBrowserDialog1.ShowNewFolderButton = false;
            //folderBrowserDialog1.SelectedPath = @"C:\Games\steamapps\common\Euro Truck Simulator 2";
            modFolderBrowserDialog.Description = "Please select the mod directory\nE.g. D:/Users/Dario/Documents/Euro Truck Simulator 2/mod";
            modFolderBrowserDialog.ShowNewFolderButton = false;
            //modFolderBrowserDialog.SelectedPath = @"D:\Users\Dario\Documents\Euro Truck Simulator 2\mod";
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                gamePath = label1.Text = folderBrowserDialog1.SelectedPath;
                if (loadMods.Checked && modPath == null) return;
                NextBtn.Enabled = true;
            }
        }

        private void NextBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            new TsMapCanvas(this, folderBrowserDialog1.SelectedPath, _mods).Show();
            Hide();
        }

        private void loadMods_CheckedChanged(object sender, EventArgs e)
        {
            if (loadMods.Checked)
            {
                modPanel.Visible = true;
                if (modPath == null) NextBtn.Enabled = false;
                UpdateModList();
            }
            else
            {
                modPanel.Visible = false;
                _mods.ForEach(x => x.Load = false);
                if (gamePath != null) NextBtn.Enabled = true;
            }
        }

        private void UpdateModList()
        {
            var selectedIndex = modList.SelectedIndex;
            modList.Items.Clear();
            for (var i = 0; i < _mods.Count; i++)
            {
                modList.Items.Add(_mods[i]);
                modList.SetItemChecked(i, _mods[i].Load);
            }

            modList.SelectedIndex = selectedIndex;
        }

        private void MoveItemToTop(int index)
        {
            if (index < 1 || index > _mods.Count - 1) return;
            var newTopMod = _mods[index];
            for (var i = index; i > 0; i--)
            {
                _mods[i] = _mods[i - 1];
            }

            _mods[0] = newTopMod;
            modList.SelectedIndex = 0;
            UpdateModList();
        }

        private void MoveItemToBottom(int index)
        {
            if (index > _mods.Count - 2 || index < 0) return;
            var newBottomMod = _mods[index];
            for (var i = index; i < _mods.Count - 1; i++)
            {
                _mods[i] = _mods[i + 1];
            }

            _mods[_mods.Count - 1] = newBottomMod;
            modList.SelectedIndex = _mods.Count - 1;
            UpdateModList();
        }

        private void MoveItem(int index, int direction)
        {
            if (index < 0) return;
            var newIndex = index + direction;
            if (newIndex < 0 || newIndex >= _mods.Count) return;
            var origItem = _mods[newIndex];
            _mods[newIndex] = _mods[index];
            _mods[index] = origItem;
            modList.SelectedIndex = modList.SelectedIndex + direction;
            UpdateModList();
        }

        private void PrioUp_Click(object sender, EventArgs e)
        {
            if (_mods.Count > 1) MoveItem(modList.SelectedIndex, -1);
        }

        private void PrioDown_Click(object sender, EventArgs e)
        {
            if (_mods.Count > 1) MoveItem(modList.SelectedIndex, 1);
        }

        private void BrowseModBtn_Click(object sender, EventArgs e)
        {
            var result = modFolderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                modPath = modFolderBrowserDialog.SelectedPath;
                var files = Directory.GetFiles(modPath, "*.scs");
                _mods = files.Select(x => new Mod(x)).ToList();
                UpdateModList();
                if (gamePath != null) NextBtn.Enabled = true;
            }
        }

        private void modList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _mods[e.Index].Load = e.NewValue == CheckState.Checked;
        }

        private void ToTop_Click(object sender, EventArgs e)
        {
            MoveItemToTop(modList.SelectedIndex);
        }

        private void ToBottom_Click(object sender, EventArgs e)
        {
            MoveItemToBottom(modList.SelectedIndex);
        }
    }
}
