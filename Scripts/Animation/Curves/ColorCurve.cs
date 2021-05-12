using Godot;

namespace MoDueler.Animation.Curves {
    public class ColorCurve : AnimationCurve<Color> {
        public override Color Lerp(Color before, Color after, float weight) => before.LinearInterpolate(after, weight);
    }
}
