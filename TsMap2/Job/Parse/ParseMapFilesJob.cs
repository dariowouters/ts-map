using System.Collections.Generic;
using System.Linq;
using Serilog;
using TsMap2.Helper;
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

            var _sectorFiles = new List< string >();

            foreach ( ScsFile file in mbd ) {
                string mapName = file.GetFileName();
                // this.IsEts2 = !( mapName == "usa" );

                ScsDirectory mapFileDir = this.Store().Rfs.GetDirectory( $"map/{mapName}" );
                if ( mapFileDir == null ) {
                    var message = $"[Job][MapFiles] Could not read map/{mapName} directory";
                    throw new JobException( message, this.JobName(), mapName );
                }

                _sectorFiles.AddRange( mapFileDir.GetFiles( ScsPath.Map.MapFileExtension ).Select( x => x.GetPath() ).ToList() );
            }

            // if ( _sectorFiles.Count <= 0 ) {

            // }

            // long preMapParseTime = DateTime.Now.Ticks;
            // this.Store().Sectors = _sectorFiles.Select( file => new TsSector( this, file ) ).ToList();
            // this.Store().Sectors.ForEach( sec => sec.Parse() );
            // this.Store().Sectors.ForEach( sec => sec.ClearFileData() );

            Log.Information( "[Job][MapFiles] Loaded. Found: {0}", this.Store().MapSectorFile.Count );
        }

        protected override void OnEnd() { }
    }
}