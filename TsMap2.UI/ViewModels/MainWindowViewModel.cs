using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace TsMap2.UI.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        public MainWindowViewModel() {
            this.ShowDialog = new Interaction< MainWindowViewModel, SettingViewModel? >();

            this.BuyMusicCommand = ReactiveCommand.CreateFromTask( async () => {
                var store = new MainWindowViewModel();

                SettingViewModel? result = await this.ShowDialog.Handle( store );
            } );
        }

        public string Greeting => "Welcome to Avalonia!";

        public ICommand BuyMusicCommand { get; }

        public Interaction< MainWindowViewModel, SettingViewModel? > ShowDialog { get; }
    }
}