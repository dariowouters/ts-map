using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TsMap2.Exceptions;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsFerryConnectionEntry : AbstractScsEntry< List< TsFerryConnection > > {
        private readonly List< TsFerryConnection > _ferryConnections = new List< TsFerryConnection >();

        public List< TsFerryConnection > List() {
            VerifyRfs();

            ScsDirectory connectionDirectory = Store.Rfs.GetDirectory( ScsPath.Def.FerryConnectionPath );
            if ( connectionDirectory == null ) {
                var message = $"Could not read '{ScsPath.Def.FerryConnectionPath}' dir";
                throw new ScsEntryException( message );
            }

            List< ScsFile > ferryConnectionFiles = connectionDirectory.GetFiles( ScsPath.ScsFileExtension );
            if ( ferryConnectionFiles == null ) {
                var message = "Could not read ferry connection files";
                throw new ScsEntryException( message );
            }

            foreach ( ScsFile ferryConnectionFile in ferryConnectionFiles ) {
                byte[] data = ferryConnectionFile.Entry.Read();
                Generate( data );
            }

            return _ferryConnections;
        }

        public override List< TsFerryConnection > Generate( byte[] stream ) {
            string[] lines = Encoding.UTF8.GetString( stream ).Split( '\n' );

            var               ferryConnections = new List< TsFerryConnection >();
            TsFerryConnection ferryConnection  = null;

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
                            StartPortToken = ScsHashHelper.StringToToken( portIds[ 1 ] ),
                            EndPortToken   = ScsHashHelper.StringToToken( portIds[ 2 ].TrimEnd( '{' ).Trim() )
                        };
                    }
                }


                if ( !line.Contains( "}" ) || ferryConnection == null ) continue;

                AddFerryConnection( ferryConnection );

                ferryConnection = null;
            }

            return ferryConnections;
        }

        private void AddFerryConnection( TsFerryConnection ferryConnection ) {
            TsFerryConnection existingItem = _ferryConnections.FirstOrDefault( item =>
                                                                                   item.StartPortToken  == ferryConnection.StartPortToken
                                                                                   && item.EndPortToken == ferryConnection.EndPortToken
                                                                                   || item.StartPortToken == ferryConnection.EndPortToken
                                                                                   && item.EndPortToken
                                                                                   == ferryConnection
                                                                                       .StartPortToken ); // Check if ferryConnectionection already exists
            if ( existingItem == null ) _ferryConnections.Add( ferryConnection );
        }
    }
}