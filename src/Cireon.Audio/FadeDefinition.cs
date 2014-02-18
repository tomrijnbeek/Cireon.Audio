using System;

namespace Cireon.Audio
{
    public sealed class FadeDefinition
    {
        public enum FadeType
        {
            Linear,
            Logarithmic,
            Exponential,
            Smooth
        }

        private float currentFadeDuration;
        private readonly float fadeDuration, initialVolume, goalVolume;
        private readonly FadeType type;

        public float CurrentVolume { get; private set; }
        public bool Finished { get { return this.currentFadeDuration >= this.fadeDuration; } }

        public FadeDefinition(float fadeDuration, float initialVolume, float goalVolume, FadeType type = FadeType.Linear)
        {
            this.fadeDuration = fadeDuration;
            this.initialVolume = initialVolume;
            this.goalVolume = goalVolume;
            this.type = type;
        }

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