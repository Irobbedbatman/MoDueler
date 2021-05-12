using Godot;
using MoDueler.Animation.Curves;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MoDueler.Animation.Transformations {

    /// <summary>
    /// Provides translation upon <see cref="Node2D"/> and <see cref="Control"/>.
    /// </summary>
    public static class Translating {

        /// <summary>
        /// Get's an action that allows translation overtime on the provided <paramref name="node"/>.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> that will be animated.</param>
        /// <param name="curveData">The json curve data.</param>
        /// <param name="keyedArguments"></param>
        /// <returns>A <see cref="Delegate"/> that performs the animations on the node.</returns>
        public static Action<float, float> AddTranslation(Node node, JToken curveData, Dictionary<string, object> keyedArguments) {
            var curve = ConstructCurve(curveData, keyedArguments);
            return  (previousTime, currentTime) => {
                // TODO: Create a seperate Apply for Node2D and Control so that the current Apply doesn't need to perform a cast everytime.
                Apply(node, curve, previousTime, currentTime);
            };
        }

        /// <summary>
        /// Applys a translation to a <paramref name="node"/> based on a <paramref name="curve"/>.
        /// <para>Translation is the diffrence between the curve at the two provided time intervals.</para>
        /// <para>Works with <see cref="Node2D"/> and <see cref="Control"/> nodes only.</para>
        /// <para>Previous time should be 0 for initial application.</para>
        /// </summary>
        /// <param name="node">The <see cref="Node"/> that will be translated.</param>
        /// <param name="curve">The <see cref="AnimationCurve{T}"/> that provideds the translation.</param>
        /// <param name="previousTime">The time the translation was last applied. <paramref name="currentTime"/> when it was last called.</param>
        /// <param name="currentTime">The current time upon the <paramref name="curve"/>.</param>
        public static void Apply(Node node, Vector2Curve curve, float previousTime, float currentTime) {
            Vector2 deltaPos = curve[currentTime] - curve[previousTime];
            // If the previous time is at the start of the curve we mode the whole distance.
            if (previousTime == 0)
                deltaPos = curve[currentTime];
            // Translating 2d nodes.
            if (node is Node2D node2d)
                node2d.Position += deltaPos;
            // Translating control nodes.
            else if (node is Control control)
                control.RectPosition += deltaPos;
            else
                GD.PrintErr("Translation Attempted on node <" + node.Name + "> translating that node type is not available.");
        }

        public static Vector2Curve ConstructCurve(JToken token, Dictionary<string, object> variables) {
            Vector2Curve curve = new Vector2Curve();

            foreach (JToken nodeToken in token) {
                CurveNode<Vector2> node = new CurveNode<Vector2>();
                float time = 0;
                Vector2 pos = Vector2.Zero;

                foreach (JProperty factor in nodeToken) {
                    var value = factor.Value;
                    switch (factor.Name.ToLower()) {
                        case "time":
                            if (variables.TryGetValue((string)value, out object vartime))
                                time = (float)vartime;
                            else
                                time = (float)value;
                            break;
                        case "x":
                            if (variables.TryGetValue((string)value, out object varX))
                                pos.x = (float)varX;
                            else
                                pos.x = (float)value;
                            break;
                        case "y":
                            if (variables.TryGetValue((string)value, out object varY))
                                pos.y = (float)varY;
                            else
                                pos.y = (float)value;
                            break;
                        case "curve":
                            if (variables.TryGetValue((string)value, out object curveType))
                                node.CurveType = (CurveType)curveType;
                            else
                                node.CurveType = Vector2Curve.ParseCurveType((string)value);
                            break;
                        case "pos":
                        case "position":
                            if (variables.TryGetValue((string)value, out object varpos))
                                pos = (Vector2)varpos;
                            break;
                        default:
                            break;
                    }
                }
                node.Value = pos;
                curve.AddNode(time, node);
            }
            return curve;
        }

    }
}
