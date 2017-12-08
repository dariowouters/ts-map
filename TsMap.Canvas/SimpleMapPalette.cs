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
            PrefabLight = new SolidBrush(Color.FromArgb(150, 150, 150)); // undiscovered
            PrefabDark = new SolidBrush(Color.FromArgb(75, 75, 75));
            PrefabGreen = new SolidBrush(Color.FromArgb(150, 150, 150)); // undiscovered

            CityName = Brushes.AliceBlue;

            Error = Brushes.LightCoral;
        }
    }
}
