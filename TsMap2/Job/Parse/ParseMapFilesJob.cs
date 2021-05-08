using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using TsMap2.Factory.TsItems;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Job.Parse {
    public class ParseMapFilesJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][MapFiles] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][MapFiles] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory baseMapEntry = this.Store().Rfs.GetDirectory( ScsPath.Map.MapDirectory );
            if ( baseMapEntry == null ) {
                var message = $"[Job][MapFiles] Could not read {ScsPath.Map.MapDirectory} dir";
                throw new JobException( message, this.JobName(), ScsPath.Map.MapDirectory );
            }

            List< ScsFile >
                mbd = baseMapEntry
                      .Files
                      .Values
                      .Where( x => x.GetExtension().Equals( ScsPath.Map.MapExtension ) )
                      .ToList(); // Get the map names from the mbd files
            if ( mbd.Count == 0 ) {
                var message = $"[Job][MapFiles] Could not find {ScsPath.Map.MapExtension} file";
                throw new JobException( message, this.JobName(), ScsPath.Map.MapExtension );
            }


            foreach ( ScsFile file in mbd ) {
                string mapName = file.GetFileName();
                // this.IsEts2 = !( mapName == "usa" );

                ScsDirectory mapFileDir = this.Store().Rfs.GetDirectory( $"map/{mapName}" );
                if ( mapFileDir == null ) {
                    var message = $"[Job][MapFiles] Could not read map/{mapName} directory";
                    throw new JobException( message, this.JobName(), mapName );
                }

                var sectorFiles = new List< string >();
                sectorFiles.AddRange( mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList() );

                foreach ( string sectorFile in sectorFiles ) this.Parse( sectorFile );
            }

            // if ( _sectorFiles.Count <= 0 ) {

            // }

            // long preMapParseTime = DateTime.Now.Ticks;
            // this.Store().Sectors = _sectorFiles.Select( file => new TsSector( this, file ) ).ToList();
            // this.Store().Sectors.ForEach( sec => sec.Parse() );
            // this.Store().Sectors.ForEach( sec => sec.ClearFileData() );

            Log.Information( "[Job][MapFiles] RoadItems: {0}", this.Store().Map.RoadItems.Count );
        }

        protected override void OnEnd() { }

        private void Parse( string path ) {
            ScsFile file  = this.Store().Rfs.GetFileEntry( path );
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
            TsSector sector;

            for ( var i = 0; i < itemCount; i++ ) {
                var type = (TsItemType) MemoryHelper.ReadUInt32( stream, lastOffset );

                sector = new TsSector( type, path, version, stream );
                if ( version <= 825 ) type++; // after version 825 all types were pushed up 1

                switch ( type ) {
                    case TsItemType.Road: {
                        TsRoadItem item = new TsRoadItemFactory( sector ).Retrieve( lastOffset );

                        // var item = new TsRoadItem( this, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.RoadItems.Add( item );
                        break;
                    }
                    case TsItemType.Terrain:        break;
                    case TsItemType.Building:       break;
                    case TsItemType.Prefab:         break;
                    case TsItemType.Model:          break;
                    case TsItemType.Company:        break;
                    case TsItemType.Service:        break;
                    case TsItemType.CutPlane:       break;
                    case TsItemType.Mover:          break;
                    case TsItemType.NoWeather:      break;
                    case TsItemType.City:           break;
                    case TsItemType.Hinge:          break;
                    case TsItemType.MapOverlay:     break;
                    case TsItemType.Ferry:          break;
                    case TsItemType.Sound:          break;
                    case TsItemType.Garage:         break;
                    case TsItemType.CameraPoint:    break;
                    case TsItemType.Trigger:        break;
                    case TsItemType.FuelPump:       break;
                    case TsItemType.RoadSideItem:   break;
                    case TsItemType.BusStop:        break;
                    case TsItemType.TrafficRule:    break;
                    case TsItemType.BezierPatch:    break;
                    case TsItemType.Compound:       break;
                    case TsItemType.TrajectoryItem: break;
                    case TsItemType.MapArea:        break;
                    case TsItemType.FarModel:       break;
                    case TsItemType.Curve:          break;
                    case TsItemType.Camera:         break;
                    case TsItemType.Cutscene:       break;
                    default: {
                        Log.Warning( "Unknown Type: {0} in {1} @ {2}", type, Path.GetFileName( path ), lastOffset );
                        break;
                    }
                }

                sector.ClearFileData();
            }
        }
    }
}