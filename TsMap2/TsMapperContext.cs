using System.Collections.Generic;
using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2 {
    public class TsMapperContext {
        public const string CONTEXT_NAME_SETTINGS = "settings";

        private readonly Dictionary< string, IContext > _contexts = new Dictionary< string, IContext >();

        public TsMapperContext( RootFileSystem rfs ) => this.rfs = rfs;

        public RootFileSystem rfs { get; }

        public void AddContext( string name, IContext context ) {
            this._contexts[ name ] = context;
        }

        public IContext GetContext( string name ) =>
            this._contexts.ContainsKey( name )
                ? this._contexts[ name ]
                : null;

        // --

        public void SetSettings( Settings settings ) {
            this.AddContext( CONTEXT_NAME_SETTINGS, settings );
        }

        public Settings GetSettings() => (Settings) this.GetContext( CONTEXT_NAME_SETTINGS );
    }
}