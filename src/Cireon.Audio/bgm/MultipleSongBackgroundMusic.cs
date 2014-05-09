using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cireon.Audio
{
    /// <summary>
    /// A background music controller that plays songs randomly from a pool of songs.
    /// </summary>
    sealed public class MultipleSongBackgroundMusic : IBackgroundMusic
    {
        private readonly static Random random = new Random();

        private readonly IList<Song> songs;

        private Song currentSong;

        private float globalVolume, localVolume = 1;
        private float pitch;

        private FadeDefinition currentFade;
        private Action fadeCallback;

        /// <summary>
        /// Creates a new background music controller playing multiple songs.
        /// </summary>
        /// <param name="songs">An enumerable containing the songs to play from.</param>
        public MultipleSongBackgroundMusic(IEnumerable<Song> songs)
        {
            this.songs = (songs as IList<Song>) ?? songs.ToList();
            if (this.songs.Count <= 0)
                throw new ArgumentException("No songs were specified.", "songs");
        }

        /// <summary>
        /// Creates a new background music controller playing multiple songs.
        /// </summary>
        /// <param name="songs">A list containing the songs to play from.</param>
        public MultipleSongBackgroundMusic(params Song[] songs)
            : this((IEnumerable<Song>)songs) { }

        /// <summary>
        /// Creates a new background music controller playing multiple songs.
        /// </summary>
        /// <param name="files">A list containing the files of the songs to play from.</param>
        public MultipleSongBackgroundMusic(params string[] files)
            : this(files.Select(file => new Song(file))) { }

        /// <summary>
        /// Updates the background music controller.
        /// </summary>
        /// <param name="elapsedTimeS">The elapsed time in seconds since the last update.</param>
        public void Update(float elapsedTimeS)
        {
            if (this.currentSong.FinishedPlaying)
                this.propagateSong();

            if (this.currentFade != null)
            {
                this.currentFade.Update(elapsedTimeS);
                this.localVolume = this.currentFade.CurrentVolume;
                this.OnVolumeChanged(this.globalVolume);

                if (this.currentFade.Finished)
                {
                    this.currentFade = null;

                    if (this.fadeCallback != null)
                    {
                        this.fadeCallback();
                        this.fadeCallback = null;
                    }
                }
            }
        }

        /// <summary>
        /// Starts playing this background music controller.
        /// </summary>
        public void Start()
        {
            this.propagateSong();
        }

        /// <summary>
        /// Stops playing this background music controller.
        /// </summary>
        public void Stop()
        {
            this.currentSong.Stop();
            this.currentSong = null;
            this.localVolume = 1;
        }

        /// <summary>
        /// Fades out this background music controller.
        /// </summary>
        /// <param name="length">The length of this fade.</param>
        /// <param name="callback">The method that should be called when the fade is finished.</param>
        public void FadeOut(float length, Action callback)
        {
            this.currentFade = new FadeDefinition(length, this.localVolume, 0);
            this.fadeCallback = callback;
        }

        /// <summary>
        /// Handles a change in music volume.
        /// </summary>
        /// <param name="volume">The new music volume.</param>
        public void OnVolumeChanged(float volume)
        {
            this.globalVolume = volume;
            if (this.currentSong != null)
                this.currentSong.Volume = this.globalVolume;
        }

        /// <summary>
        /// Handles a change in pitch.
        /// </summary>
        /// <param name="pitch">The new pitch.</param>
        public void OnPitchChanged(float pitch)
        {
            this.pitch = pitch;
            if (this.currentSong != null)
                this.currentSong.Pitch = this.pitch;
        }

        private void propagateSong()
        {
            if (this.currentSong != null)
                this.currentSong.Stop();
            this.currentSong = this.selectRandomSong();

            this.currentSong.Volume = this.globalVolume * this.localVolume;
            this.currentSong.Pitch = this.pitch;
            this.currentSong.Looping = false;

            this.currentSong.Play();
        }

        private Song selectRandomSong()
        {
            if (this.songs.Count == 1)
                return this.songs[0];

            Song s;
            do
            {
                s = this.songs[MultipleSongBackgroundMusic.random.Next(this.songs.Count)];
            } while (s == this.currentSong);

            return s;
        }

        /// <summary>
        /// Generates a MultipleSongBackgroundMusic controller from a folder of files.
        /// </summary>
        /// <param name="folder">The folder to extract the files from.</param>
        /// <returns>A background music controller playing the songs from the specified folder.</returns>
        public static MultipleSongBackgroundMusic FromFolder(string folder)
        {
            return new MultipleSongBackgroundMusic(Directory.GetFiles(folder, "*.ogg"));
        }
    }
}
