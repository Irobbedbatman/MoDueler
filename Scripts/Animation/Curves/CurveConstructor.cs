using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Animation.Curves {
    public class CurveConstructor {


        public static T Construct<T, V>(JToken token, Dictionary<string, object> variables) where T : AnimationCurve<V>, new() {
            var result = new T();

            foreach (JToken nodeToken in token) {

                var node = new CurveNode<V>();

                float time = 0;
                object value = default;

                CurveType curveType = default;

                foreach (JProperty factor in nodeToken) {
                    var factorName = factor.Name;
                    var factorValue = factor.Value;

                    var variable = variables.TryGetValue(factorName, out var variableValue);

                    result.ParseAnimationToken(factorName, factorValue, variableValue, ref time, ref value, ref curveType);

                   

                }


                node.CurveType = curveType;
                node.Value = (V)value;

                result.AddNode(time, node);

            }


            return new T();
        }

        public void Test() {

            Construct<Vector2Curve, Vector2>(null, new Dictionary<string, object>());

        }

    }
}
