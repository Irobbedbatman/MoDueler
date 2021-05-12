using Godot;
using Newtonsoft.Json.Linq;

namespace MoDueler {
    public class GlobalSettings {

        public static JObject SettingsObject;


        /// <summary>
        /// The folder to get resources.
        /// </summary>
        public static string ContentDirectory = "";

        /// <summary>
        /// The inital volume the game will start at.
        /// </summary>
        public static float Volume = .1f;

        /// <summary>
        /// The font to use if we can't find a requested font.
        /// </summary>
        public static string FallbackFontName = "NotoFallbackFont.otf";
        /// <summary>
        /// The font to use if we cant find a requested image.
        /// </summary>
        public static string FallBackImageName = "FileNotFoundFallback.png";


        public static void ApplySettings() {

            // Get the folder the program is running in.
            string folder = OS.GetExecutablePath().GetBaseDir();

            // Get the path to a settings file.
            var settingsFile = System.IO.Path.Combine(folder, "settings.json");

            // If the file exists we can apply the settings.
            if (System.IO.File.Exists(settingsFile)) {

                // Parse the settings file.
                SettingsObject = JObject.Parse(System.IO.File.ReadAllText(settingsFile));

                //Retrive the volume
                if (SettingsObject.TryGetValue("Volume", out var volume)) {
                    Volume = (float)volume.Value<double>();
                }

                // Retrieve the folder that files can be found.
                if (SettingsObject.TryGetValue("ContentDirectory", out var directory)) {
                    string path = directory.Value<string>();
                    if (System.IO.Path.IsPathRooted(path))
                        ContentDirectory = path;
                    else
                        ContentDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(folder, path));

                    ContentDirectory = directory.Value<string>();
                }

                // Apply setting for the player profile.
                PlayerProfile.ApplyProfile(SettingsObject);


            }
            else {

                // Apply Some defaults if the settings file couldn't be found.
                // TODO: Create a settings file if one isn't found.
                ContentDirectory = folder;
            }

            // TODO: Display Settings.

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