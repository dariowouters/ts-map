using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views.MainView {
    public class SidePanelView : UserControl {
        public SidePanelView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}