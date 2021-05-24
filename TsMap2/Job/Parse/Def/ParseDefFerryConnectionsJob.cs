using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Serilog;
using TsMap2.Factory;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefFerryConnectionsJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][FerryConnections] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][FerryConnections] The root file system was not initialized. Check the game path", this.JobName(), null );


            ScsDirectory connectionDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.FerryConnectionPath );
            if ( connectionDirectory == null ) {
                var message = $"Could not read '{ScsPath.Def.FerryConnectionPath}' dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.FerryConnectionPath );
            }

            List< ScsFile > ferryConnectionFiles = connectionDirectory.GetFiles( ScsPath.ScsFileExtension );
            if ( ferryConnectionFiles == null ) {
                var message = "Could not read ferry connection files";
                throw new JobException( message, this.JobName(), null );
            }

            var _isFirstFileRead = false;
            foreach ( ScsFile ferryConnectionFile in ferryConnectionFiles ) {
                byte[]   data  = ferryConnectionFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                TsFerryConnection ferryConnection = null;

                // -- Raw generation
                if ( !_isFirstFileRead ) {
                    RawHelper.SaveRawFile( RawType.FERRY_CONNECTION, ferryConnectionFile.GetFullName(), data );
                    _isFirstFileRead = true;
                }
                // -- ./Raw generation

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                    if ( validLine ) {
                        if ( ferryConnection != null ) {
                            if ( key.Contains( "connection_positions" ) ) {
                                int      index  = int.Parse( key.Split( '[' )[ 1 ].Split( ']' )[ 0 ] );
                                string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                                string[] values = vector.Split( ',' );
                                float    x      = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                                float    z      = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                                ferryConnection.AddConnectionPosition( index, x, z );
                            } else if ( key.Contains( "connection_directions" ) ) {
                                int      index  = int.Parse( key.Split( '[' )[ 1 ].Split( ']' )[ 0 ] );
                                string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                                string[] values = vector.Split( ',' );
                                float    x      = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                                float    z      = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                                ferryConnection.AddRotation( index, Math.Atan2( z, x ) );
                            }
                        }

                        if ( key == "ferry_connection" ) {
                            string[] portIds = value.Split( '.' );
                            ferryConnection = new TsFerryConnection {
                                StartPortToken = ScsHash.StringToToken( portIds[ 1 ] ),
                                EndPortToken   = ScsHash.StringToToken( portIds[ 2 ].TrimEnd( '{' ).Trim() )
                            };
                        }
                    }

                    if ( !line.Contains( "}" ) || ferryConnection == null ) continue;

                    this.Store().Def.AddFerryConnection( ferryConnection );

                    ferryConnection = null;
                }
            }

            Log.Information( "[Job][FerryConnections] Loaded. Found: {0}", this.Store().Def.FerryConnections.Count );
        }
    }
}