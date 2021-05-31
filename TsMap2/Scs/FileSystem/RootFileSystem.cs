using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TsMap2.Helper;

namespace TsMap2.Scs.FileSystem {
    public class ScsFile {
        private readonly string _entryPath;

        public ScsFile( ScsEntry entry, string path ) {
            Entry      = entry;
            _entryPath = path;
        }

        public ScsEntry Entry { get; }

        /// <summary>
        ///     Will return full path
        ///     eg. def/city.sii
        /// </summary>
        public string GetPath() => _entryPath;

        /// <summary>
        ///     Will return local path
        ///     eg. If full path is material/ui/test.mat this will return 'material/ui'
        /// </summary>
        public string GetLocalPath() {
            int lastSlash = _entryPath.LastIndexOf( '/' );
            return _entryPath.Substring( 0, lastSlash );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'test'
        /// </summary>
        public string GetFileName() {
            int lastSlash  = _entryPath.LastIndexOf( '/' );
            int lastPeriod = _entryPath.LastIndexOf( '.' );
            return _entryPath.Substring( lastSlash + 1, lastPeriod - lastSlash - 1 );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'test.sii'
        /// </summary>
        public string GetFullName() {
            int lastSlash = _entryPath.LastIndexOf( '/' );
            return _entryPath.Substring( lastSlash + 1 );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'sii'
        /// </summary>
        public string GetExtension() {
            int lastPeriod = _entryPath.LastIndexOf( '.' );
            return _entryPath.Substring( lastPeriod + 1 );
        }
    }

    public class ScsDirectory {
        private readonly RootFileSystem _rfs;

        public ScsDirectory( RootFileSystem rfs, string path ) {
            Directories = new Dictionary< ulong, ScsDirectory >();
            Files       = new Dictionary< ulong, ScsFile >();

            _rfs      = rfs;
            EntryPath = path;
        }

        private string EntryPath { get; }

        public Dictionary< ulong, ScsDirectory > Directories { get; }

        public Dictionary< ulong, ScsFile > Files { get; }

        public void AddHashEntry( ScsHashEntry entry ) {
            if ( entry == null ) return;

            byte[] data     = entry.Read();
            string contents = Encoding.UTF8.GetString( data );
            if ( entry.IsDirectory() ) {
                string[] lines = contents.Split( '\n' );
                foreach ( string line in lines ) // loop through file/dir list
                {
                    if ( line.Equals( "" ) ) continue;

                    if ( line.StartsWith( "*" ) ) // dirs
                    {
                        string dirPath = Path.Combine( EntryPath, line.Substring( 1 ) );
                        dirPath = dirPath.Replace( '\\', '/' );

                        var   nextEntry = (ScsHashEntry) entry.GetRootFile().GetEntry( dirPath );
                        ulong nextHash  = CityHash.CityHash64( Encoding.UTF8.GetBytes( dirPath ), (ulong) dirPath.Length );

                        if ( nextEntry == null ) // Log.Msg( $"Could not find hash for '{dirPath}'" );
                            continue;

                        if ( !Directories.ContainsKey( nextHash ) ) {
                            var dir = new ScsDirectory( _rfs, dirPath );
                            dir.AddHashEntry( nextEntry );
                            Directories.Add( nextHash, dir );
                        } else
                            Directories[ nextHash ].AddHashEntry( nextEntry );
                    } else // file
                    {
                        string filePath = Path.Combine( EntryPath, line );
                        filePath = filePath.Replace( '\\', '/' );

                        ulong    nextHash  = CityHash.CityHash64( Encoding.UTF8.GetBytes( filePath ), (ulong) filePath.Length );
                        ScsEntry nextEntry = entry.GetRootFile().GetEntry( filePath );

                        if ( nextEntry == null ) // Log.Msg( $"Could not find hash for '{filePath}'" );
                            continue;

                        if ( Files.ContainsKey( nextHash ) ) // Log.Msg($"File '{filePath}' already exists => overwriting");
                            Files[ nextHash ] = new ScsFile( nextEntry, filePath );
                        else
                            Files.Add( nextHash, new ScsFile( nextEntry, filePath ) );
                    }
                }
            }
        }

        public void AddZipEntry( ScsZipEntry entry, string path ) {
            if ( entry == null || path == "" ) return;

            int slashIndex = path.IndexOf( "/", StringComparison.Ordinal );

            if ( slashIndex == -1 ) // no slash found => end of path = file location
            {
                ulong  fileHash = entry.GetHash();
                string newPath  = Path.Combine( EntryPath, path );
                newPath = newPath.Replace( '\\', '/' );

                if ( Files.ContainsKey( fileHash ) ) // Log.Msg($"File '{filePath}' already exists => overwriting");
                    Files[ fileHash ] = new ScsFile( entry, newPath );
                else
                    Files.Add( fileHash, new ScsFile( entry, newPath ) );

                return;
            }

            if ( path.StartsWith( "/" ) ) path = path.Substring( 1 );

            string currentDir = path.Substring( 0, slashIndex );
            string hashName   = ScsHelper.CombinePath( EntryPath, currentDir );
            ulong  hash       = CityHash.CityHash64( Encoding.UTF8.GetBytes( hashName ), (ulong) hashName.Length );

            if ( Directories.ContainsKey( hash ) )
                Directories[ hash ].AddZipEntry( entry, path.Substring( slashIndex + 1 ) );
            else {
                string newPath = Path.Combine( EntryPath, currentDir );
                newPath = newPath.Replace( '\\', '/' );

                var dir = new ScsDirectory( _rfs, newPath );
                dir.AddZipEntry( entry, path.Substring( slashIndex + 1 ) );

                Directories.Add( hash, dir );
            }
        }

        public void AddDirectoryManually( string path, ScsEntry entry ) {
            Directories.Add( entry.GetHash(), new ScsDirectory( _rfs, path ) );
        }

        public string GetCurrentDirectoryName() {
            if ( !EntryPath.Contains( '/' ) ) return EntryPath;
            string[] pathParts = EntryPath.Split( '/' );
            return pathParts[ pathParts.Length - 1 ];
        }

        public List< ScsFile > GetFiles( string filter = "" ) {
            return Files.Values.Where( x => x.GetFullName().Contains( filter ) ).ToList();
        }

        public ScsFile GetFileEntry( ulong hash ) {
            if ( Files.ContainsKey( hash ) ) return Files[ hash ];

            foreach ( KeyValuePair< ulong, ScsDirectory > scsDirectory in Directories ) {
                ScsFile res = scsDirectory.Value.GetFileEntry( hash );
                if ( res != null ) return res;
            }

            return null;
        }

        public ScsDirectory GetDirectory( ulong hash ) {
            if ( Directories.ContainsKey( hash ) ) return Directories[ hash ];

            foreach ( KeyValuePair< ulong, ScsDirectory > scsDirectory in Directories ) {
                ScsDirectory res = scsDirectory.Value.GetDirectory( hash );
                if ( res != null ) return res;
            }

            return null;
        }
    }

    public class RootFileSystem {
        /// <summary>
        ///     SCS#
        /// </summary>
        private const uint ScsMagic = 592659283;

        private string _path;

        private ScsDirectory _rootDirectory;

        public RootFileSystem( string path ) {
            _path = path;

            Files = new Dictionary< string, ScsRootFile >();

            AddSourceDirectory( path );
        }

        public Dictionary< string, ScsRootFile > Files { get; }

        public void AddSourceFile( string path ) {
            FileStream f = File.OpenRead( path );
            f.Seek( 0, SeekOrigin.Begin );
            var buff = new byte[ 4 ];
            f.Read( buff, 0, 4 );

            if ( BitConverter.ToUInt32( buff, 0 ) == ScsMagic ) Files.Add( path, new HashFile( path, this ) );
            else Files.Add( path,                                                new ScsZipFile( path, this ) );
        }

        public void AddSourceDirectory( string path ) {
            string[] scsFiles = Directory.GetFiles( path, "*.scs" );

            foreach ( string scsFile in scsFiles ) AddSourceFile( scsFile );
        }

        public void AddHashEntry( ScsHashEntry entry ) {
            if ( _rootDirectory == null ) {
                _rootDirectory = new ScsDirectory( this, "" );
                _rootDirectory.AddHashEntry( entry );
            } else
                _rootDirectory.AddHashEntry( entry );
        }

        public void AddZipEntry( ScsZipEntry entry, string path ) {
            if ( _rootDirectory == null ) {
                _rootDirectory = new ScsDirectory( this, "" );
                _rootDirectory.AddZipEntry( entry, path );
            } else
                _rootDirectory.AddZipEntry( entry, path );
        }

        public ScsDirectory GetRootDirectory() => _rootDirectory;

        public ScsDirectory GetDirectory( ulong hash ) => _rootDirectory?.GetDirectory( hash );

        public ScsDirectory GetDirectory( string name ) => GetDirectory( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong) name.Length ) );

        public ScsFile GetFileEntry( ulong hash ) => _rootDirectory?.GetFileEntry( hash );

        public ScsFile GetFileEntry( string name ) => GetFileEntry( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong) name.Length ) );
    }
}