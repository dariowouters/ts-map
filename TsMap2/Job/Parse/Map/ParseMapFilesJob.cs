using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using TsMap2.Factory;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem;

namespace TsMap2.Job.Parse.Map {
    public class ParseMapFilesJob : ThreadJob {
        private bool _isFirstFileRead;

        protected override void Do() {
            Log.Debug( "[Job][MapFiles] Loading" );

            // --- Check RFS
            if ( Store().Rfs == null )
                throw new JobException( "[Job][MapFiles] The root file system was not initialized. Check the game path", JobName(), null );

            ScsDirectory baseMapEntry = Store().Rfs.GetDirectory( ScsPath.Map.MapDirectory );
            if ( baseMapEntry == null ) {
                var message = $"[Job][MapFiles] Could not read {ScsPath.Map.MapDirectory} dir";
                throw new JobException( message, JobName(), ScsPath.Map.MapDirectory );
            }

            List< ScsFile >
                mbd = baseMapEntry
                      .Files
                      .Values
                      .Where( x => x.GetExtension().Equals( ScsPath.Map.MapExtension ) )
                      .ToList(); // Get the map names from the mbd files
            if ( mbd.Count == 0 ) {
                var message = $"[Job][MapFiles] Could not find {ScsPath.Map.MapExtension} file";
                throw new JobException( message, JobName(), ScsPath.Map.MapExtension );
            }


            var sectorFiles = new List< string >();
            foreach ( ScsFile file in mbd ) {
                string mapName = file.GetFileName();
                // this.IsEts2 = !( mapName == "usa" );

                ScsDirectory mapFileDir = Store().Rfs.GetDirectory( $"map/{mapName}" );
                if ( mapFileDir == null ) {
                    var message = $"[Job][MapFiles] Could not read map/{mapName} directory";
                    throw new JobException( message, JobName(), mapName );
                }

                sectorFiles = mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList();
                // sectorFiles.AddRange( mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList() );
            }

            sectorFiles.ForEach( Parse );
            // if ( _sectorFiles.Count <= 0 ) {

            // }

            // long preMapParseTime = DateTime.Now.Ticks;
            // this.Store().Sectors = _sectorFiles.Select( file => new TsSector( sector, file ) ).ToList();
            // sectorFiles.ForEach( this.Parse );
            // this.Store().Sectors.ForEach( sec => sec.ClearFileData() );

            Log.Information( "[Job][MapFiles] Loaded. Roads: {0}",            Store().Map.Roads.Count );
            Log.Information( "[Job][MapFiles] Loaded. Prefabs: {0}",          Store().Map.Prefabs.Count );
            Log.Information( "[Job][MapFiles] Loaded. Companies: {0}",        Store().Map.Companies.Count );
            Log.Information( "[Job][MapFiles] Loaded. Cities: {0}",           Store().Map.Cities.Count );
            Log.Information( "[Job][MapFiles] Loaded. MapOverlays: {0}",      Store().Map.MapOverlays.Count );
            Log.Information( "[Job][MapFiles] Loaded. FerryConnections: {0}", Store().Map.FerryConnections.Count );
            Log.Information( "[Job][MapFiles] Loaded. Triggers: {0}",         Store().Map.Triggers.Count );
            Log.Information( "[Job][MapFiles] Loaded. MapAreas: {0}",         Store().Map.MapAreas.Count );
        }

