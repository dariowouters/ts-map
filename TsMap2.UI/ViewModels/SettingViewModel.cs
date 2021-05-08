using ReactiveUI;

namespace TsMap2.UI.ViewModels {
    public class SettingViewModel {
        public Interaction< MainWindowViewModel, SettingViewModel? > ShowDialog { get; }
    }
}