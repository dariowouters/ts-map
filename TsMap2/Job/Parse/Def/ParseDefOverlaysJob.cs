using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using TsMap2.Factory;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefOverlaysJob : ThreadJob {
        private bool _isFirstFileRead;

        protected override void Do() {
            Log.Debug( "[Job][MapOverlay] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][MapOverlay] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory uiMapDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.MaterialUiMapPath );
            if ( uiMapDirectory == null ) {
                var message = $"[Job][MapOverlay] Could not read {ScsPath.Def.MaterialUiMapPath} dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.MaterialUiMapPath );
            }

            List< ScsFile > matFiles = uiMapDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
            if ( matFiles == null ) {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.ScsMatExtension} files";
                throw new JobException( message, this.JobName(), ScsPath.ScsMatExtension );
            }

            ScsDirectory uiCompanyDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.MaterialUiCompanyPath );
            if ( uiCompanyDirectory != null ) {
                List< ScsFile > data = uiCompanyDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
                if ( data != null ) matFiles.AddRange( data );
            } else {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.Def.MaterialUiCompanyPath} dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.MaterialUiCompanyPath );
            }

            ScsDirectory uiMapRoadDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.MaterialUiRoadPath );
            if ( uiMapRoadDirectory != null ) {
                List< ScsFile > data = uiMapRoadDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
                if ( data != null ) matFiles.AddRange( data );
            } else {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.Def.MaterialUiRoadPath} dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.MaterialUiRoadPath );
            }

            var isFirstFileRead = false;
            foreach ( ScsFile matFile in matFiles ) {
                byte[]   data  = matFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                // -- Raw generation
                if ( !isFirstFileRead ) {
                    RawHelper.SaveRawFile( RawType.OVERLAY, matFile.GetFullName(), data );
                    isFirstFileRead = true;
                }
                // -- ./Raw generation

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                    if ( !validLine ) continue;
                    if ( key != "texture" ) continue;

                    string objPath = ScsHelper.CombinePath( matFile.GetLocalPath(), value.Split( '"' )[ 1 ] );

                    byte[] objData = this.Store().Rfs.GetFileEntry( objPath )?.Entry?.Read();

                    if ( objData == null ) break;

                    string path = ScsHelper.GetFilePath( Encoding.UTF8.GetString( objData, 0x30, objData.Length - 0x30 ) );

                    string name = matFile.GetFileName();
                    if ( name.StartsWith( "map" ) ) continue;
                    if ( name.StartsWith( "road_" ) ) name = name.Substring( 5 );

                    ulong token = ScsHash.StringToToken( name );

                    this.Store().Def.AddOverlay( this.Parse( path, token ) );
                }
            }

            Log.Information( "[Job][MapOverlay] Loaded. Found: {0}", this.Store().Def.Overlays.Count );
        }

        private TsMapOverlay Parse( string path, ulong token ) {
            ScsFile file = this.Store().Rfs.GetFileEntry( path );

            // Log.Debug( path );

            if ( file != null ) {
                OverlayIcon icon = this.ParseOverlayIcon( file );

                if ( !icon.Valid ) return null;

                var overlayBitmap = new Bitmap( (int) icon.Width, (int) icon.Height, PixelFormat.Format32bppArgb );

                BitmapData bd = overlayBitmap.LockBits( new Rectangle( 0, 0, overlayBitmap.Width, overlayBitmap.Height ), ImageLockMode.WriteOnly,
                                                        PixelFormat.Format32bppArgb );

                IntPtr ptr = bd.Scan0;

                Marshal.Copy( icon.GetData(), 0, ptr, bd.Width * bd.Height * 0x4 );

                overlayBitmap.UnlockBits( bd );

                return new TsMapOverlay( overlayBitmap, token );
            }

            Log.Warning( "Map Overlay file not found, {0}", path );
            return null;
        }

        private OverlayIcon ParseOverlayIcon( ScsFile file ) {
            byte[] stream  = file.Entry.Read();
            var    overlay = new OverlayIcon();

            if ( stream.Length < 128 || MemoryHelper.ReadUInt32( stream, 0x00 ) != 0x20534444 || MemoryHelper.ReadUInt32( stream, 0x04 ) != 0x7C ) {
                Log.Error( "Invalid DDS file. | {0}", file.GetPath() );
                return null;
            }

            // -- Raw generation
            if ( !this._isFirstFileRead ) {
                RawHelper.SaveRawFile( RawType.OVERLAY, file.GetFullName(), stream );
                this._isFirstFileRead = true;
            }
            // -- ./Raw generation

            uint height = MemoryHelper.ReadUInt32( stream, 0x0C );
            uint width  = MemoryHelper.ReadUInt32( stream, 0x10 );

            uint        fourCc = MemoryHelper.ReadUInt32( stream, 0x54 );
            Color8888[] overlayRawData;

            if ( fourCc == 861165636 )
                overlayRawData = ScsOverlayHelper.ParseDxt3( stream, width, height );
            else if ( fourCc == 894720068 )
                overlayRawData = ScsOverlayHelper.ParseDxt5( stream, width, height );
            else
                overlayRawData = ScsOverlayHelper.ParseUncompressed( file.GetFileName(), stream, width, height );

            if ( overlayRawData != null )
                overlay = new OverlayIcon( overlayRawData, width, height );

            return overlay;
        }
    }
}