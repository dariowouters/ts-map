using System;
using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2.Work {
    public class AWork {
        private readonly TsMapperContext _mapperContext;

        public AWork( TsMapperContext mapperContext ) => this._mapperContext = mapperContext;

        public void Run() {
            Console.WriteLine( "Running" );
            Settings settings = this._mapperContext.GetSettings();

            Console.WriteLine( settings.GamePath );

            ScsFile ets2File = this._mapperContext.rfs.GetFileEntry( TsSiiDef.Ets2LogoScene );
            ScsFile atsFile  = this._mapperContext.rfs.GetFileEntry( TsSiiDef.AtsLogoScene );

            Console.WriteLine( settings.GamePath );

            if ( ets2File != null ) // Log.Msg( "ETS2 detected" );
                Console.WriteLine( "ETS2 detected" );
            else if ( atsFile != null ) // Log.Msg( "ATS detected" );
                Console.WriteLine( "ATS detected" );
            else // Log.Msg( "Unknown game" );
                Console.WriteLine( "Unknown game" );
        }
    }
}