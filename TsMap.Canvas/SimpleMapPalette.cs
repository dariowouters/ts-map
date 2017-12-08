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
            PrefabLight = new SolidBrush(Color.FromArgb(100, 150, 150, 150));
            PrefabDark = new SolidBrush(Color.FromArgb(20, 20, 20));
            PrefabGreen = new SolidBrush(Color.FromArgb(50, 125, 125, 125));

            CityName = Brushes.AliceBlue;

            Error = Brushes.LightCoral;
        }
    }
}
