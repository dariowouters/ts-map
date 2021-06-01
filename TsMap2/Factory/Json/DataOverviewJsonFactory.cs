using System;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;

namespace TsMap2.Factory.Json {
    public class DataOverviewJsonFactory : JsonFactory< JObject > {
        public override string GetFileName() => AppPath.DataOverview;

        public override string GetSavingPath() => Store.Settings.OutputPath;

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override JObject Convert( JObject raw ) => throw new NotImplementedException();

        public override JContainer RawData() =>
            new JObject {
                [ "def_cities" ]           = StoreHelper.Instance.Def.Cities.Count,
                [ "def_countries" ]        = StoreHelper.Instance.Def.Countries.Count,
                [ "def_ferryConnections" ] = StoreHelper.Instance.Def.FerryConnections.Count,
                [ "def_overlays" ]         = StoreHelper.Instance.Def.Overlays.Count,
                [ "def_prefab" ]           = StoreHelper.Instance.Def.Prefabs.Count,
                [ "def_roadLook" ]         = StoreHelper.Instance.Def.RoadLooks.Count,
                [ "map_cities" ]           = StoreHelper.Instance.Map.Cities.Count,
                [ "map_companies" ]        = StoreHelper.Instance.Map.Companies.Count,
                [ "map_ferryConnections" ] = StoreHelper.Instance.Map.FerryConnections.Count,
                [ "map_areas" ]            = StoreHelper.Instance.Map.MapAreas.Count,
                [ "map_overlays" ]         = StoreHelper.Instance.Map.MapOverlays.Count,
                [ "map_prefab" ]           = StoreHelper.Instance.Map.Prefabs.Count,
                [ "map_road" ]             = StoreHelper.Instance.Map.Roads.Count,
                [ "map_triggers" ]         = StoreHelper.Instance.Map.Triggers.Count,
                [ "map_maxX" ]             = StoreHelper.Instance.Map.MaxX,
                [ "map_maxZ" ]             = StoreHelper.Instance.Map.MaxZ,
                [ "map_minX" ]             = StoreHelper.Instance.Map.MinX,
                [ "map_minZ" ]             = StoreHelper.Instance.Map.MinZ,
                [ "map_nodes" ]            = StoreHelper.Instance.Map.Nodes.Count,
                [ "item_all" ] = StoreHelper.Instance.Map.Overlays.Company.Count
                                 + StoreHelper.Instance.Map.Overlays.Ferry.Count
                                 + StoreHelper.Instance.Map.Overlays.Fuel.Count
                                 + StoreHelper.Instance.Map.Overlays.Garage.Count
                                 + StoreHelper.Instance.Map.Overlays.Overlay.Count
                                 + StoreHelper.Instance.Map.Overlays.Parking.Count
                                 + StoreHelper.Instance.Map.Overlays.Recruitment.Count
                                 + StoreHelper.Instance.Map.Overlays.Service.Count
                                 + StoreHelper.Instance.Map.Overlays.Train.Count
                                 + StoreHelper.Instance.Map.Overlays.TruckDealer.Count
                                 + StoreHelper.Instance.Map.Overlays.WeightStation.Count,
                [ "item_companies" ]     = StoreHelper.Instance.Map.Overlays.Company.Count,
                [ "item_ferries" ]       = StoreHelper.Instance.Map.Overlays.Ferry.Count,
                [ "item_fuel" ]          = StoreHelper.Instance.Map.Overlays.Fuel.Count,
                [ "item_garage" ]        = StoreHelper.Instance.Map.Overlays.Garage.Count,
                [ "item_overlay" ]       = StoreHelper.Instance.Map.Overlays.Overlay.Count,
                [ "item_parking" ]       = StoreHelper.Instance.Map.Overlays.Parking.Count,
                [ "item_recruitment" ]   = StoreHelper.Instance.Map.Overlays.Recruitment.Count,
                [ "item_service" ]       = StoreHelper.Instance.Map.Overlays.Service.Count,
                [ "item_train" ]         = StoreHelper.Instance.Map.Overlays.Train.Count,
                [ "item_truckDealer" ]   = StoreHelper.Instance.Map.Overlays.TruckDealer.Count,
                [ "item_weightStation" ] = StoreHelper.Instance.Map.Overlays.WeightStation.Count
            };
    }
}