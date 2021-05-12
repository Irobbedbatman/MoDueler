using System;

namespace MoDueler.Animation.Curves {

    /// <summary>
    /// A node on <see cref="AnimationCurve"/>. This the value of the node rather than the time it occurs.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of value the curve uses.</typeparam>
    public struct CurveNode<T> {

        public T Value { get; set; }
        public CurveType CurveType { get; set; }

    }
}
