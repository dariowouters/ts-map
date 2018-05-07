using System;
using System.IO;

namespace TsMap.HashFiles
{
    internal static class Helper
    {
        internal static ushort ReadUshort(FileStream fs, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            const int size = sizeof(ushort);
            var tmp = new byte[size];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, size);
            return BitConverter.ToUInt16(tmp, 0);
        }

        internal static uint ReadUint(FileStream fs, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            const int size = sizeof(uint);
            var tmp = new byte[size];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, size);
            return BitConverter.ToUInt32(tmp, 0);
        }

        internal static int ReadInt(FileStream fs, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            const int size = sizeof(int);
            var tmp = new byte[size];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, size);
            return BitConverter.ToInt32(tmp, 0);
        }

        internal static long ReadLong(FileStream fs, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            const int size = sizeof(long);
            var tmp = new byte[size];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, size);
            return BitConverter.ToInt64(tmp, 0);
        }

        internal static ulong ReadUlong(FileStream fs, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            const int size = sizeof(ulong);
            var tmp = new byte[size];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, size);
            return BitConverter.ToUInt64(tmp, 0);
        }

        internal static byte[] ReadBytes(FileStream fs, long offset, int count, SeekOrigin origin = SeekOrigin.Begin)
        {
            var tmp = new byte[count];
            fs.Seek(offset, origin);
            fs.Read(tmp, 0, count);
            return tmp;
        }

        internal static string CombinePath(string firstPath, string secondPath)
        {
            var fullPath = Path.Combine(firstPath, secondPath);

            fullPath = fullPath.Replace('\\', '/');

            if (fullPath.StartsWith("/")) // absolute path
            {
                fullPath = fullPath.Substring(1);
            }

            return fullPath;
        }

        internal static string GetFilePath(string path, string currentPath = "")
        {
            if (path.StartsWith("/")) // absolute path
            {
                path = path.Substring(1);
                return path;
            }
            return CombinePath(currentPath, path);
        }
    }
}
