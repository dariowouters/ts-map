using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class MapPanel : Panel
    {
        public MapPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }
    }
}
