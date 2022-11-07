using MoDueler.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoDueler.Animation.Curves {

    /// <summary>
    /// If the code uses <see cref="AnimationCurve{T}.Lerp(T, T, float)"/> or simple returns the lower value.
    /// </summary>
    public enum CurveType {
        /// <summary>
        /// Curve uses linear interpolation.
        /// </summary>
        Linear,
        /// <summary>
        /// Curve will only use values below the time specified.
        /// </summary>
        Stepped
    };

    public abstract class AnimationCurve<T> {

        /// <summary>
        /// Parses the provided string to a <see cref="CurveType"/>.
        /// <para>Currently this is either explicily `steppped` or always treaded to be linear.</para>
        /// </summary>
        public static CurveType ParseCurveType(string value) => value.ToLower().Equals("stepped") ? CurveType.Stepped : CurveType.Linear;

        /// <summary>
        /// The <see cref="CurveNode{T}"/>s that make up the curve. Sorted by time.
        /// </summary>
        private readonly SortedList<float, CurveNode<T>> Curve = new SortedList<float, CurveNode<T>>();

        /// <summary>
        /// Adds a node to the <see cref="Curve"/>.
        /// </summary>
        /// <param name="time">The time the key curve inserts the new node.</param>
        /// <param name="value">The value of the curve at <paramref name="time"/>.</param>
        /// <param name="type">Whether this part of the curve is stepped or not.</param>
        /// <returns>The <see cref="AnimationCurve{T}"/> itself so multiple nodes can be added in succession.</returns>
        public AnimationCurve<T> AddNode(float time, T value, CurveType type) {
            Curve.Add(time, new CurveNode<T>() { Value = value, CurveType = type });
            return this;
        }

        /// <summary>
        /// Adds a node to the <see cref="Curve"/>.
        /// </summary>
        /// <param name="time">The time the key curve inserts the new node.</param>
        /// <param name="node">The node to add.</param>
        /// <returns>The <see cref="AnimationCurve{T}"/> itself so multiple nodes can be added in succession.</returns>
        public AnimationCurve<T> AddNode(float time, CurveNode<T> node) {
            Curve.Add(time, node);
            return this;
        }

        /// <summary>
        /// Adds a range of nodes to the <see cref="Curve"/>.
        /// </summary>
        /// <param name="nodes">The collection of nodes to add. In the same format as <see cref="Curve"/>.</param>
        /// <returns>he <see cref="AnimationCurve{T}"/> itself so multiple nodes can be added in succession.</returns>
        public AnimationCurve<T> AddNodeRange(IEnumerable<KeyValuePair<float, CurveNode<T>>> nodes) {
            foreach (var node in nodes)
                Curve.Add(node.Key, node.Value);
            return this;
        }

        /// <summary>
        /// Retrieves all the nodes between the two times specified.
        /// <paramref name="below"/> must be smaller than <paramref name="above"/>. 
        /// </summary>
        /// <param name="below">The time to check from.</param>
        /// <param name="above">The time to check to.</param>
        /// <returns>The set of nodes on the <see cref="Curve"/> between the two times specified.</returns>
        public T[] GetNodesBetween(float below, float above) {
            // Above must be greater than below.
            if (above < below)
                return default;

            // Get the index for both times.
            int belowIndex = BinarySearch.Search(Curve.Keys, below);
            int aboveIndex = BinarySearch.Search(Curve.Keys, above);

            // Ensure both indexs are valid.
            if (belowIndex > Curve.Keys.Count || belowIndex > aboveIndex || aboveIndex < 0)
                return default;

            // Retrieve all the nodes and store them in an array.
            T[] inBetweenNodes = new T[aboveIndex - belowIndex + 1];
            for (int i = belowIndex; i < aboveIndex; ++i) {
                inBetweenNodes[i - belowIndex] = Curve.Values[i].Value;
            }
            return inBetweenNodes;
        }

        /// <summary>
        /// The lerp function to get intermediate values.
        /// </summary>
        /// <param name="before">The value when <paramref name="weight"/> = 0.</param>
        /// <param name="after">The value when <paramref name="weight"/> = 1.</param>
        /// <param name="weight">The rate between <paramref name="before"/> and <paramref name="after"/>.</param>
        /// <returns>The linear interpolated value.</returns>
        public abstract T Lerp(T before, T after, float weight);

        /// <summary>
        /// Get's the value of the <see cref="Curve"/> at the provided time.
        /// </summary>
        /// <param name="time">The time in the <see cref="Curve"/>.</param>
        /// <returns>The value of the <see cref="Curve"/> at the provided <paramref name="time"/>.</returns>
        public T GetValue(float time) {
            // Get the index of time above the provided time.
            int aboveIndex = BinarySearch.Search(Curve.Keys, time);
            // If above index is higher or equal to the end of the curve return the highest value in the curve.
            if (aboveIndex >= Curve.Count)
                return Curve.Last().Value.Value;

            // If the found index is the first index retrun the start of the curve.
            if (aboveIndex <= 0)
                return Curve[aboveIndex].Value;

            // The below index will be 1 before the aboveIndex.
            int belowIndex = aboveIndex - 1;

            // If the curve is stepped return the belowIndex of the curve instead.
            if (Curve.Values[aboveIndex].CurveType == CurveType.Stepped)
                return Curve[belowIndex].Value;

            // Calculate the diffrence between the two curve nodes time.
            float lerpTime = (Curve.Keys[belowIndex] - time) / (Curve.Keys[belowIndex] - Curve.Keys[aboveIndex]);

            // Lerp the values of the curve using the lerp time calculated.
            return Lerp(Curve[belowIndex].Value, Curve[aboveIndex].Value, lerpTime);
        }

        /// <summary>
        /// Easier accessor for <see cref="GetValue(float)"/>.
        /// </summary>
        public T this[float time] => GetValue(time);

        /// <summary>
        /// Get's the key with the highest value from the <see cref="Curve"/>.
        /// </summary>
        public float HighestKey => Curve.Last().Key;


        /// <summary>
        /// Parses an animation token passed to it via the <see cref="CurveConstructor"/>
        /// <para>Children of the this class should default to using the base to automatically apply time and curve type.</para>
        /// </summary>
        /// <param name="factorName">The json key.</param>
        /// <param name="factorValue">The json value.</param>
        /// <param name="variableValue">The value that was retrieved from the variables dictionary.</param>
        /// <param name="time">The time the node is added at.</param>
        /// <param name="value">THe value the animation will have at the provided time</param>
        /// <param name="curveType">The <see cref="CurveType"/> used by the curve for this step.</param>
        public void ParseAnimationToken(string factorName, JToken factorValue, object variableValue, ref float time, ref object value, ref CurveType curveType) {
            switch (factorName) {
                // Get the time parameter.
                case "time":
                    time = (float)(variableValue ?? factorValue);
                    break;
                // Get the curvetype paramater. When provided via a variable a number should be provided.
                case "curve":
                    if (variableValue != null)
                        curveType = (CurveType)variableValue;
                    else
                        curveType = ParseCurveType((string)factorValue);
                    break;
            }
        }

    }
}
