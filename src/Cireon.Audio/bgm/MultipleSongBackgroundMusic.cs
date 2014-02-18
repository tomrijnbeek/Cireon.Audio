using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cireon.Audio
{
    sealed public class MultipleSongBackgroundMusic : IBackgroundMusic
    {
        private readonly static Random random = new Random();

        private readonly IList<Song> songs;

        private Song currentSong;

        private float volume;
        private float pitch;

        public MultipleSongBackgroundMusic(IEnumerable<Song> songs)
        {
            this.songs = (songs as IList<Song>) ?? songs.ToList();
            if (this.songs.Count <= 0)
                throw new ArgumentException("No songs were specified.", "songs");
        }

        public MultipleSongBackgroundMusic(params Song[] songs)
            : this((IEnumerable<Song>)songs) { }

        public MultipleSongBackgroundMusic(params string[] files)
            : this(files.Select(file => new Song(file))) { }

        public void Update(float elapsedTimeS)
        {
            if (this.currentSong.FinishedPlaying)
                this.propagateSong();
        }

        public void Start()
        {
            this.currentSong = this.selectRandomSong();
            this.currentSong.Looping = false;
            this.currentSong.Play();
        }

        public void Stop()
        {
            this.currentSong.Stop();
            this.currentSong = null;
        }

        public void OnVolumeChanged(float volume)
        {
            this.volume = volume;
            this.currentSong.Volume = this.volume;
        }

        public void OnPitchChanged(float pitch)
        {
            this.pitch = pitch;
            this.currentSong.Pitch = this.pitch;
        }

        private void propagateSong()
        {
            this.currentSong.Stop();
            this.currentSong = this.selectRandomSong();
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

        public static MultipleSongBackgroundMusic FromFolder(string folder)
        {
            return new MultipleSongBackgroundMusic(Directory.GetFiles(folder, "*.ogg"));
        }
    }
}
