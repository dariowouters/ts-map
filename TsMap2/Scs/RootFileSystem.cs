using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TsMap2.Helper;

namespace TsMap2.Scs {
    public class ScsFile {
        private readonly string _entryPath;

        public ScsFile( ScsEntry entry, string path ) {
            this.Entry      = entry;
            this._entryPath = path;
        }

        public ScsEntry Entry { get; }

        /// <summary>
        ///     Will return full path
        ///     eg. def/city.sii
        /// </summary>
        public string GetPath() => this._entryPath;

        /// <summary>
        ///     Will return local path
        ///     eg. If full path is material/ui/test.mat this will return 'material/ui'
        /// </summary>
        public string GetLocalPath() {
            int lastSlash = this._entryPath.LastIndexOf( '/' );
            return this._entryPath.Substring( 0, lastSlash );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'test'
        /// </summary>
        public string GetFileName() {
            int lastSlash  = this._entryPath.LastIndexOf( '/' );
            int lastPeriod = this._entryPath.LastIndexOf( '.' );
            return this._entryPath.Substring( lastSlash + 1, lastPeriod - lastSlash - 1 );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'test.sii'
        /// </summary>
        public string GetFullName() {
            int lastSlash = this._entryPath.LastIndexOf( '/' );
            return this._entryPath.Substring( lastSlash + 1 );
        }

        /// <summary>
        ///     Returns name of the file only
        ///     eg. If full name is test.sii this will return 'sii'
        /// </summary>
        public string GetExtension() {
            int lastPeriod = this._entryPath.LastIndexOf( '.' );
            return this._entryPath.Substring( lastPeriod + 1 );
        }
    }

    public class ScsDirectory {
        private readonly RootFileSystem _rfs;

        public ScsDirectory( RootFileSystem rfs, string path ) {
            this.Directories = new Dictionary< ulong, ScsDirectory >();
            this.Files       = new Dictionary< ulong, ScsFile >();

            this._rfs      = rfs;
            this.EntryPath = path;
        }

        private string EntryPath { get; }

        private Dictionary< ulong, ScsDirectory > Directories { get; }

        private Dictionary< ulong, ScsFile > Files { get; }

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
                        string dirPath = Path.Combine( this.EntryPath, line.Substring( 1 ) );
                        dirPath = dirPath.Replace( '\\', '/' );

                        var   nextEntry = (ScsHashEntry) entry.GetRootFile().GetEntry( dirPath );
                        ulong nextHash  = CityHash.CityHash64( Encoding.UTF8.GetBytes( dirPath ), (ulong) dirPath.Length );

                        if ( nextEntry == null ) // Log.Msg( $"Could not find hash for '{dirPath}'" );
                            continue;

                        if ( !this.Directories.ContainsKey( nextHash ) ) {
                            var dir = new ScsDirectory( this._rfs, dirPath );
                            dir.AddHashEntry( nextEntry );
                            this.Directories.Add( nextHash, dir );
                        } else
                            this.Directories[ nextHash ].AddHashEntry( nextEntry );
                    } else // file
                    {
                        string filePath = Path.Combine( this.EntryPath, line );
                        filePath = filePath.Replace( '\\', '/' );

                        ulong    nextHash  = CityHash.CityHash64( Encoding.UTF8.GetBytes( filePath ), (ulong) filePath.Length );
                        ScsEntry nextEntry = entry.GetRootFile().GetEntry( filePath );

                        if ( nextEntry == null ) // Log.Msg( $"Could not find hash for '{filePath}'" );
                            continue;

                        if ( this.Files.ContainsKey( nextHash ) ) // Log.Msg($"File '{filePath}' already exists => overwriting");
                            this.Files[ nextHash ] = new ScsFile( nextEntry, filePath );
                        else
                            this.Files.Add( nextHash, new ScsFile( nextEntry, filePath ) );
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
                string newPath  = Path.Combine( this.EntryPath, path );
                newPath = newPath.Replace( '\\', '/' );

                if ( this.Files.ContainsKey( fileHash ) ) // Log.Msg($"File '{filePath}' already exists => overwriting");
                    this.Files[ fileHash ] = new ScsFile( entry, newPath );
                else
                    this.Files.Add( fileHash, new ScsFile( entry, newPath ) );

                return;
            }

            if ( path.StartsWith( "/" ) ) path = path.Substring( 1 );

            string currentDir = path.Substring( 0, slashIndex );
            string hashName   = ScsHelper.CombinePath( this.EntryPath, currentDir );
            ulong  hash       = CityHash.CityHash64( Encoding.UTF8.GetBytes( hashName ), (ulong) hashName.Length );

            if ( this.Directories.ContainsKey( hash ) )
                this.Directories[ hash ].AddZipEntry( entry, path.Substring( slashIndex + 1 ) );
            else {
                string newPath = Path.Combine( this.EntryPath, currentDir );
                newPath = newPath.Replace( '\\', '/' );

                var dir = new ScsDirectory( this._rfs, newPath );
                dir.AddZipEntry( entry, path.Substring( slashIndex + 1 ) );

                this.Directories.Add( hash, dir );
            }
        }

