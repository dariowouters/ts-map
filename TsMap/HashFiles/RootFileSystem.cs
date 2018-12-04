using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TsMap.HashFiles
{
    public class ScsFile
    {
        private readonly string _entryPath;

        public ScsEntry Entry { get; }

        public ScsFile(ScsEntry entry, string path)
        {
            Entry = entry;
            _entryPath = path;
        }

        /// <summary>
        /// Will return full path
        /// eg. def/city.sii
        /// </summary>
        public string GetPath()
        {
            return _entryPath;
        }

        /// <summary>
        /// Will return local path
        /// eg. If full path is material/ui/test.mat this will return 'material/ui'
        /// </summary>
        public string GetLocalPath()
        {
            var lastSlash = _entryPath.LastIndexOf('/');
            return _entryPath.Substring(0, lastSlash);
        }

        /// <summary>
        /// Returns name of the file only
        /// eg. If full name is test.sii this will return 'test'
        /// </summary>
        public string GetFileName()
        {
            var lastSlash = _entryPath.LastIndexOf('/');
            var firstPeriod = _entryPath.IndexOf('.');
            return _entryPath.Substring(lastSlash + 1, firstPeriod - lastSlash - 1);
        }

        /// <summary>
        /// Returns name of the file only
        /// eg. If full name is test.sii this will return 'test.sii'
        /// </summary>
        public string GetFullName()
        {
            var lastSlash = _entryPath.LastIndexOf('/');
            return _entryPath.Substring(lastSlash + 1);
        }

        /// <summary>
        /// Returns name of the file only
        /// eg. If full name is test.sii this will return 'sii'
        /// </summary>
        public string GetExtension()
        {
            var firstPeriod = _entryPath.IndexOf('.');
            return _entryPath.Substring(firstPeriod + 1);
        }
    }

    public class ScsDirectory
    {
        public string EntryPath { get; }

        public Dictionary<ulong, ScsDirectory> Directories { get; }

        public Dictionary<ulong, ScsFile> Files { get; }

        private RootFileSystem _rfs;

        public ScsDirectory(RootFileSystem rfs, string path)
        {
            Directories = new Dictionary<ulong, ScsDirectory>();
            Files = new Dictionary<ulong, ScsFile>();

            _rfs = rfs;
            EntryPath = path;
        }

        public void AddHashEntry(ScsHashEntry entry)
        {
            if (entry == null) return;

            var data = entry.Read();
            var contents = Encoding.UTF8.GetString(data);
            if (entry.IsDirectory())
            {
                var lines = contents.Split('\n');
                foreach (var line in lines) // loop through file/dir list
                {
                    if (line.Equals("")) continue;

                    if (line.StartsWith("*")) // dirs
                    {
                        var dirPath = Path.Combine(EntryPath, line.Substring(1));
                        dirPath = dirPath.Replace('\\', '/');

                        var nextEntry = (ScsHashEntry) entry.GetRootFile().GetEntry(dirPath);
                        var nextHash = CityHash.CityHash64(Encoding.UTF8.GetBytes(dirPath), (ulong)dirPath.Length);

                        if (nextEntry == null)
                        {
                            Log.Msg($"Could not find hash for '{dirPath}'");
                            continue;
                        }

                        if (!Directories.ContainsKey(nextHash))
                        {
                            var dir = new ScsDirectory(_rfs, dirPath);
                            dir.AddHashEntry(nextEntry);
                            Directories.Add(nextHash, dir);
                        }
                        else
                        {
                            Directories[nextHash].AddHashEntry(nextEntry);
                        }
                    }
                    else // file
                    {
                        var filePath = Path.Combine(EntryPath, line);
                        filePath = filePath.Replace('\\', '/');

                        var nextHash = CityHash.CityHash64(Encoding.UTF8.GetBytes(filePath), (ulong)filePath.Length);
                        var nextEntry = entry.GetRootFile().GetEntry(filePath);

                        if (nextEntry == null)
                        {
                            Log.Msg($"Could not find hash for '{filePath}'");
                            continue;
                        }

                        if (Files.ContainsKey(nextHash))
                        {
                            // Log.Msg($"File '{filePath}' already exists => overwriting");
                            Files[nextHash] = new ScsFile(nextEntry, filePath);
                        }
                        else
                        {
                            Files.Add(nextHash, new ScsFile(nextEntry, filePath));
                        }
                    }
                }
            }
        }

        public void AddZipEntry(ScsZipEntry entry, string path)
        {
            if (entry == null || path == "") return;

            var slashIndex = path.IndexOf("/", StringComparison.Ordinal);

            if (slashIndex == -1) // no slash found => end of path = file location
            {
                var fileHash = entry.GetHash();
                var newPath = Path.Combine(EntryPath, path);
                newPath = newPath.Replace('\\', '/');

                if (Files.ContainsKey(fileHash))
                {
                    // Log.Msg($"File '{filePath}' already exists => overwriting");
                    Files[fileHash] = new ScsFile(entry, newPath);
                }
                else
                {
                    Files.Add(fileHash, new ScsFile(entry, newPath));
                }

                return;
            }

            if (path.StartsWith("/")) path = path.Substring(1);

            var currentDir = path.Substring(0, slashIndex);
            var hashName = Helper.CombinePath(EntryPath, currentDir);
            var hash = CityHash.CityHash64(Encoding.UTF8.GetBytes(hashName), (ulong) hashName.Length);

            if (Directories.ContainsKey(hash))
            {
                Directories[hash].AddZipEntry(entry, path.Substring(slashIndex + 1));
            }
            else
            {
                var newPath = Path.Combine(EntryPath, currentDir);
                newPath = newPath.Replace('\\', '/');

                var dir = new ScsDirectory(_rfs, newPath);
                dir.AddZipEntry(entry, path.Substring(slashIndex + 1));

                Directories.Add(hash, dir);
            }
        }

        public void AddDirectoryManually(string path, ScsEntry entry)
        {
            Directories.Add(entry.GetHash(), new ScsDirectory(_rfs, path));
        }

        public string GetCurrentDirectoryName()
        {
            if (!EntryPath.Contains('/')) return EntryPath;
            var pathParts = EntryPath.Split('/');
            return pathParts[pathParts.Length - 1];
        }

        public List<ScsFile> GetFiles(string filter = "")
        {
            return Files.Values.Where(x => x.GetFullName().Contains(filter)).ToList();
        }

        public ScsFile GetFileEntry(ulong hash)
        {
            if (Files.ContainsKey(hash))
            {
                return Files[hash];
            }

            foreach (var scsDirectory in Directories)
            {
                var res = scsDirectory.Value.GetFileEntry(hash);
                if (res != null) return res;
            }

            return null;
        }

        public ScsDirectory GetDirectory(ulong hash)
        {
            if (Directories.ContainsKey(hash))
            {
                return Directories[hash];
            }

            foreach (var scsDirectory in Directories)
            {
                var res = scsDirectory.Value.GetDirectory(hash);
                if (res != null) return res;
            }

            return null;
        }
    }

    public class RootFileSystem
    {

        /// <summary>
        /// SCS#
        /// </summary>
        private const uint ScsMagic = 592659283;

        private string _path;
        public Dictionary<string, ScsRootFile> Files { get; }

        private ScsDirectory _rootDirectory;

        public RootFileSystem(string path)
        {
            _path = path;

            Files = new Dictionary<string, ScsRootFile>();

            AddSourceDirectory(path);
        }

        public void AddSourceFile(string path)
        {
            var f = File.OpenRead(path);
            f.Seek(0, SeekOrigin.Begin);
            var buff = new byte[4];
            f.Read(buff, 0, 4);

            if (BitConverter.ToUInt32(buff, 0) == ScsMagic) Files.Add(path, new HashFile(path, this));
            else Files.Add(path, new ScsZipFile(path, this));
        }

        public void AddSourceDirectory(string path)
        {
            var scsFiles = Directory.GetFiles(path, "*.scs");

            foreach (var scsFile in scsFiles)
            {
                AddSourceFile(scsFile);
            }
        }

        public void AddHashEntry(ScsHashEntry entry)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = new ScsDirectory(this, "");
                _rootDirectory.AddHashEntry(entry);
            }
            else
            {
                _rootDirectory.AddHashEntry(entry);
            }
        }

        public void AddZipEntry(ScsZipEntry entry, string path)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = new ScsDirectory(this, "");
                _rootDirectory.AddZipEntry(entry, path);
            }
            else
            {
                _rootDirectory.AddZipEntry(entry, path);
            }
        }

        public ScsDirectory GetRootDirectory()
        {
            return _rootDirectory;
        }

        public ScsDirectory GetDirectory(ulong hash)
        {
            return _rootDirectory?.GetDirectory(hash);
        }

        public ScsDirectory GetDirectory(string name)
        {
            return GetDirectory(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong) name.Length));
        }

        public ScsFile GetFileEntry(ulong hash)
        {
            return _rootDirectory?.GetFileEntry(hash);
        }

        public ScsFile GetFileEntry(string name)
        {
            return GetFileEntry(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong) name.Length));
        }
    }
}
