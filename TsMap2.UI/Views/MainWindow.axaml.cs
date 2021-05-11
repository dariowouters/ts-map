using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using TsMap2.UI.ViewModels;

namespace TsMap2.UI.Views {
    public class MainWindow : ReactiveWindow< MainWindowViewModel > {
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