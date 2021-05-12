using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views.MainView {
    public class MapView : UserControl {
        public MapView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}