using System.Collections.Generic;

namespace MoDueler.Animation {

    /// <summary>
    /// Single object that resolves multiple <see cref="AnimationInstance"/> when <see cref="Update(float)"/> is called.
    /// <para><see cref="AnimationInstance"/>s are automatically removed upon finishing.</para>
    /// <para>Looped <see cref="AnimationInstance"/> need to be removed with <see cref="StopInstance(AnimationInstance)"/>.</para>
    /// </summary>
    public class Animator {

        /// <summary>
        /// List of <see cref="AnimationInstance"/> currently running.
        /// </summary>
        private readonly List<AnimationInstance> Instances = new List<AnimationInstance>();

        /// <summary>
        /// Update function that updates all the <see cref="Instances"/>.
        /// </summary>
        /// <param name="deltaTime">The time since <see cref="Instances"/> were last updated.</param>
        public void Update(float deltaTime) {
            // Update the animation instance. Removing any that have finished.
            Instances.RemoveAll((instance) => {
                return !instance.Update(deltaTime);
            });
        }

        /// <summary>
        /// Adds a new <see cref="AnimationInstance"/> so that it can begin playing.
        /// </summary>
        public void StartInstance(AnimationInstance instance) => Instances.Add(instance);

        /// <summary>
        /// Removes ands stops a currrent <see cref="AnimationInstance"/>.
        /// </summary>
        public void StopInstance(AnimationInstance instance) {
            // Only dtop animations that currently playing.
            if (!Instances.Contains(instance))
                return;
            instance.CleanUp();
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
