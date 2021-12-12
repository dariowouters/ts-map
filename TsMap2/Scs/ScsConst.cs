﻿namespace TsMap2.Scs {
    public static class ScsConst {
        public const int Ets2DlcGuardCount = 12; // TODO: Figure out how to get these dynamically
        public const int AtsDlcGuardCount  = 19;
    }

    public enum ScsItemType {
        Terrain        = 1,
        Building       = 2,
        Road           = 3,
        Prefab         = 4,
        Model          = 5,
        Company        = 6,
        Service        = 7,
        CutPlane       = 8,
        Mover          = 9,
        NoWeather      = 11,
        City           = 12,
        Hinge          = 13,
        MapOverlay     = 18,
        Ferry          = 19,
        Sound          = 21,
        Garage         = 22,
        CameraPoint    = 23,
        Trigger        = 34,
        FuelPump       = 35, // services
        RoadSideItem   = 36, // sign
        BusStop        = 37,
        TrafficRule    = 38, // traffic_area
        BezierPatch    = 39,
        Compound       = 40,
        TrajectoryItem = 41,
        MapArea        = 42,
        FarModel       = 43,
        Curve          = 44,
        Camera         = 45,
        Cutscene       = 46,
        VisibilityArea = 48
    }


    // values from https://github.com/SCSSoftware/BlenderTools/blob/master/addon/io_scs_tools/consts.py
    public enum TsSpawnPointType {
        None             = 0,
        TrailerPos       = 1,
        UnloadEasyPos    = 2,
        GasPos           = 3,
        ServicePos       = 4,
        TruckStopPos     = 5,
        WeightStationPos = 6,
        TruckDealerPos   = 7,
        Hotel            = 8,
        Custom           = 9,
        Parking          = 10, // also shows parking in companies which don't work/show up in game
        Task             = 11,
        MeetPos          = 12,
        CompanyPos       = 13,
        GaragePos        = 14, // manage garage
        BuyPos           = 15, // buy garage
        RecruitmentPos   = 16,
        CameraPoint      = 17,
        BusStation       = 18,
        UnloadMediumPos  = 19,
        UnloadHardPos    = 20,
        UnloadRigidPos   = 21,
        WeightCatPos     = 22,
        CompanyUnloadPos = 23,
        TrailerSpawn     = 24,
        LongTrailerPos   = 25
    }

    public static class ScsPath {
        public const string GameVersion      = "version.txt";
        public const string ScsFileExtension = "sii";
        public const string ScsMatExtension  = "mat";

        public static class Def {
            public const string DefFolderName = "def";

            // -- TsGame
            public const string Ets2LogoScene = "def/ets2_logo_scene.sii";

            // -- TsCountry
            public const string CountryFileName = "country";

            // -- TsPrefab
            public const string WorldPath      = "def/world";
            public const string PrefabFileName = "prefab";

            // -- TsFerryConnection
            public const string FerryConnectionPath = "def/ferry/connection";

            // -- TsCity
            public const string CityFileName = "city";

            // -- TsRoadLook
            public const string RoadLook = "road_look";

            // -- TsOverlay
            public const           string MaterialUiMapPath     = "material/ui/map";
            public const           string MaterialUiCompanyPath = "material/ui/company/small";
            public const           string MaterialUiRoadPath    = "material/ui/map/road";
            public static readonly string CountryBaseName       = "custom/country.sii";
        }

        public static class Map {
            // -- TsMap
            public const string MapDirectory     = "map";
            public const string MapExtension     = "mbd";
            public const string MapFileExtension = ".base";
        }
    }
}