        public void AddDirectoryManually( string path, ScsEntry entry ) {
            this.Directories.Add( entry.GetHash(), new ScsDirectory( this._rfs, path ) );
        }

        public string GetCurrentDirectoryName() {
            if ( !this.EntryPath.Contains( '/' ) ) return this.EntryPath;
            string[] pathParts = this.EntryPath.Split( '/' );
            return pathParts[ pathParts.Length - 1 ];
        }

        public List< ScsFile > GetFiles( string filter = "" ) {
            return this.Files.Values.Where( x => x.GetFullName().Contains( filter ) ).ToList();
        }

        public ScsFile GetFileEntry( ulong hash ) {
            if ( this.Files.ContainsKey( hash ) ) return this.Files[ hash ];

            foreach ( KeyValuePair< ulong, ScsDirectory > scsDirectory in this.Directories ) {
                ScsFile res = scsDirectory.Value.GetFileEntry( hash );
                if ( res != null ) return res;
            }

            return null;
        }

        public ScsDirectory GetDirectory( ulong hash ) {
            if ( this.Directories.ContainsKey( hash ) ) return this.Directories[ hash ];

            foreach ( KeyValuePair< ulong, ScsDirectory > scsDirectory in this.Directories ) {
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
            this._path = path;

            this.Files = new Dictionary< string, ScsRootFile >();

            this.AddSourceDirectory( path );
        }

        public Dictionary< string, ScsRootFile > Files { get; }

        public void AddSourceFile( string path ) {
            FileStream f = File.OpenRead( path );
            f.Seek( 0, SeekOrigin.Begin );
            var buff = new byte[ 4 ];
            f.Read( buff, 0, 4 );

            if ( BitConverter.ToUInt32( buff, 0 ) == ScsMagic ) this.Files.Add( path, new HashFile( path, this ) );
            else this.Files.Add( path,                                                new ScsZipFile( path, this ) );
        }

        public void AddSourceDirectory( string path ) {
            string[] scsFiles = Directory.GetFiles( path, "*.scs" );

            foreach ( string scsFile in scsFiles ) this.AddSourceFile( scsFile );
        }

        public void AddHashEntry( ScsHashEntry entry ) {
            if ( this._rootDirectory == null ) {
                this._rootDirectory = new ScsDirectory( this, "" );
                this._rootDirectory.AddHashEntry( entry );
            } else
                this._rootDirectory.AddHashEntry( entry );
        }

        public void AddZipEntry( ScsZipEntry entry, string path ) {
            if ( this._rootDirectory == null ) {
                this._rootDirectory = new ScsDirectory( this, "" );
                this._rootDirectory.AddZipEntry( entry, path );
            } else
                this._rootDirectory.AddZipEntry( entry, path );
        }

        public ScsDirectory GetRootDirectory() => this._rootDirectory;

        public ScsDirectory GetDirectory( ulong hash ) => this._rootDirectory?.GetDirectory( hash );

        public ScsDirectory GetDirectory( string name ) => this.GetDirectory( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong) name.Length ) );

        public ScsFile GetFileEntry( ulong hash ) => this._rootDirectory?.GetFileEntry( hash );

        public ScsFile GetFileEntry( string name ) => this.GetFileEntry( CityHash.CityHash64( Encoding.UTF8.GetBytes( name ), (ulong) name.Length ) );
    }
}