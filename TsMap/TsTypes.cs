using System;
using System.IO;

namespace TsMap
{

    public static class Common
    {
        public const int BaseFileVersion130 = 853;
        public const int BaseFileVersion132 = 855;
        public const int BaseFileVersion133 = 858;
        public const int BaseFileVersion136 = 875;

        public const int Ets2DlcGuardCount = 10; // TODO: Figure out how to get these dynamically
        public const int AtsDlcGuardCount = 8;
    }

    [Flags]
    public enum RenderFlags
    {
        None = 0,
        TextOverlay = 1,
        Prefabs = 2,
        Roads = 4,
        MapAreas = 8,
        MapOverlays = 16,
        FerryConnections = 32,
        CityNames = 64,
        All = int.MaxValue
    }

    public class Mod
    {
        public string ModPath { get; set; }
        public bool Load { get; set; }

        public Mod(string path)
        {
            ModPath = path;
            Load = false;
        }

        public override string ToString()
        {
            return Path.GetFileName(ModPath);
        }
    }

    public enum TsItemType
    {
        Building = 0x01,
        Road = 0x03,
        Prefab = 0x04,
        Model = 0x05,
        Company = 0x06,
        Service = 0x07,
        CutPlane = 0x08,
        Dunno = 0x09,
        City = 0x0C,
        MapOverlay = 0x12,
        Ferry = 0x13,
        Garage = 0x16,
        Trigger = 0x22,
        FuelPump = 0x23,
        RoadSideItem = 0x24,
        BusStop = 0x25,
        TrafficRule = 0x26,
        TrajectoryItem = 0x29,
        MapArea = 0x2A,
    };

    public enum TsSpawnPointType
    {
        TrailerPickup = 0x01,
        Unk1, // company related
        Fuel,
        Service,
        Unk2, // seems unused
        WeightStation,
        TruckDealer,
        Unk3, // seems unused
        Unk4, // seems unused
        SomeParking, // also shows parking in companies which don't work/show up in game
        Unk5, // seems unused
        Unk6, // seems unused
        JobSelect,
        GarageIndoor, // manage garage
        GarageOutdoor, // buy garage
        Recruitment,
        Unk7, // seems unused
        BusStation,
        Unk8, // Something with companies ; TrailerDropOff ??
        Unk9, // Something with companies ; TrailerDropOff ??
    }
}
