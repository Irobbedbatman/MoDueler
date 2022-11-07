using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Resources {
    public static class ColorCreation {

        /// <summary>
        /// Creates a color, useful only for lua but they could call Color._new() anyway.
        /// </summary>
        public static Color CreateColor(float r, float g, float b, float a = 1) => new Color(r, g, b, a);
    }
}
