# TS Map - Map renderer

## About

This project are written on C#. This app are used to exctact datas from the game file and export as Json files or Png files

The main purpose about it, it's the map extraction. It's can create image of the game map.

You can use it on the opensource map like OpenLayer.

![Plop](docs/preview.png)


## What's can do ?

This project can: 

- Read the SCS files
- Extract POI( Ferry connexion, companies, route name, etc... )
- Extract cities list and position
- Generate map tiles

### Map available

|Map|Game|Version|Release|
|---|---|---|---|
|Base|Euro Truck Simulator 2|v1.40.3.3|N/A|
|Base|American Trucksimulator *|v1.40.3.3|N/A|
|Promod|Euro Truck Simulator 2 *|v2.50|N/A|
|Promod Canada|American Trucksimulator *|v2.50|N/A|

> Note: The tested mods load and get drawn but I haven't looked at anything specific so it's always possible there will be some items missing or things will get drawn that shouldn't.

### DLC Supported

|Game|Name|Release|
|---|---|---|
|Going East|Euro Truck Simulator 2|N/A|
|Scandinavia|Euro Truck Simulator 2|N/A|
|Vive La France|Euro Truck Simulator 2|N/A|
|Italia|Euro Truck Simulator 2|N/A|
|Beyond the Baltic Sea|Euro Truck Simulator 2|N/A|
|Road to the Black Sea|Euro Truck Simulator 2|N/A|
|Arizona|American Trucksimulator|N/A|
|New Mexico|American Trucksimulator|N/A|
|Oregon|American Trucksimulator|N/A|
|Washington|American Trucksimulator|N/A|
|Utah|American Trucksimulator|N/A|
|Colorado|American Trucksimulator|N/A|


#### Dependencies (NuGet)
- [DotNetZip](https://www.nuget.org/packages/DotNetZip/)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)

#### Based on
Fork of [dariowouters/ts-map](https://github.com/dariowouters/ts-mapp)

## License
Under the [MIT License](LICENSE)

© JAGFx - hey@emmanuel-smith.me
