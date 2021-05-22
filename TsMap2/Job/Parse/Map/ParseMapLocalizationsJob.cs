using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Map {
    public class ParseMapLocalizationsJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Localizations] Loading" );
            // Console.WriteLine( this.Store().Settings.GamePath );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][Localizations] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory localeDir = this.Store().Rfs.GetDirectory( "locale" );
            if ( localeDir == null ) {
                Log.Warning( "[Job][Localizations] Could not find locale directory." );
                return;
            }


            var localizationList = new List< string >();

            foreach ( KeyValuePair< ulong, ScsDirectory > localeDirDirectory in localeDir.Directories ) {
                localizationList.Add( localeDirDirectory.Value.GetCurrentDirectoryName() );

                foreach ( KeyValuePair< ulong, ScsFile > localeFile in localeDirDirectory.Value.Files )
                    this.ParseLocale( localeFile.Value, localeDirDirectory.Value.GetCurrentDirectoryName() );
            }

            Log.Information( "[Job][Localizations] Loaded." );
        }

        private void ParseLocale( ScsFile localeFile, string locale ) {
            if ( localeFile == null ) return;

            byte[] entryContents = localeFile.Entry.Read();
            uint   magic         = MemoryHelper.ReadUInt32( entryContents, 0 );
            string fileContents = magic == 21720627
                                      ? ScsHelper.Decrypt3Nk( entryContents )
                                      : Encoding.UTF8.GetString( entryContents );

            if ( fileContents == null ) {
                Log.Warning( "Could not read locale file '{0}'", localeFile.GetPath() );
                return;
            }

            var key = string.Empty;

            foreach ( string l in fileContents.Split( '\n' ) ) {
                if ( !l.Contains( ':' ) ) continue;

                if ( l.Contains( "key[]" ) )
                    key = l.Split( '"' )[ 1 ];

                else if ( l.Contains( "val[]" ) ) {
                    string val = l.Split( '"' )[ 1 ];

                    if ( key != string.Empty && val != string.Empty ) {
                        IEnumerable< TsCity > cities = this.Store().Def.Cities.Values.Where( x => x.LocalizationToken == key );

                        foreach ( TsCity city in cities )
                            this.Store().Def.Cities[ city.Token ].AddLocalizedName( locale, val );

                        TsCountry country = this.Store().Def.Countries.Values.FirstOrDefault( x => x.LocalizationToken == key );
                        if ( country != null )
                            this.Store().Def.Countries[ country.Token ].AddLocalizedName( locale, val );
                    }
                }
            }
        }
    }
}