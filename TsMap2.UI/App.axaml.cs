using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TsMap2.UI.ViewModels;
using TsMap2.UI.Views;

namespace TsMap2.UI {
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load( this );
        }

        public override void OnFrameworkInitializationCompleted() {
            if ( this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel()
                };

            base.OnFrameworkInitializationCompleted();
        }
    }
}