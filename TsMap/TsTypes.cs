using System;
using System.IO;

namespace TsMap
{

    public static class Common
    {
        public const int Ets2DlcGuardCount = 12; // TODO: Figure out how to get these dynamically
        public const int AtsDlcGuardCount = 19;
        public const float LaneWidth = 4.5f;
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

    [Flags]
    public enum ExportFlags
    {
        None = 0,
        TileMapInfo = 1,
        CityList = 2,
        CityDimensions = 4,
        CityLocalizedNames = 8,
        CountryList = 16,
        CountryLocalizedNames = 32,
        OverlayList = 64,
        OverlayPNGs = 128,
        All = int.MaxValue
    }

    public static class FlagMethods
    {
        public static bool IsActive(this RenderFlags self, RenderFlags value)
        {
            return (self & value) == value;
        }
        public static bool IsActive(this ExportFlags self, ExportFlags value)
        {
            return (self & value) == value;
        }
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
        Terrain = 1,
        Building = 2,
        Road = 3,
        Prefab = 4,
        Model = 5,
        Company = 6,
        Service = 7,
        CutPlane = 8,
        Mover = 9,
        NoWeather = 11,
        City = 12,
        Hinge = 13,
        MapOverlay = 18,
        Ferry = 19,
        Sound = 21,
        Garage = 22,
        CameraPoint = 23,
        Trigger = 34,
        FuelPump = 35, // services
        RoadSideItem = 36, // sign
        BusStop = 37,
        TrafficRule = 38, // traffic_area
        BezierPatch = 39,
        Compound = 40,
        TrajectoryItem = 41,
        MapArea = 42,
        FarModel = 43,
        Curve = 44,
        Camera = 45,
        Cutscene = 46,
    };


    // values from https://github.com/SCSSoftware/BlenderTools/blob/master/addon/io_scs_tools/consts.py
    public enum TsSpawnPointType
    {
        None = 0,
        TrailerPos = 1,
        UnloadEasyPos = 2,
        GasPos = 3,
        ServicePos = 4,
        TruckStopPos = 5,
        WeightStationPos = 6,
        TruckDealerPos = 7,
        Hotel = 8,
        Custom = 9,
        Parking = 10, // also shows parking in companies which don't work/show up in game
        Task = 11,
        MeetPos = 12,
        CompanyPos = 13,
        GaragePos = 14, // manage garage
        BuyPos = 15, // buy garage
        RecruitmentPos = 16,
        CameraPoint = 17,
        BusStation = 18,
        UnloadMediumPos = 19,
        UnloadHardPos = 20,
        UnloadRigidPos = 21,
        WeightCatPos = 22,
        CompanyUnloadPos = 23,
        TrailerSpawn = 24,
        LongTrailerPos = 25,
    }
}
