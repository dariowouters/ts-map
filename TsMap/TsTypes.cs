using System;

namespace TsMap
{

    public static class Common
    {
        public const int BaseFileVersion130 = 853;
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
        unk6, // seems unused
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