using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
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


            var sectorFiles = new List< string >();
            foreach ( ScsFile file in mbd ) {
                string mapName = file.GetFileName();
                // this.IsEts2 = !( mapName == "usa" );

                ScsDirectory mapFileDir = this.Store().Rfs.GetDirectory( $"map/{mapName}" );
                if ( mapFileDir == null ) {
                    var message = $"[Job][MapFiles] Could not read map/{mapName} directory";
                    throw new JobException( message, this.JobName(), mapName );
                }

                sectorFiles = mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList();
                // sectorFiles.AddRange( mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList() );

                foreach ( string sectorFile in sectorFiles )
                    this.Parse( sectorFile );
            }

            // if ( _sectorFiles.Count <= 0 ) {

            // }

            // long preMapParseTime = DateTime.Now.Ticks;
            // this.Store().Sectors = _sectorFiles.Select( file => new TsSector( sector, file ) ).ToList();
            // sectorFiles.ForEach( this.Parse );
            // this.Store().Sectors.ForEach( sec => sec.ClearFileData() );

            Log.Information( "[Job][MapFiles] Loaded. Roads: {0}",            this.Store().Map.Roads.Count );
            Log.Information( "[Job][MapFiles] Loaded. Prefabs: {0}",          this.Store().Map.Prefabs.Count );
            Log.Information( "[Job][MapFiles] Loaded. Companies: {0}",        this.Store().Map.Companies.Count );
            Log.Information( "[Job][MapFiles] Loaded. Cities: {0}",           this.Store().Map.Cities.Count );
            Log.Information( "[Job][MapFiles] Loaded. MapOverlays: {0}",      this.Store().Map.MapOverlays.Count );
            Log.Information( "[Job][MapFiles] Loaded. FerryConnections: {0}", this.Store().Map.FerryConnections.Count );
            Log.Information( "[Job][MapFiles] Loaded. Triggers: {0}",         this.Store().Map.Triggers.Count );
            Log.Information( "[Job][MapFiles] Loaded. MapAreas: {0}",         this.Store().Map.MapAreas.Count );
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

                TsItem item;
                switch ( type ) {
                    case TsItemType.Road: {
                        item       =  new TsRoadItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.Roads.Add( (TsRoadItem) item );
                        break;
                    }
                    case TsItemType.Prefab: {
                        item       =  new TsPrefabItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.Prefabs.Add( (TsPrefabItem) item );
                        break;
                    }
                    case TsItemType.Company: {
                        item       =  new TsCompanyItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.Companies.Add( (TsCompanyItem) item );
                        break;
                    }
                    case TsItemType.Service: {
                        item       =  new TsServiceItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.CutPlane: {
                        item       =  new TsCutPlaneItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.City: {
                        item       =  new TsCityItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.Cities.Add( (TsCityItem) item );
                        break;
                    }
                    case TsItemType.MapOverlay: {
                        item       =  new TsMapOverlayItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.MapOverlays.Add( (TsMapOverlayItem) item );
                        break;
                    }
                    case TsItemType.Ferry: {
                        item       =  new TsFerryItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.FerryConnections.Add( (TsFerryItem) item );
                        break;
                    }
                    case TsItemType.Garage: {
                        item       =  new TsGarageItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.Trigger: {
                        item       =  new TsTriggerItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.Triggers.Add( (TsTriggerItem) item );
                        break;
                    }
                    case TsItemType.FuelPump: {
                        item       =  new TsFuelPumpItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.RoadSideItem: {
                        item       =  new TsRoadSideItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.BusStop: {
                        item       =  new TsBusStopItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.TrafficRule: {
                        item       =  new TsTrafficRuleItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.TrajectoryItem: {
                        item       =  new TsTrajectoryItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.MapArea: {
                        item       =  new TsMapAreaItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        if ( item.Valid ) this.Store().Map.MapAreas.Add( (TsMapAreaItem) item );
                        break;
                    }
                    case TsItemType.Cutscene: {
                        item       =  new TsCutsceneItem( sector, lastOffset );
                        lastOffset += item.BlockSize;
                        break;
                    }
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