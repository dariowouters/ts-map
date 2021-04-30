using System.Collections.Generic;
using System.IO;
using System.Text;
using TsMap2.Helper;

namespace TsMap2.ScsHash {
    internal class ScsHeader {
        public uint   Magic       { get; set; }
        public ushort Version     { get; set; }
        public ushort Salt        { get; set; }
        public uint   HashMethod  { get; set; }
        public int    EntryCount  { get; set; }
        public int    StartOffset { get; set; }

        public override string ToString() =>
            $"Magic {this.Magic}\n"
            + $"Version {this.Version}\n"
            + $"Salt {this.Salt}\n"
            + $"HashMethod {this.HashMethod}\n"
            + $"EntryCount {this.EntryCount}\n"
            + $"StartOffset {this.StartOffset}";
    }


    public abstract class ScsRootFile {
        protected readonly string         Path;
        protected readonly RootFileSystem Rfs;

        protected ScsRootFile( string path, RootFileSystem rfs ) {
            this.Path = path;
            this.Rfs  = rfs;
        }

        public abstract ScsEntry GetEntry( string name );

        public abstract List< ScsEntry > GetEntriesValues();
    }

    public class ScsZipFile : ScsRootFile {
        private readonly Dictionary< string, ScsZipEntry > _entries;

        public ScsZipFile( string path, RootFileSystem rfs ) : base( path, rfs ) {
            if ( !File.Exists( this.Path ) ) return;

            this.Br       = new BinaryReader( File.OpenRead( this.Path ) );
            this._entries = new Dictionary< string, ScsZipEntry >();

            ushort entryCount = ScsHelper.ReadUInt16( this.Br, -22 + 10, SeekOrigin.End );

            var fileOffset = 0;

            for ( var i = 0; i < entryCount; i++ ) {
                var entry = new ScsZipEntry( this ) {
                    CompressionMethod = ScsHelper.ReadUInt16( this.Br, fileOffset += 8 ),
                    CompressedSize    = ScsHelper.ReadInt32( this.Br, fileOffset += 10 ),
                    Size              = ScsHelper.ReadInt32( this.Br, fileOffset += 4 ),
                    NameLength        = (short) ScsHelper.ReadUInt16( this.Br, fileOffset += 4 )
                };

                ushort extraFieldLength = ScsHelper.ReadUInt16( this.Br, fileOffset += 2 );
                this.Br.BaseStream.Seek( fileOffset += 2, SeekOrigin.Begin );
                entry.Name = Encoding.UTF8.GetString( this.Br.ReadBytes( entry.NameLength ) );

                fileOffset   += entry.NameLength + extraFieldLength;
                entry.Offset =  fileOffset; // Offset to data

                fileOffset += entry.CompressedSize;

                if ( entry.CompressedSize != 0 ) // only files
                {
                    string filePath = entry.Name.Replace( '\\', '/' );
                    this.Rfs.AddZipEntry( entry, filePath );
                }

                this._entries.Add( entry.Name, entry );
            }
        }

        public BinaryReader Br { get; }

        public override ScsEntry GetEntry( string name ) =>
            this._entries.ContainsKey( name )
                ? this._entries[ name ]
                : null;

        public override List< ScsEntry > GetEntriesValues() {
            var entries = new List< ScsEntry >();
            foreach ( ScsZipEntry scsZipEntry in this._entries.Values ) entries.Add( scsZipEntry );

            return entries;
        }
    }

    /// <summary>
    ///     Used to read CityHash .scs files
    /// </summary>
    public class HashFile : ScsRootFile {
        /// <summary>
        ///     SCS#
        /// </summary>
        private const uint Magic = 592659283;

        /// <summary>
        ///     CITY
        /// </summary>
        private const uint HashMethod = 1498696003;

        private const ushort SupportedHashVersion = 1;
        private const ushort HeaderBlockSize      = 0x14;
        private const ushort EntryBlockSize       = 0x20;

        /// <summary>
        ///     CityHash64("")
        /// </summary>
        private const ulong RootDirHash = 11160318154034397263;

        public Dictionary< ulong, ScsHashEntry > Entries;

