using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views {
    public class MenuView : UserControl {
        public MenuView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}