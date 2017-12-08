using System.Collections.Generic;

namespace TsMap
{
    public class TsRoadLook
    {
        public string LookId { get; private set; }

        public float Offset;
        public float SizeLeft;
        public float SizeRight;
        public float ShoulderLeft;
        public float ShoulderRight;

        public readonly List<string> LanesLeft;
        public readonly List<string> LanesRight;

        public TsRoadLook(string look)
        {
            LanesLeft = new List<string>();
            LanesRight = new List<string>();
            LookId = look;
        }

        public float GetWidth()
        {
            return Offset + 4.5f + LanesLeft.Count + 4.5f * LanesRight.Count;
        }

    }
}
