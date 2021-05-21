using ReactiveUI.Fody.Helpers;

namespace TsMap2.UI.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        public string Greeting => "Welcome to Avalonia!";

        [ Reactive ] public bool AppLoaded { get; set; } = false;
    }
}