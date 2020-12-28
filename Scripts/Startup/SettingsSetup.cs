
using Godot;
using IniParser;
using IniParser.Model;

namespace MoDueler.Startup {

    /// <summary>
    /// Changes the way the application is run at startup from a .ini file or from some deafult parameters.
    /// </summary>
    public static class SettingsSetup {

        public static void ApplySettings() {

            // Get the folder the program is running in.
            string folder = OS.GetExecutablePath().GetBaseDir();

            // Get the path to a settings file.
            var settings = System.IO.Path.Combine(folder, "settings.ini");

            // If the file exists we can apply the settings.
            if (System.IO.File.Exists(settings)) {
                // Parse the settings file.
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(settings);
                // Get the setting for location resource files.
                var contentPath = data["Files"]["ContentFolder"];
                // If it's a full path we can immeadilty use it.
                if (System.IO.Path.IsPathRooted(contentPath))
                    GlobalSettings.ResourceDirectory = contentPath;
                else // Otherwise we can combine it with the executable folder.
                    GlobalSettings.ResourceDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(folder, contentPath));

                // Get the setting for volume from the settings file.
                var volume = data["Audio"]["Volume"];
                if (float.TryParse(volume, out float _vol)) {
                    GlobalSettings.Volume = _vol;
                }

            }
            else {
                // Apply Some defaults if the settings file couldn't be found.
                // TODO: Create an ini if one isn't found.
                GlobalSettings.ResourceDirectory = folder;
            }

            //GetViewport().

            //OS.WindowSize = new Vector2(1920, 1080);
            // GetTree().SetScreenStretch(SceneTree.StretchMode.Mode2d, SceneTree.StretchAspect.Expand, new Vector2(1920, 1080));
            //OS.WindowSize = new Vector2(1024, 600);
            //OS.WindowPosition = new Vector2(0, 0);
            //OS.CenterWindow();
            //ProjectSettings.SetSetting("display/window/size/width", 1920);
            //ProjectSettings.SetSetting("display/window/size/height", 1080);
            //ProjectSettings.SetSetting("display/window/size/borderless", false);
            //ProjectSettings.Save();
            //VisualServer.SetDefaultClearColor(new Color(0, 0, 0));
            //OS.CurrentScreen = Mathf.Min(OS.GetScreenCount(), 0);
        }
    }
}
