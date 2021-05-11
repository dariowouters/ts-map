using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views {
    public class SidePanelView : UserControl {
        public SidePanelView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}