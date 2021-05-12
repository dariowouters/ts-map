using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views {
    public class ExportWindow : Window {
        public ExportWindow() {
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