using System;

namespace Cireon.Audio
{
    /// <summary>
    /// An object that defines a fade.
    /// </summary>
    public sealed class FadeDefinition
    {
        /// <summary>
        /// The types of fade.
        /// </summary>
        public enum FadeType
        {
            /// <summary>
            /// A linear fade.
            /// </summary>
            Linear,
            /// <summary>
            /// A fade that starts fast and slows down near the end.
            /// </summary>
            Logarithmic,
            /// <summary>
            /// A fade that starts slow and speeds up near the end.
            /// </summary>
            Exponential,
            /// <summary>
            /// A fade that starts slow, speeds up and slows down near the end again.
            /// </summary>
            Smooth
        }

        private float currentFadeDuration;
        private readonly float fadeDuration, initialVolume, goalVolume;
        private readonly FadeType type;

        /// <summary>
        /// The current volume of this fade.
        /// </summary>
        public float CurrentVolume { get; private set; }
        /// <summary>
        /// True if the fade is finished.
        /// </summary>
        public bool Finished { get { return this.currentFadeDuration >= this.fadeDuration; } }

        /// <summary>
        /// Creates a new fade definition.
        /// </summary>
        /// <param name="fadeDuration">The duration of the fade.</param>
        /// <param name="initialVolume">The volume at the start of the fade.</param>
        /// <param name="goalVolume">The volume at the end of the fade.</param>
        /// <param name="type">The interpolation method for the fade.</param>
        public FadeDefinition(float fadeDuration, float initialVolume, float goalVolume, FadeType type = FadeType.Linear)
        {
            this.fadeDuration = fadeDuration;
            this.initialVolume = initialVolume;
            this.goalVolume = goalVolume;
            this.type = type;
        }

        /// <summary>
        /// Updates the fade.
        /// </summary>
        /// <param name="elapsedTimeS">The amount of elapsed time since last update (in seconds).</param>
        public void Update(float elapsedTimeS)
        {
            this.currentFadeDuration += elapsedTimeS;

            if (this.Finished)
            {
                this.CurrentVolume = this.goalVolume;
                return;
            }

            float t = 0;

            switch (this.type)
            {
                case FadeType.Linear:
                    t = this.progressLinear();
                    break;
                case FadeType.Logarithmic:
                    t = this.progressLogarithmic();
                    break;
                case FadeType.Exponential:
                    t = this.progressExponential();
                    break;
                case FadeType.Smooth:
                    t = this.progressSmooth();
                    break;
            }

            this.CurrentVolume = this.initialVolume + t * (this.goalVolume - this.initialVolume);
        }

        private float progressLinear()
        {
            return this.currentFadeDuration / this.fadeDuration;
        }

        private float progressLogarithmic()
        {
            throw new NotImplementedException();
        }

        private float progressExponential()
        {
            throw new NotImplementedException();
        }

        private float progressSmooth()
        {
            return 0.5f - 0.5f * (float) Math.Cos(Math.PI * this.progressLinear());
        }
    }
}