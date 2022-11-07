using System.Collections.Generic;

namespace MoDueler.Animation {

    /// <summary>
    /// Single object that resolves multiple <see cref="AnimationInstance"/> when <see cref="_Process(float)"/> is called.
    /// <para><see cref="AnimationInstance"/>s are automatically removed upon finishing.</para>
    /// <para>Looped <see cref="AnimationInstance"/> need to be removed with <see cref="StopInstance(AnimationInstance)"/>.</para>
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class Animator : Godot.Node {

        /// <summary>
        /// Singleton instance of the <see cref="Animator"/>.
        /// </summary>
        public static Animator Instance { get; private set; }

        /// <summary>
        /// List of <see cref="AnimationInstance"/> currently running.
        /// </summary>
        private readonly List<AnimationInstance> Instances = new List<AnimationInstance>();

        public override void _Ready() {
            // Only allow 1 instance at any time.
            if (Instance != null)
                return;
            Instance = this;
        }

        /// <summary>
        /// Update function that updates all the <see cref="Instances"/>.
        /// </summary>
        /// <param name="deltaTime">The time since <see cref="Instances"/> were last updated.</param>
        public override void _Process(float deltaTime) {
            // Update the animation instance. Removing any that have finished.
            Instances.RemoveAll((instance) => {
                return !instance.Advance(deltaTime);
            });
        }

        /// <summary>
        /// Adds a new <see cref="AnimationInstance"/> so that it can begin playing.
        /// </summary>
        public void StartInstance(AnimationInstance instance) => Instances.Add(instance);

        /// <summary>
        /// Removes and stops a currrent <see cref="AnimationInstance"/>.
        /// </summary>
        public void StopInstance(AnimationInstance instance) {
            // Only stop animations that are currently playing.
            if (!Instances.Contains(instance))
                return;
            instance.CleanUp();
            Instances.Remove(instance);
        }

        /// <summary>
        /// Removes and stops a current <see cref="AnimationInstance"/> without calling <see cref="AnimationInstance.CleanUp"/>.
        /// <para>
        /// Use <see cref="StopInstance(AnimationInstance)"/> if <see cref="AnimationInstance.CleanUp"/> needs to be called.
        /// </para>
        /// </summary>
        /// <param name="instance"></param>
        public void StopInstanceUnclean(AnimationInstance instance) {
            // Only stop animations that are currently playing.
            if (!Instances.Contains(instance))
                return;
            Instances.Remove(instance);
        }

        /// <summary>
        /// Clears all the currently actie animations.
        /// <para>Typically used before a scene transition.</para>
        /// </summary>
        public void Clear() {
            Instances.Clear();
            //TODO: See of instances need to be cleaned up.
        }



    }
}
