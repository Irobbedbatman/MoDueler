using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Resources {

    /// <summary>
    /// A .json file loaded for use in lua.
    /// </summary>
    [MoonSharpUserData]
    public class JsonResource {

        /// <summary>
        /// The the contents of the .json file.
        /// </summary>
        public readonly JObject Data;

        public JsonResource(string key) {
            // Try and file the file.
            var file = ResourceFiles.FindFile(key);
            if (file == null)
                return;

            // If we find the file we can read it all.
            string fileContents = File.ReadAllText(file);

            try {
                Data = JObject.Parse(fileContents);
            }
            catch { }
        }

        /// <summary>
        /// Get a sub element of the <see cref="Data"/> with the provided key.
        /// </summary>
        public JToken this[string key] {
            get => Data[key];
        }

    }
}
