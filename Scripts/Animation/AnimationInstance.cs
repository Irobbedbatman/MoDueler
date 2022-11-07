using Godot;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using MoDueler.Animation.Transformations;

namespace MoDueler.Animation {
    /// <summary>
    /// An animation that is playing currently.
    /// <para>Uses a <see cref="AnimationBasis"/> as a template and all parameters are supplied in the constructor.</para>
    /// <para>One of the constructors builds the argument table and one needs it supplied. The latter is for lua files that find explicit naming to be benefical.</para>
    /// </summary>
    public class AnimationInstance {

        /// <summary>
        /// The animation data this <see cref="AnimationInstance"/> uses to control the animations.
        /// </summary>
        public readonly AnimationBasis Basis;
        /// <summary>
        /// The node that the animation will be played on.
        /// </summary>
        public readonly Node RootNode;

        /// <summary>
        /// All the <see cref="Transformations"/> on bones that need to be <see cref="Advance(float)(float)"/>d each frame.
        /// </summary>
        private Action<float, float> AnimationComponents;

        /// <summary>
        /// The method that is called when the <see cref="AnimationInstance"/> has reached the end of the animation and isn't looping.
        /// <para>Calling <see cref="CleanUp()"/> will allow it to be called in the case where an animation might be ended purposfully.</para>
        /// </summary>
        private readonly Closure CleanUpFunction;
        /// <summary>
        /// How long the animation has been going for.
        /// <para>For looping animations timeElapsed will modulate using <see cref="AnimationBasis.Duration"/>.</para>
        /// </summary>
        private float timeElapsed = 0;

        /// <summary>
        /// Contructor for a new animation. This one constructs the keyed arguments using <paramref name="basis"/> and <paramref name="arguments"/>.
        /// </summary>
        /// <param name="basis">The template animation data from a file.</param>
        /// <param name="rootNode">The element that the animation nodes are searched from.</param>
        /// <param name="arguments">Variables passed to the animation in the order <see cref="AnimationBasis.ArgumentNames"/>.</param>
        /// <param name="finishedFunction">The function that is executed upon the animation finishing.</param>
        public AnimationInstance(AnimationBasis basis, Node rootNode, object[] arguments, Closure finishedFunction = null) {
            RootNode = rootNode;
            CleanUpFunction = finishedFunction;
            Basis = basis;

            // If there are no animations for whatever reason they can't be created.
            if (basis.Animations == null)
                return;

            // Collection mapping the animation argument names to the argument values provided.
            Dictionary<string, object> keyedArguments = new Dictionary<string, object>();
            for (int i = 0; i < basis.ArgumentNames.Length; ++i) {
                // Not all arguments need to be specified similar to the way lua works.
                if (i < arguments.Length)
                    keyedArguments.Add(basis.ArgumentNames[i], arguments[i]);
            }

            // Create the key animation.
            AddBonesAndTransforms(basis, keyedArguments);
        }

        /// <summary>
        /// Contructor for a new animation. This one needs the already created and managed data.
        /// <para>This one is more applicable if you want to use varaible names in lua.</para>
        /// </summary>
        /// <param name="basis">The template animation data from a file.</param>
        /// <param name="rootNode">The element that the animation nodes are searched from.</param>
        /// <param name="keyedArguments">The arguments already stored as a variable key to varaible value dictionary.</param>
        /// <param name="finishedFunction">The function that is executed upon the animation finishing.</param>
        public AnimationInstance(AnimationBasis basis, Node rootNode, Dictionary<string, object> keyedArguments, Closure finishedFunction = null) {
            RootNode = rootNode;
            CleanUpFunction = finishedFunction;
            Basis = basis;

            // If there are no animations for whatever reason they can't be created.
            if (basis.Animations == null)
                return;

            // Create the key animation.
            AddBonesAndTransforms(basis, keyedArguments);
        }

        /// <summary>
        /// Reads the <see cref="AnimationBasis.Animations"/> and creates releavent transformations on each requested bone.
        /// </summary>
        /// <param name="basis">The template animation data from a file.</param>
        /// <param name="keyedArguments">The arguments stored as a variable key to varaible value dictionary.</param>
        private void AddBonesAndTransforms(AnimationBasis basis, Dictionary<string, object> keyedArguments) {
            foreach (var boneName in basis.Bones) {
                Node bone = null;
                // If the bone is specified as a variable use that.
                if (keyedArguments.TryGetValue(boneName, out var tryNode)) {
                    // If the argument is not a node it can't be used as a bone.
                    if (tryNode is Node node)
                        bone = node;
                }
                else {
                    // Get the bone node affected by the animation. If root names are provided the root bone is used.
                    bone = boneName == "" || boneName == "root" ? RootNode : RootNode.GetNode(boneName);
                }
                // Get the animation for this bone from the animation data.
                var anims = basis.Animations[boneName];
                // Add each animation transformation.
                foreach (JProperty anim in anims) {
                    AddTransformation(anim.Name, anim.Value, bone, keyedArguments);
                }
            }
        }

        /// <summary>
        /// Advances the animation by a provided time in seconds.
        /// </summary>
        /// <param name="time">The time in seconds to move forward.</param>
        /// <returns>True if the animation has finished.</returns>
        public bool Advance(float time) {
            // Record the value of time last update.
            float previousTIme = timeElapsed;
            // Get the new time into the animation.
            timeElapsed += time;
            // Loop the animation by resetting the animation's time.
            if (Basis.Loop)
                timeElapsed %= (float)Basis.Duration;

            // Perfrom animation tranformations provided to the animation instance.
            AnimationComponents?.Invoke(previousTIme, timeElapsed);

            // If the animation is still ongoing return false to indicate as such.
            if (timeElapsed < Basis.Duration)
                return false;
            // If the animation is over cleanup and end the animation.
            CleanUp();
            return true;
        }

        /// <summary>
        /// Performs cleanup functionally provided to the <see cref="AnimationInstance"/> in the constructor.
        /// <para>As this uses <see cref="MoonSharp.Interpreter.Script"/> it is and cannot be thread safe.</para>
        /// </summary>
        public void CleanUp() => CleanUpFunction?.Call();

        /// <summary>
        /// Adds a specified animation component to <see cref="AnimationComponents"/> with <paramref name="transformationName"/>.
        /// </summary>
        /// <param name="transformationName">The type of transformation mentioned by name in the animation file. Used in a switch case.</param>
        /// <param name="curveData">The json token specific to this transformation.</param>
        /// <param name="bone">The <see cref="Node"/> that the animation will affect.</param>
        /// <param name="keyedArguments">The arguments stored as a variable key to varaible value dictionary.</param>
        private void AddTransformation(string transformationName, JToken curveData, Node bone, Dictionary<string, object> keyedArguments) {

            switch (transformationName) {
                case "translate":
                    AnimationComponents += Translating.AddTranslation(bone, curveData, keyedArguments);
                    break;
            }

        }

    }
}
