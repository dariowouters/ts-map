using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsMap2.UI.Views.MainView {
    public class SplashView : UserControl {
        public SplashView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }
    }
}