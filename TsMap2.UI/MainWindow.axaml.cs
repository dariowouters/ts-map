using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI {
    public class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}