        public HashFile( string filePath, RootFileSystem rfs ) : base( filePath, rfs ) {
            if ( !File.Exists( this.Path ) ) return;

            this.Br      = new BinaryReader( File.OpenRead( this.Path ) );
            this.Entries = new Dictionary< ulong, ScsHashEntry >();

            this.Header = new ScsHeader {
                Magic       = ScsHelper.ReadUInt32( this.Br, 0x0 ),
                Version     = ScsHelper.ReadUInt16( this.Br, 0x04 ),
                Salt        = ScsHelper.ReadUInt16( this.Br, 0x06 ),
                HashMethod  = ScsHelper.ReadUInt32( this.Br, 0x08 ),
                EntryCount  = ScsHelper.ReadInt32( this.Br, 0x0C ),
                StartOffset = ScsHelper.ReadInt32( this.Br, 0x10 )
            };

            if ( this.Header.Magic != Magic ) // Log.Msg( "Incorrect File Structure" );
                return;

            if ( this.Header.HashMethod != HashMethod ) // Log.Msg( "Incorrect Hash Method" );
                return;

            if ( this.Header.Version != SupportedHashVersion ) // Log.Msg( "Unsupported Hash Version" );
                return;

            this.Br.BaseStream.Seek( this.Header.StartOffset, SeekOrigin.Begin );
            byte[] entriesRaw = this.Br.ReadBytes( this.Header.EntryCount * EntryBlockSize );

            for ( var i = 0; i < this.Header.EntryCount; i++ ) {
                int offset = i * EntryBlockSize;
                var entry = new ScsHashEntry {
                    Hash           = MemoryHelper.ReadUInt64( entriesRaw, offset ),
                    Offset         = MemoryHelper.ReadInt64( entriesRaw, offset  + 0x08 ),
                    Flags          = MemoryHelper.ReadUInt32( entriesRaw, offset + 0x10 ),
                    Crc            = MemoryHelper.ReadUInt32( entriesRaw, offset + 0x14 ),
                    Size           = MemoryHelper.ReadInt32( entriesRaw, offset  + 0x18 ),
                    CompressedSize = MemoryHelper.ReadInt32( entriesRaw, offset  + 0x1C ),
                    Hf             = this
                };

                this.Entries.Add( entry.Hash, entry );
            }

            ScsHashEntry rootDir = this.GetEntry( RootDirHash );

            if ( rootDir == null || rootDir.Size == 0 ) // Try to add important sub directories directly
            {
                var defEntry = (ScsHashEntry) this.GetEntry( "def" );
                if ( defEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "def" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "def", defEntry );
                    dir = this.Rfs.GetDirectory( "def" );
                    dir?.AddHashEntry( defEntry );
                }

                var defWorldEntry = (ScsHashEntry) this.GetEntry( "def/world" );
                if ( defWorldEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "def/world" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "def/world", defWorldEntry );
                    dir = this.Rfs.GetDirectory( "def/world" );
                    dir?.AddHashEntry( defWorldEntry );
                }

                var mapEntry = (ScsHashEntry) this.GetEntry( "map" );
                if ( mapEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "map" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "map", mapEntry );
                    dir = this.Rfs.GetDirectory( "map" );
                    dir?.AddHashEntry( mapEntry );
                }

                var materialEntry = (ScsHashEntry) this.GetEntry( "material" );
                if ( materialEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "material" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "material", materialEntry );
                    dir = this.Rfs.GetDirectory( "material" );
                    dir?.AddHashEntry( materialEntry );
                }

                var prefabEntry = (ScsHashEntry) this.GetEntry( "prefab" );
                if ( prefabEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "prefab" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "prefab", prefabEntry );
                    dir = this.Rfs.GetDirectory( "prefab" );
                    dir?.AddHashEntry( prefabEntry );
                }

                var prefab2Entry = (ScsHashEntry) this.GetEntry( "prefab2" );
                if ( prefab2Entry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "prefab2" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "prefab2", prefab2Entry );
                    dir = this.Rfs.GetDirectory( "prefab2" );
                    dir?.AddHashEntry( prefab2Entry );
                }

                var modelEntry = (ScsHashEntry) this.GetEntry( "model" );
                if ( modelEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "model" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "model", modelEntry );
                    dir = this.Rfs.GetDirectory( "model" );
                    dir?.AddHashEntry( modelEntry );
                }

                var model2Entry = (ScsHashEntry) this.GetEntry( "model2" );
                if ( model2Entry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "model2" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "model2", model2Entry );
                    dir = this.Rfs.GetDirectory( "model2" );
                    dir?.AddHashEntry( model2Entry );
                }

                var localeEntry = (ScsHashEntry) this.GetEntry( "locale" );
                if ( localeEntry != null ) {
                    ScsDirectory dir = this.Rfs.GetDirectory( "locale" );
                    if ( dir == null ) this.Rfs.GetRootDirectory()?.AddDirectoryManually( "locale", localeEntry );
                    dir = this.Rfs.GetDirectory( "locale" );
                    dir?.AddHashEntry( localeEntry );
                }
            } else
                this.Rfs.AddHashEntry( rootDir );
        }

        public BinaryReader Br { get; }

        private ScsHeader Header { get; }

        public override string ToString() => this.Path.Substring( this.Path.LastIndexOf( '\\' ) + 1 );

        private ScsHashEntry GetEntry( ulong hash ) =>
            this.Entries.ContainsKey( hash )
                ? this.Entries[ hash ]
                : null;

        public sealed override ScsEntry GetEntry( string name ) => this.GetEntry( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong) name.Length ) );

        public override List< ScsEntry > GetEntriesValues() {
            var entries = new List< ScsEntry >();
            foreach ( ScsHashEntry scsHashEntry in this.Entries.Values ) entries.Add( scsHashEntry );

            return entries;
        }
    }
}