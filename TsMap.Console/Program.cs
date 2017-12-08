namespace TsMap.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            TsMapper mapper = new TsMapper("D:/Projects/ts-map-files/europe/");
            mapper.Parse();
            System.Console.ReadLine();
        }
    }
}
