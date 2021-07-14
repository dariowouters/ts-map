using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TsMap2.Helper;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsGameEntry : AbstractScsEntry< string > {
        public override string? Generate( byte[] stream ) {
            string[]     lines          = Encoding.UTF8.GetString( stream ).Split( '\n' );
            string       gameCode       = null;
            const string regExpGameCode = @"(ats|ets2)\.pma";

            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                if ( !validLine ) continue;

                if ( key == "scene_model_animation" ) {
                    MatchCollection matches = Regex.Matches( value, regExpGameCode );
                    gameCode = matches.First()?.Groups[ 1 ].Value;

                    break;
                }
            }

            return gameCode;
        }
    }
}