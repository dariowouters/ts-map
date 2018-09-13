namespace TsMap.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            TsMapper mapper = new TsMapper("D:/Games/steamapps/common/Euro Truck Simulator 2/");
            mapper.Parse();
            // System.Console.ReadLine();
        }
    }
}
