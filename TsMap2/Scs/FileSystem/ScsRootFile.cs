using System.Collections.Generic;
using System.IO;
using System.Text;
using TsMap2.Helper;

namespace TsMap2.Scs.FileSystem {
    internal class ScsHeader {
        public uint   Magic       { get; set; }
        public ushort Version     { get; set; }
        public ushort Salt        { get; set; }
        public uint   HashMethod  { get; set; }
        public int    EntryCount  { get; set; }
        public int    StartOffset { get; set; }

        public override string ToString() =>
            $"Magic {Magic}\n"
            + $"Version {Version}\n"
            + $"Salt {Salt}\n"
            + $"HashMethod {HashMethod}\n"
            + $"EntryCount {EntryCount}\n"
            + $"StartOffset {StartOffset}";
    }


    public abstract class ScsRootFile {
        protected readonly string         Path;
        protected readonly RootFileSystem Rfs;

        protected ScsRootFile( string path, RootFileSystem rfs ) {
            Path = path;
            Rfs  = rfs;
        }

        public abstract ScsEntry GetEntry( string name );

        public abstract List< ScsEntry > GetEntriesValues();
    }

    public class ScsZipFile : ScsRootFile {
        private readonly Dictionary< string, ScsZipEntry > _entries;

        public ScsZipFile( string path, RootFileSystem rfs ) : base( path, rfs ) {
            if ( !File.Exists( Path ) ) return;

            // this.Br       = new BinaryReader( File.OpenRead( this.Path ) );
            _entries = new Dictionary< string, ScsZipEntry >();

            Br = new BinaryReader( ScsHelper.WaitForFile( Path, FileMode.Open, FileAccess.Read, FileShare.Read ) );
            ushort entryCount = ScsHelper.ReadUInt16( Br, -22 + 10, SeekOrigin.End );

            var fileOffset = 0;

            for ( var i = 0; i < entryCount; i++ ) {
                var entry = new ScsZipEntry( this ) {
                    CompressionMethod = ScsHelper.ReadUInt16( Br, fileOffset += 8 ),
                    CompressedSize    = ScsHelper.ReadInt32( Br, fileOffset += 10 ),
                    Size              = ScsHelper.ReadInt32( Br, fileOffset += 4 ),
                    NameLength        = (short)ScsHelper.ReadUInt16( Br, fileOffset += 4 )
                };

                ushort extraFieldLength = ScsHelper.ReadUInt16( Br, fileOffset += 2 );
                Br.BaseStream.Seek( fileOffset += 2, SeekOrigin.Begin );
                entry.Name = Encoding.UTF8.GetString( Br.ReadBytes( entry.NameLength ) );

                // FIXME: This cause crash... Commented
                // Br.Dispose();

                fileOffset   += entry.NameLength + extraFieldLength;
                entry.Offset =  fileOffset; // Offset to data

                fileOffset += entry.CompressedSize;

                if ( entry.CompressedSize != 0 ) // only files
                {
                    string filePath = entry.Name.Replace( '\\', '/' );
                    Rfs.AddZipEntry( entry, filePath );
                }

                _entries.Add( entry.Name, entry );
            }
        }

        public BinaryReader Br { get; }
        // public BinaryReader Br => new BinaryReader( ScsHelper.WaitForFile( Path, FileMode.Open, FileAccess.Read, FileShare.Read ) );

        public override ScsEntry GetEntry( string name ) =>
            _entries.ContainsKey( name )
                ? _entries[ name ]
                : null;

