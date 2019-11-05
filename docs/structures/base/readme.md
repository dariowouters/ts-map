## File Structures

These are the file structures for .base files, for the versions that were still available on steam.

It's always still possible there are incorrect values/structures and there definitely are values that have incorrect/guessed names. But the structures should be correct enough to load the base game(s) and dlc files as these templates are what I use to create the program.

Here is a list of what file version corresponds to what game version, some versions are missing due to not having a backup of the game files.

The file versions can be found in each .base file as the first 4 bytes.

File Version | ATS | ETS2 | Extra
:---: | :---: | :---: | ---
825 | - | 1.22
826 | 1.00 | - | Might be on ETS2 1.23 and/or 1.24 (no backup on steam)
829 | 1.03 | 1.25 | On 1.25 the map files from steam were 829
831 | - | 1.25 | The editor on 1.25 generated 831 files
834 | 1.05 | 1.26
836 | 1.06 | 1.27

From here the game versions are synced between the games

File Version | Game Version(s) | Extra
:---: | :---: | ---
836 | 1.28 | ATS moved to same version number as ETS2
846 | 1.29 | ETS2 skipped 1.29
847 | 1.30
854 | 1.31
855 | 1.32
858 | 1.33 & 1.34
869 | 1.35
875 | 1.36

These files are made using [010 editor](https://www.sweetscape.com/010editor/), but if you just want to see the structures you can just read them in a text editor.

If you are using 010 editor, use base-template.bt for the correct version.
