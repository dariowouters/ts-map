using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views.MainView {
    public class MenuView : UserControl {
        public MenuView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }

        private void Button_OnClick( object? sender, RoutedEventArgs e ) {
            var v = new ExportWindow();
            v.Show();
        }
    }
}