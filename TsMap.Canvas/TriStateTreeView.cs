using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TsMap.Canvas
{
    // https://www.codeproject.com/Articles/202435/Tri-State-Tree-View
    public class TriStateTreeView : TreeView
    {
        public enum CheckedState
        {
            Unchecked,
            Checked,
            Mixed,
        }

        private bool _updating;

        public delegate void ItemCheckedEvent(TreeNode node);

        public ItemCheckedEvent ItemChecked;

        public TriStateTreeView()
        {
            StateImageList = new ImageList();

            for (var i = 0; i < 3; i++)
            {
                var b = new Bitmap(16, 16);
                var g = Graphics.FromImage(b);

                switch (i)
                {
                    case 0:
                        CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), CheckBoxState.UncheckedNormal);
                        break;
                    case 1:
                        CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), CheckBoxState.CheckedNormal);
                        break;
                    case 2:
                        CheckBoxRenderer.DrawCheckBox(g, new Point(0, 1), CheckBoxState.MixedNormal);
                        break;
                }

                StateImageList.Images.Add(b);
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            CheckBoxes = false;

            _updating = true;
            UpdateChildState(Nodes, (int) CheckedState.Unchecked, false, true);
            _updating = false;
        }
        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);
            if (_updating) return;

            _updating = true;
            var node = e.Node;

            node.StateImageIndex = node.Checked ? (int) CheckedState.Checked : (int) CheckedState.Unchecked;
            UpdateChildState(node.Nodes, node.StateImageIndex, node.Checked);
            UpdateParentState(node.Parent);

            ItemChecked?.Invoke(node);

            _updating = false;
        }
        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            base.OnAfterExpand(e);
            _updating = true;
            UpdateChildState(e.Node.Nodes, e.Node.StateImageIndex, e.Node.Checked);
            _updating = false;
        }
        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            // is the click on the checkbox? If not, discard it
            var info = HitTest(e.X, e.Y);
            if (info.Location != TreeViewHitTestLocations.StateImage) return;

            // toggle the node's checked status. This will then fire OnAfterCheck
            var node = e.Node;
            node.Checked = !node.Checked;
        }

        // https://stackoverflow.com/a/15760762 (stop double click from expanding when on checkbox)
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203) // double click
            {
                var localPos = PointToClient(Cursor.Position);
                var info = HitTest(localPos);
                if (info.Location == TreeViewHitTestLocations.StateImage)
                    m.Result = IntPtr.Zero;
                else
                    base.WndProc(ref m);
            }
            else base.WndProc(ref m);
        }

        protected void UpdateChildState(TreeNodeCollection childNodes, int stateImageIndex, bool isChecked, bool onlyUninitialised = false)
        {
            foreach (TreeNode childNode in childNodes)
            {
                if (!onlyUninitialised || childNode.StateImageIndex == -1)
                {
                    childNode.StateImageIndex = stateImageIndex;
                    childNode.Checked = isChecked;
                }
                if (childNode.Nodes.Count > 0)
                    UpdateChildState(childNode.Nodes, childNode.StateImageIndex, childNode.Checked);
            }
        }

        protected void UpdateParentState(TreeNode node)
        {
            if (node == null) return;
            var checkedChildren = node.Nodes.Cast<TreeNode>().Count(childNode => childNode.StateImageIndex == (int) CheckedState.Checked);

            if (checkedChildren == node.Nodes.Count)
            {
                node.StateImageIndex = (int) CheckedState.Checked;
                node.Checked = true;
            }
            else
                node.StateImageIndex = (int) CheckedState.Mixed;
        }

        public TreeNode GetNodeByName(TreeNode rootNode, string name)
        {
            if (rootNode.Name == name) return rootNode;
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Name == name) return node;
                var childNode = GetNodeByName(node, name);
                if (childNode != null) return childNode;
            }

            return null;
        }

        public TreeNode GetNodeByName(string name)
        {
            foreach (TreeNode node in Nodes)
            {
                var result = GetNodeByName(node, name);
                if (result != null) return result;
            }

            return null;
        }

        public CheckedState GetStateByNodeName(string name)
        {
            var node = GetNodeByName(name);
            if (node == null) return CheckedState.Unchecked;
            return (CheckedState) node.StateImageIndex;
        }

        public bool GetCheckedByNodeName(string name)
        {
            var node = GetNodeByName(name);
            if (node == null) return false;
            return node.Checked;
        }
    }
}
