## Euro Truck Simulator 2 / American Truck Simulator Map Renderer

This Application reads ATS/ETS2 files to draw roads, prefabs, (some) map overlays and city names.

#### 1.31 (ATS/ETS2 open beta) Supported

### Supported maps / DLC
- ATS
    - Base
    - Nevada
    - Arizona
    - New Mexico
- ETS2
    - Base
    - Going East!
    - Scandinavia
    - Vive la France !
    - Italia

### Setting up
#### Folder structure
```
map_data_directory (the directory you set in TsMapCanvas.cs)
├───LUT
│       cities.json
│       ferryPorts.json
│       overlays.json
│       prefabs.json
│       roads.json
└───SCS
    ├───LUT
    │   ├───road
    │   │       road_look.sii
    │   │       road_look.template.dlc_fr.sii
    │   │       road_look.template.sii
    │   └───ferryConnection
    │           all ferry connection files (*.sii)
    ├───map
    │       all *.base files
    ├───overlay
    │       *.dds map icon files
    └───prefab
            all *.ppd files in subfolders
```

In order to run the program, you'll need to manually extract some \*.scs files.

Use the [SCS extractor](http://modding.scssoft.com/wiki/Documentation/Tools/Game_Archive_Extractor) to extract \*.SCS files.

Files you'll have to extract:
- base.scs
- def.scs
- Any map dlc you want to render (e.g. dlc_fr.scs)

Easy way to copy all \*.ppd files (command-line) `Robocopy c:\source\ c:\destination\ *.ppd /s`

- Base map
    - Map data
        - This is located in base.scs at `map/europe/` or `map/usa/`
        - Put all \*.base files in `SCS/map/`
    - Overlays
        - Put all \*.dds files in `SCS/overlay/`
        - Located in
            - `base/material/ui/map/`
            - `base/material/ui/map/road/`
            - `base/material/ui/company/small/`
    - Prefab information
        - Located in base.scs, copy all \*.ppd files (with directories) from `prefab/` and `prefab2/` to `SCS/prefab/`
        - ***Also copy all \*.ppd files from `model/` and `model2/` to `SCS/prefab/`***
    - Sii road look files
        - Located in def.scs at `def/world/`
        - Copy road_look.sii and road_look.template.sii to `SCS/LUT/road/`
    - ferry connection Files
        - Located in def.scs at `def/ferry/connection/`
        - copy all \*.sii files to `SCS/LUT/ferryConnection/`


- DLC (Vive la France ! as an example)
    - Raw map information
        - This is located in dlc_fr.scs at `map/europe/`
        - Put all \*.base files in `SCS/map/`
    - Prefab information **(some DLC won't have these)**
        - Located in dlc_fr.scs, copy all \*.ppd files (with directories) from `prefab/` and `prefab2/` to `SCS/prefab/`
        - ***Also copy all \*.ppd files from `model/` and `model2/` to `SCS/prefab/`***
    - Sii road look files **(some DLC won't have these)**
        - Also Located in dlc_fr.scs at `def/world/`
        - Copy road_look.sii and road_look.template.sii to `SCS/LUT/road/`
    - ferry connection Files **(some DLC won't have these)**
        - Also Located in dlc_fr.scs at `def/ferry/connection/`
        - copy all \*.sii files to `SCS/LUT/ferryConnection/`

[Original project](https://github.com/nlhans/ets2-map)
