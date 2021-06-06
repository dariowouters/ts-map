using System;
using Microsoft.Extensions.CommandLineUtils;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Helper;
using TsMap2.Job;
using TsMap2.Job.Export;
using TsMap2.Job.Export.Tiles;
using TsMap2.Job.Parse.Def;
using TsMap2.Job.Parse.Map;
using TsMap2.Job.Parse.Overlays;
using TsMap2.Model;

namespace TsMap2 {
    public class CommandLine {
        private readonly CommandLineApplication _app;

        public CommandLine( string[] args ) {
            _app = new CommandLineApplication {
                Name        = "TsMap2.exe",
                FullName    = "Sample App",
                Description = "Description de l'application"
            };

            Options();
            CommandExport();

            _app.OnExecute( () => {
                _app.ShowHelp();
                return 0;
            } );

            try {
                _app.Execute( args );
            } catch ( CommandParsingException ex ) {
                Console.WriteLine( ex.Message );
                _app.ShowHelp();
            }
        }

        private void Options() {
            _app.HelpOption( "-?|-h|--help" );
            _app.VersionOption( "--version", "1.0.0" );
        }

        private void CommandExport() {
            _app.Command( "export", command => {
                // sampleapp.exe restore [root] --source <SOURCE>
                // TsMap2.exe export -s -o --verbose

                command.Description = "Export files";
                command.HelpOption( "-?|-h|--help" ); // AppSample.exe restore --help

                // var argSettingPath = command.Argument( "[settingPath]", "Path for setting file" );
                CommandOption optSaveSetting  = command.Option( "-s|--save-setting", $"Generate a setting file at '{AppPath.HomeDirApp}'", CommandOptionType.NoValue );
                CommandOption optOverviewData = command.Option( "-o|--overview-data", "Export the overview data", CommandOptionType.NoValue );
                CommandOption optVerbose      = command.Option( "--verbose", "Increase the verbosity", CommandOptionType.NoValue );

                command.OnExecute( () => {
                    bool exportOverviewData = optOverviewData.HasValue();
                    bool debug              = optVerbose.HasValue();
                    bool saveSettingFile    = optSaveSetting.HasValue();

                    LoggerHelper.Init( debug );

                    Log.Information( "HomeDir: {0}", AppPath.HomeDirApp );

                    try {
                        // -- Settings
                        if ( saveSettingFile ) {
                            var settingFactory = new SettingsJsonFactory( new Settings() );
                            settingFactory.Save();
                            Log.Information( "Your setting file was saved at {0}", settingFactory.GetSavingPath() );
                            return 0;
                        }

                        var s = new SettingsLoadJob();
                        s.RunAndWait();

                        // -- Parse
                        var c = new ParseScsDefJob();
                        c.Run();

                        var m = new ParseMapJob();
                        m.Run();

                        var o = new ParseOverlaysJob();
                        o.Run();

                        // -- Export
                        var e = new ExportJob();
                        e.Run();

                        var t = new ExportTilesJob();
                        t.Run();

                        // -- Debug
                        if ( exportOverviewData ) {
                            var d = new ExportDataOverviewJob();
                            d.RunAndWait();
                        }
                    } catch ( Exception e ) {
                        Log.Error( "Unexpected Exception: {0}", e.GetBaseException().ToString() );
                        Log.Error( "Unexpected Exception: {0}", e.ToString() );
                    }

                    return 0;
                } );
            } );
        }
    }
}