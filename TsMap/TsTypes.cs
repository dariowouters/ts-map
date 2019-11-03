using System;
using System.IO;

namespace TsMap
{

    public static class Common
    {
        public const int Ets2DlcGuardCount = 8; // TODO: Figure out how to get these dynamically
        public const int AtsDlcGuardCount = 6;

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


    // values from https://github.com/SCSSoftware/BlenderTools/blob/master/addon/io_scs_tools/consts.py
    public enum TsSpawnPointType
    {
        None = 0x00,
        TrailerPos,
        UnloadEasyPos,
        GasPos,
        ServicePos,
        TruckStopPos,
        WeightStationPos,
        TruckDealerPos,
        Hotel,
        Custom,
        Parking, // also shows parking in companies which don't work/show up in game
        Task,
        MeetPos,
        CompanyPos,
        GaragePos, // manage garage
        BuyPos, // buy garage
        RecruitmentPos,
        CameraPoint,
        BusStation,
        UnloadMediumPos,
        UnloadHardPos,
        UnloadRigidPos,
        WeightCatPos,
        CompanyUnloadPos,
        TrailerSpawn,
        LongTrailerPos,
    }
}