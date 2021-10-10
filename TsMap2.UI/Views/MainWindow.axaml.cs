using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Serilog;
using TsMap2.UI.ViewModels;

namespace TsMap2.UI.Views {
    public sealed class MainWindow : ReactiveWindow< MainWindowViewModel > {
        public MainWindow() {
            this.InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load( this );
        }

        protected override void OnGotFocus( GotFocusEventArgs e ) {
            base.OnGotFocus( e );

            // Console.Write( "D >> " );
            // Console.WriteLine( ViewModel );

            try {
                Task? t = Task.Run( this.Init );
                t.ContinueWith( e => this.PostInit() );
                // t.Wait();
            } catch ( Exception ex ) {
                Console.WriteLine( ex.GetBaseException().Message, ex.GetBaseException().StackTrace );
            }
        }

        private void Init() {
            Dispatcher.UIThread.InvokeAsync( () => {
                Thread.Sleep( 5000 );
                this.ViewModel.AppLoaded = true;
                Log.Debug( "UI Loaded {0}", this.ViewModel.AppLoaded );
            } );
        }

        private void PostInit() {
            // Console.Write( "D2 >> " );
            // Console.WriteLine( ViewModel.AppLoaded );
        }
    }
}