        private void Parse( string path ) {
            ScsFile file  = Store().Rfs.GetFileEntry( path );
            var     empty = false;

            if ( file == null ) // empty = true;
                return;

            byte[] stream  = file.Entry.Read();
            var    version = BitConverter.ToInt32( stream, 0x0 );

            if ( version < 825 ) {
                Log.Warning( $"{path} version ({version}) is too low, min. is 825" );
                return;
            }

            var itemCount               = BitConverter.ToUInt32( stream, 0x10 );
            if ( itemCount == 0 ) empty = true;
            if ( empty ) return;

            var      lastOffset = 0x14;
            TsSector sector     = null;

            // -- Raw generation
            if ( !_isFirstFileRead ) {
                RawHelper.SaveRawFile( RawType.MAP_SECTORS, file.GetFullName(), stream );
                _isFirstFileRead = true;
            }
            // -- ./Raw generation

            for ( var i = 0; i < itemCount; i++ ) {
                var type = (TsItemType) MemoryHelper.ReadUInt32( stream, lastOffset );

                sector = new TsSector( type, path, version, stream );
                if ( version <= 825 ) type++; // after version 825 all types were pushed up 1

                TsMapItem mapItem;
                switch ( type ) {
                    case TsItemType.Road: {
                        mapItem    =  new TsMapRoadItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.Roads.Add( (TsMapRoadItem) mapItem );
                        break;
                    }
                    case TsItemType.Prefab: {
                        mapItem    =  new TsMapPrefabItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.Prefabs.Add( (TsMapPrefabItem) mapItem );
                        break;
                    }
                    case TsItemType.Company: {
                        mapItem    =  new TsMapCompanyItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.Companies.Add( (TsMapCompanyItem) mapItem );
                        break;
                    }
                    case TsItemType.Service: {
                        mapItem    =  new TsMapServiceItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.CutPlane: {
                        mapItem    =  new TsMapCutPlaneItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.City: {
                        mapItem    =  new TsMapCityItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.Cities.Add( (TsMapCityItem) mapItem );
                        break;
                    }
                    case TsItemType.MapOverlay: {
                        mapItem    =  new TsMapMapOverlayItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.MapOverlays.Add( (TsMapMapOverlayItem) mapItem );
                        break;
                    }
                    case TsItemType.Ferry: {
                        mapItem    =  new TsMapFerryItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.FerryConnections.Add( (TsMapFerryItem) mapItem );
                        break;
                    }
                    case TsItemType.Garage: {
                        mapItem    =  new TsMapGarageItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.Trigger: {
                        mapItem    =  new TsMapTriggerItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;

                        if ( mapItem.Valid ) Store().Map.Triggers.Add( (TsMapTriggerItem) mapItem );
                        break;
                    }
                    case TsItemType.FuelPump: {
                        mapItem    =  new TsMapFuelPumpItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.RoadSideItem: {
                        mapItem    =  new TsMapRoadSideItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.BusStop: {
                        mapItem    =  new TsMapBusStopItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.TrafficRule: {
                        mapItem    =  new TsMapTrafficRuleItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.TrajectoryItem: {
                        mapItem    =  new TsMapTrajectoryItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    case TsItemType.MapArea: {
                        mapItem    =  new TsMapMapAreaItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        if ( mapItem.Valid ) Store().Map.MapAreas.Add( (TsMapMapAreaItem) mapItem );
                        break;
                    }
                    case TsItemType.Cutscene: {
                        mapItem    =  new TsMapCutsceneItem( sector, lastOffset );
                        lastOffset += mapItem.BlockSize;
                        break;
                    }
                    default: {
                        Log.Warning( "Unknown Type: {0} in {1} @ {2}", type, Path.GetFileName( path ), lastOffset );
                        break;
                    }
                }

                // sector.ClearFileData();
            }

            if ( sector == null ) return;

            int nodeCount = MemoryHelper.ReadInt32( stream, lastOffset );
            for ( var i = 0; i < nodeCount; i++ ) {
                var node = new TsNode( sector, lastOffset += 0x04 );
                Store().Map.UpdateEdgeCoords( node );
                if ( !Store().Map.Nodes.ContainsKey( node.Uid ) ) Store().Map.Nodes.Add( node.Uid, node );
                lastOffset += 0x34;
            }

            lastOffset += 0x04;
            if ( lastOffset != stream.Length )
                Log.Warning( $"File '{Path.GetFileName( path )}' was not read correctly. Read offset was at 0x{lastOffset:X} while file is 0x{stream.Length:X} bytes long." );

            sector.ClearFileData();
        }
    }
}