using Godot;

namespace MoDueler.Animation.Curves {
    public class Vector2Curve : AnimationCurve<Vector2> {
        public override Vector2 Lerp(Vector2 before, Vector2 after, float weight) => before.LinearInterpolate(after, weight);
    }
}
