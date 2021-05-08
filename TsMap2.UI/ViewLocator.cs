using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TsMap2.UI.ViewModels;

namespace TsMap2.UI {
    public class ViewLocator : IDataTemplate {
        public bool SupportsRecycling => false;

        public IControl Build( object data ) {
            string? name = data.GetType().FullName!.Replace( "ViewModel", "View" );
            var     type = Type.GetType( name );

            if ( type != null )
                return (Control) Activator.CreateInstance( type )!;
            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match( object data ) => data is ViewModelBase;
    }
}