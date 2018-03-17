using System.Drawing;

namespace TsMap.Canvas
{
    public class SimpleMapPalette : MapPalette
    {
        public SimpleMapPalette()
        {
            Background = new SolidBrush(Color.FromArgb(72, 78, 102));
            Road = Brushes.Wheat;
            PrefabRoad = Brushes.Wheat;
            PrefabLight = Brushes.Wheat;
            PrefabDark = new SolidBrush(Color.FromArgb(50, 75, 75, 75));
            PrefabGreen = Brushes.Wheat; // TODO: Check if green has a specific z-index

            CityName = Brushes.AliceBlue;

            FerryLines = new SolidBrush(Color.FromArgb(80, 255, 255, 255));

            Error = Brushes.LightCoral;
        }
    }
}
