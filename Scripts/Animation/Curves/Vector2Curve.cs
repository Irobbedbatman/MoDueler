using Godot;
using Newtonsoft.Json.Linq;

namespace MoDueler.Animation.Curves {
    public class Vector2Curve : AnimationCurve<Vector2> {
        public override Vector2 Lerp(Vector2 before, Vector2 after, float weight) => before.LinearInterpolate(after, weight);

        public static Vector2Curve Construct() {
            Vector2Curve result = new Vector2Curve();
            

            return result;
        }


        public new void ParseAnimationToken(string factorName, JToken factorValue, object variableValue, ref float time, ref object value, ref CurveType curveType) {

            Vector2 val = default;

            switch (factorName) {

                case "x":
                    val.x = 10;
                    break;

                default:
                    base.ParseAnimationToken(factorName, factorValue, variableValue, ref time, ref value, ref curveType);
                    break;
            }

            value = val;


        }

    }
}
