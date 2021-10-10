using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using TsMap2.Exceptions;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsOverlayEntry : AbstractScsEntry< Dictionary< ulong, TsMapOverlay > > {
        private readonly ScsOverlayIconEntry               _overlayIconEntry = new ScsOverlayIconEntry();
        private readonly Dictionary< ulong, TsMapOverlay > _overlays         = new Dictionary< ulong, TsMapOverlay >();
        private          ScsFile                           _currentMat;

        public Dictionary< ulong, TsMapOverlay > List() {
            VerifyRfs();

            ScsDirectory uiMapDirectory = Store.Rfs.GetDirectory( ScsPath.Def.MaterialUiMapPath );
            if ( uiMapDirectory == null ) {
                var message = $"[Job][MapOverlay] Could not read {ScsPath.Def.MaterialUiMapPath} dir";
                throw new ScsEntryException( message );
            }

            List< ScsFile > matFiles = uiMapDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
            if ( matFiles == null ) {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.ScsMatExtension} files";
                throw new ScsEntryException( message );
            }

            ScsDirectory uiCompanyDirectory = Store.Rfs.GetDirectory( ScsPath.Def.MaterialUiCompanyPath );
            if ( uiCompanyDirectory != null ) {
                List< ScsFile > data = uiCompanyDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
                if ( data != null ) matFiles.AddRange( data );
            } else {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.Def.MaterialUiCompanyPath} dir";
                throw new ScsEntryException( message );
            }

            ScsDirectory uiMapRoadDirectory = Store.Rfs.GetDirectory( ScsPath.Def.MaterialUiRoadPath );
            if ( uiMapRoadDirectory != null ) {
                List< ScsFile > data = uiMapRoadDirectory.GetFiles( $".{ScsPath.ScsMatExtension}" );
                if ( data != null ) matFiles.AddRange( data );
            } else {
                var message = $"[Job][MapOverlay] Could not read .{ScsPath.Def.MaterialUiRoadPath} dir";
                throw new ScsEntryException( message );
            }

            foreach ( ScsFile matFile in matFiles ) {
                _currentMat = matFile;
                byte[] data = matFile.Entry.Read();
                Generate( data );
            }

            return _overlays;
        }

        public override Dictionary< ulong, TsMapOverlay > Generate( byte[] stream ) {
            string[] lines = Encoding.UTF8.GetString( stream ).Split( '\n' );


            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                if ( !validLine ) continue;
                if ( key != "texture" ) continue;

                string objPath = ScsHelper.CombinePath( _currentMat.GetLocalPath(), value.Split( '"' )[ 1 ] );

                byte[] objData = Store.Rfs.GetFileEntry( objPath )?.Entry?.Read();

                if ( objData == null ) break;

                string path = ScsHelper.GetFilePath( Encoding.UTF8.GetString( objData, 0x30, objData.Length - 0x30 ) );

                string name = _currentMat.GetFileName();
                if ( name.StartsWith( "map" ) ) continue;
                if ( name.StartsWith( "road_" ) ) name = name.Substring( 5 );

                ulong        token   = ScsHashHelper.StringToToken( name );
                TsMapOverlay overlay = Parse( path, token );

                if ( overlay != null )
                    AddOverlay( overlay );
            }

            return _overlays;
        }

        private TsMapOverlay Parse( string path, ulong token ) {
            ScsFile file = Store.Rfs.GetFileEntry( path );

            if ( file != null ) {
                OverlayIcon icon = _overlayIconEntry.Get( path );

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

        private void AddOverlay( TsMapOverlay mapOverlay ) {
            if ( !_overlays.ContainsKey( mapOverlay.Token ) )
                _overlays.Add( mapOverlay.Token, mapOverlay );
        }
    }

    public class ScsOverlayIconEntry : AbstractScsEntry< OverlayIcon > {
        public override OverlayIcon Generate( byte[] stream ) {
            var overlay = new OverlayIcon();

            if ( stream.Length < 128 || MemoryHelper.ReadUInt32( stream, 0x00 ) != 0x20534444 || MemoryHelper.ReadUInt32( stream, 0x04 ) != 0x7C ) {
                Log.Error( "Invalid DDS file" );
                return null;
            }

            uint height = MemoryHelper.ReadUInt32( stream, 0x0C );
            uint width  = MemoryHelper.ReadUInt32( stream, 0x10 );

            uint        fourCc = MemoryHelper.ReadUInt32( stream, 0x54 );
            Color8888[] overlayRawData;

            if ( fourCc == 861165636 )
                overlayRawData = ScsOverlayHelper.ParseDxt3( stream, width, height );
            else if ( fourCc == 894720068 )
                overlayRawData = ScsOverlayHelper.ParseDxt5( stream, width, height );
            else
                overlayRawData = ScsOverlayHelper.ParseUncompressed( stream, width, height );

            if ( overlayRawData != null )
                overlay = new OverlayIcon( overlayRawData, width, height );

            return overlay;
        }
    }
}