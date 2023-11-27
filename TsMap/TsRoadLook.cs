﻿using System;
using System.Collections.Generic;
using TsMap.Common;

namespace TsMap
{
    public class TsRoadLook
    {
        public ulong Token { get; }

        public float Offset;

        public readonly List<string> LanesLeft;
        public readonly List<string> LanesRight;

        public static double Hermite(float s, float x, float z, double tanX, double tanZ)
        {
            double h1 = 2 * Math.Pow(s, 3) - 3 * Math.Pow(s, 2) + 1;
            double h2 = -2 * Math.Pow(s, 3) + 3 * Math.Pow(s, 2);
            double h3 = Math.Pow(s, 3) - 2 * Math.Pow(s, 2) + s;
            double h4 = Math.Pow(s, 3) - Math.Pow(s, 2);
            return h1 * x + h2 * z + h3 * tanX + h4 * tanZ;
        }

        public TsRoadLook(ulong token)
        {
            LanesLeft = new List<string>();
            LanesRight = new List<string>();
            Token = token;
        }

        public float GetWidth()
        {
            return Offset + Consts.LaneWidth * LanesLeft.Count + Consts.LaneWidth * LanesRight.Count;
        }

        public string GetRoadType()
        {
            if (LanesLeft.Count > 0) return LanesLeft[0];
            else if (LanesRight.Count > 0) return LanesRight[0];
            return null;
        }

        public bool IsOneWay()
        {
            return (this.LanesLeft.Count == 0 && this.LanesRight.Count > 0) || (this.LanesRight.Count == 0 && this.LanesLeft.Count > 0);
        }
    }
}
