using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TsMap.HashFiles
{

    public class ScsFile
    {
        private readonly string _entryPath;

        public ScsHashEntry Entry { get; }

        public ScsFile(ScsHashEntry entry, string path)
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

        public ScsDirectory(ScsHashEntry entry, string path)
        {
            if (entry == null) return;

            EntryPath = path;
            Directories = new Dictionary<ulong, ScsDirectory>();
            Files = new Dictionary<ulong, ScsFile>();
            AddDirEntry(entry);
        }

        public void AddDirEntry(ScsHashEntry entry)
        {
            if (entry == null) return;

            var data = entry.Read();
            var contents = Encoding.UTF8.GetString(data);
            if (contents.Contains("*") || contents.Contains(".")) // dir/file list
            {
                var lines = contents.Split('\n');
                foreach (var line in lines) // loop through file/dir list
                {
                    if (line.StartsWith("*")) // dirs
                    {
                        var dirPath = Helper.CombinePath(EntryPath, line.Substring(1));
                        var nextHash = CityHash.CityHash64(Encoding.UTF8.GetBytes(dirPath), (ulong)dirPath.Length);
                        var nextEntry = entry.Hf.GetEntry(nextHash);

                        if (nextEntry == null)
                        {
                            Log.Msg($"Could not find hash for '{dirPath}'");
                            continue;
                        }

                        if (!Directories.ContainsKey(nextHash))
                        {
                            Directories.Add(nextHash, new ScsDirectory(nextEntry, dirPath));
                        }
                        else
                        {
                            Directories[nextHash].AddDirEntry(nextEntry);
                        }
                    }
                    else // file
                    {
                        var filePath = Helper.CombinePath(EntryPath, line);
                        var nextHash = CityHash.CityHash64(Encoding.UTF8.GetBytes(filePath), (ulong)filePath.Length);
                        var nextEntry = entry.Hf.GetEntry(nextHash);

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
        private string _path;
        private readonly Dictionary<string, HashFile> _hashFiles;

        private ScsDirectory _rootDirectory;

        public RootFileSystem(string path)
        {
            _path = path;

            _hashFiles = new Dictionary<string, HashFile>();

            var scsFiles = Directory.GetFiles(path, "*.scs");

            if (scsFiles.Length < 2)
            {
                Log.Msg("[Error] Needs atleast 2 .scs files (base.scs and def.scs)");
                return;
            }

            foreach (var scsFile in scsFiles)
            {
                _hashFiles.Add(scsFile, new HashFile(scsFile, this));
            }
        }

        public void AddDirEntry(ScsHashEntry entry)
        {
            if (_rootDirectory == null)
            {
                _rootDirectory = new ScsDirectory(entry, "");
            }
            else
            {
                _rootDirectory.AddDirEntry(entry);
            }
        }

        public ScsDirectory GetDirectory(ulong hash)
        {
            return _rootDirectory.GetDirectory(hash);
        }

        public ScsDirectory GetDirectory(string name)
        {
            return GetDirectory(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong)name.Length));
        }

        public ScsFile GetFileEntry(ulong hash)
        {
            return _rootDirectory.GetFileEntry(hash);
        }

        public ScsFile GetFileEntry(string name)
        {
            return GetFileEntry(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong) name.Length));
        }
    }
}
