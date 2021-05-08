using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TsMap2.UI.ViewModels;

namespace TsMap2.UI.Views {
    public class MainWindow : ReactiveWindow< MainWindowViewModel > {
        public MainWindow() {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated( d => d( this.ViewModel.ShowDialog.RegisterHandler( this.DoShowDialogAsync ) ) );
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }

        private async Task DoShowDialogAsync( InteractionContext< MainWindowViewModel, SettingViewModel? > interaction ) {
            var dialog = new SettingsWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog< SettingViewModel? >( this );
            interaction.SetOutput( result );
        }
    }
}