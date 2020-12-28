using Godot;
using System.Collections.Generic;


namespace MoDueler.Camera {
    /// <summary>
    /// Sorts given <see cref="Node2D"/> by their <see cref="Node2D.ZIndex"/>
    /// </summary>
    public class ZDepthSort : IComparer<Node2D> {

        public int Compare(Node2D x, Node2D y) {

            //TODO: ZIndexAsRelative checking.

            int compare = y.ZIndex.CompareTo(x.ZIndex);
            if (compare == 0)
                return y.GetIndex().CompareTo(x.GetIndex());
            return compare;
        }

    }

}
