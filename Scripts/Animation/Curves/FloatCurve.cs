using Godot;

namespace MoDueler.Animation.Curves {
    public class FloatCurve : AnimationCurve<float> {
        public override float Lerp(float before, float after, float weight) => Mathf.Lerp(before, after, weight);
    }
}
