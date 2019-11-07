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

        public Settings AppSettings { get; }

        public SetupForm()
        {
            InitializeComponent();
            AppSettings = JsonHelper.LoadSettings();
            GameFolderBrowserDialog.Description = "Please select the game directory\nE.g. D:/Games/steamapps/common/Euro Truck Simulator 2/";
            GameFolderBrowserDialog.ShowNewFolderButton = false;
            if (AppSettings.LastGamePath != null)
            {
                GameFolderBrowserDialog.SelectedPath = AppSettings.LastGamePath;
                SelectedGamePath();
            }

            ModFolderBrowserDialog.Description = "Please select the mod directory\nE.g. D:/Users/Dario/Documents/Euro Truck Simulator 2/mod";
            ModFolderBrowserDialog.ShowNewFolderButton = false;
            if (AppSettings.LastModPath != null)
            {
                ModFolderBrowserDialog.SelectedPath = AppSettings.LastModPath;
                SelectedModPath();
            }
        }

        private void SelectedGamePath()
        {
            if (!Directory.Exists(GameFolderBrowserDialog.SelectedPath)) return;
            gamePath = SelectedGamePathLabel.Text = AppSettings.LastGamePath = GameFolderBrowserDialog.SelectedPath;
            if (loadMods.Checked && modPath == null) return;
            NextBtn.Enabled = true;
        }

        private void SelectedModPath()
        {
            if (!Directory.Exists(ModFolderBrowserDialog.SelectedPath)) return;
            modPath = SelectedModPathLabel.Text = AppSettings.LastModPath = ModFolderBrowserDialog.SelectedPath;
            var files = Directory.EnumerateFiles(modPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".scs", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
            _mods = files.Select(x => new Mod(x)).ToList();
            UpdateModList();
            if (gamePath != null) NextBtn.Enabled = true;
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var result = GameFolderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SelectedGamePath();
            }
        }

        private void NextBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            new TsMapCanvas(this, GameFolderBrowserDialog.SelectedPath, _mods).Show();
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
            var result = ModFolderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SelectedModPath();
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

        private void InverseSelection_Click(object sender, EventArgs e)
        {
            _mods.ForEach(x => x.Load = !x.Load);
            UpdateModList();
        }

        private void CheckAll_Click(object sender, EventArgs e)
        {
            _mods.ForEach(x => x.Load = true);
            UpdateModList();
        }
    }
}
