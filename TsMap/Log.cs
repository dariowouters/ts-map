using System;
using System.IO;

namespace TsMap
{
    public static class Log
    {

        private static TextWriter _tw;

        private static void OpenWriter()
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ts-map");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                _tw = TextWriter.Synchronized(File.CreateText(Path.Combine(logDir, "TsMap.log")));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void Msg(string text)
        {
            if (_tw == null) OpenWriter();
            Console.WriteLine(text);
            if (_tw == null) return;
            _tw.WriteLine(text);
            _tw.Flush();
        }
    }
}
