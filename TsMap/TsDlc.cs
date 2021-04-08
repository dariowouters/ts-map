using System.Collections.Generic;
using System.Text;

namespace TsMap {
    public class TsDlc {
        private readonly TsMapper _mapper;

        public TsDlc( TsMapper mapper, string path ) {
            this._mapper = mapper;

            byte[]   dlcFileContent = this._mapper.Rfs.GetFileEntry( path ).Entry.Read();
            string[] lines          = Encoding.UTF8.GetString( dlcFileContent ).Split( '\n' );

            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = SiiHelper.ParseLine( line );
                if ( !validLine ) continue;

                if ( key.Equals( "package_version" ) )
                    this.packageVersion = value;

                if ( key.Equals( "display_name" ) )
                    this.displayName = value;

                if ( key.Contains( "compatible_versions[]" ) )
                    this.compatibleVersion.Add( value );

                if ( key.Contains( "features[]" ) )
                    this.features.Add( value );
            }
        }

        public string         packageVersion    { get; }
        public string         displayName       { get; }
        public List< string > compatibleVersion { get; } = new List< string >();
        public List< string > features          { get; } = new List< string >();

        public bool IsAMapDlc() {
            return this.features.Exists( e => e.Equals( "map" ) );
        }
    }
}