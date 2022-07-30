using System.Collections.Generic;
using System.Windows.Forms;
using TsMap.Common;

namespace TsMap.Canvas
{
    public partial class DlcGuardForm : Form
    {
        private bool _addingItems;

        public DlcGuardForm(List<DlcGuard> dlcGuards)
        {
            InitializeComponent();
            _addingItems = true;
            foreach (var dlcGuard in dlcGuards) DlcGuardCheckedListBox.Items.Add(dlcGuard, dlcGuard.Enabled);

            _addingItems = false;
        }

        public delegate void UpdateDlcGuardsEvent(byte index, bool enabled);

        public UpdateDlcGuardsEvent UpdateDlcGuards;

        private void DlcGuardCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_addingItems || !(DlcGuardCheckedListBox.Items[e.Index] is DlcGuard dlcGuard)) return;

            UpdateDlcGuards?.Invoke(dlcGuard.Index, e.NewValue == CheckState.Checked);
        }
    }
}