        public override List< ScsEntry > GetEntriesValues() {
            var entries = new List< ScsEntry >();
            foreach ( ScsZipEntry scsZipEntry in _entries.Values ) entries.Add( scsZipEntry );

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
            if ( !File.Exists( Path ) ) return;

            // this.Br = new BinaryReader( File.OpenRead( this.Path ) );
            Entries = new Dictionary< ulong, ScsHashEntry >();

            BinaryReader br = Br;
            Header = new ScsHeader {
                Magic       = ScsHelper.ReadUInt32( br, 0x0 ),
                Version     = ScsHelper.ReadUInt16( br, 0x04 ),
                Salt        = ScsHelper.ReadUInt16( br, 0x06 ),
                HashMethod  = ScsHelper.ReadUInt32( br, 0x08 ),
                EntryCount  = ScsHelper.ReadInt32( br, 0x0C ),
                StartOffset = ScsHelper.ReadInt32( br, 0x10 )
            };

            if ( Header.Magic != Magic ) // Log.Msg( "Incorrect File Structure" );
                return;

            if ( Header.HashMethod != HashMethod ) // Log.Msg( "Incorrect Hash Method" );
                return;

            if ( Header.Version != SupportedHashVersion ) // Log.Msg( "Unsupported Hash Version" );
                return;

            br.BaseStream.Seek( Header.StartOffset, SeekOrigin.Begin );
            byte[] entriesRaw = br.ReadBytes( Header.EntryCount * EntryBlockSize );
            br.Dispose();

            for ( var i = 0; i < Header.EntryCount; i++ ) {
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

                Entries.Add( entry.Hash, entry );
            }

            ScsHashEntry rootDir = GetEntry( RootDirHash );

            if ( rootDir == null || rootDir.Size == 0 ) // Try to add important sub directories directly
            {
                var defEntry = (ScsHashEntry)GetEntry( "def" );
                if ( defEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "def" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "def", defEntry );
                    dir = Rfs.GetDirectory( "def" );
                    dir?.AddHashEntry( defEntry );
                }

                var defWorldEntry = (ScsHashEntry)GetEntry( "def/world" );
                if ( defWorldEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "def/world" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "def/world", defWorldEntry );
                    dir = Rfs.GetDirectory( "def/world" );
                    dir?.AddHashEntry( defWorldEntry );
                }

                var mapEntry = (ScsHashEntry)GetEntry( "map" );
                if ( mapEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "map" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "map", mapEntry );
                    dir = Rfs.GetDirectory( "map" );
                    dir?.AddHashEntry( mapEntry );
                }

                var materialEntry = (ScsHashEntry)GetEntry( "material" );
                if ( materialEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "material" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "material", materialEntry );
                    dir = Rfs.GetDirectory( "material" );
                    dir?.AddHashEntry( materialEntry );
                }

                var prefabEntry = (ScsHashEntry)GetEntry( "prefab" );
                if ( prefabEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "prefab" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "prefab", prefabEntry );
                    dir = Rfs.GetDirectory( "prefab" );
                    dir?.AddHashEntry( prefabEntry );
                }

                var prefab2Entry = (ScsHashEntry)GetEntry( "prefab2" );
                if ( prefab2Entry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "prefab2" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "prefab2", prefab2Entry );
                    dir = Rfs.GetDirectory( "prefab2" );
                    dir?.AddHashEntry( prefab2Entry );
                }

                var modelEntry = (ScsHashEntry)GetEntry( "model" );
                if ( modelEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "model" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "model", modelEntry );
                    dir = Rfs.GetDirectory( "model" );
                    dir?.AddHashEntry( modelEntry );
                }

                var model2Entry = (ScsHashEntry)GetEntry( "model2" );
                if ( model2Entry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "model2" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "model2", model2Entry );
                    dir = Rfs.GetDirectory( "model2" );
                    dir?.AddHashEntry( model2Entry );
                }

                var localeEntry = (ScsHashEntry)GetEntry( "locale" );
                if ( localeEntry != null ) {
                    ScsDirectory dir = Rfs.GetDirectory( "locale" );
                    if ( dir == null ) Rfs.GetRootDirectory()?.AddDirectoryManually( "locale", localeEntry );
                    dir = Rfs.GetDirectory( "locale" );
                    dir?.AddHashEntry( localeEntry );
                }
            } else
                Rfs.AddHashEntry( rootDir );
        }

        // public BinaryReader Br { get; }
        public BinaryReader Br => new BinaryReader( ScsHelper.WaitForFile( Path, FileMode.Open, FileAccess.Read, FileShare.Read ) );

        private ScsHeader Header { get; }

        public override string ToString() => Path.Substring( Path.LastIndexOf( '\\' ) + 1 );

        private ScsHashEntry GetEntry( ulong hash ) =>
            Entries.ContainsKey( hash )
                ? Entries[ hash ]
                : null;

        public sealed override ScsEntry GetEntry( string name ) => GetEntry( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong)name.Length ) );

        public override List< ScsEntry > GetEntriesValues() {
            var entries = new List< ScsEntry >();
            foreach ( ScsHashEntry scsHashEntry in Entries.Values ) entries.Add( scsHashEntry );

            return entries;
        }
    }
}