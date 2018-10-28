using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenPlayer
{
    class Player
    {
        /// <summary>
        /// A collection of all the settings required for playback.
        /// Yes, I know public member data is generally considered bad practice.
        /// Since this object has no such concept as invalid state, and merely
        /// serves as a collection of variables, I think it's OK to break dogma.
        /// </summary>
        public struct Settings
        {
            public string ConfigName;
            public string DitResourceName;
            public string DahResourceName;
            public string AmbientResourceName;
            public int    DitDuration;
            public int    DahDuration;
            public int    AmbientDuration;
        }

        private Settings activeSettings;
        private int nextTextIndex = 0;

        /// <summary>
        /// A list of the options that are valid to choose.
        /// Ideally these would be read from a config file or something.
        /// </summary>
        public static readonly Settings[] AvailableSettings =
        {
            new Settings {
                ConfigName          = "Bell and wood",
                DitResourceName     = "ZenPlayer.ZenAudio.dit0_67ms.wav",
                DahResourceName     = "ZenPlayer.ZenAudio.dah_202ms.wav",
                AmbientResourceName = "",
                DitDuration = 67,
                DahDuration = 202,
                AmbientDuration = 0,
            }
        };
  

        private System.Media.SoundPlayer ditPlayer = null;
        private System.Media.SoundPlayer dahPlayer = null;
        private System.Media.SoundPlayer ambientPlayer = null;

        /// <summary>
        /// Prepares to play the given settings by loading them into memory.
        /// </summary>
        /// <param name="settings"></param>
        public async void LoadFiles(Settings settings)
        {
            activeSettings = settings;
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream ditStream = a.GetManifestResourceStream("ZenPlayer.ZenAudio.dit0_67ms.wav");
            System.IO.Stream dahStream = a.GetManifestResourceStream("ZenPlayer.ZenAudio.dah_202ms.wav");
            // TODO: do the same for ambient sounds
            ditPlayer = new System.Media.SoundPlayer(ditStream);
            dahPlayer = new System.Media.SoundPlayer(dahStream);
            await Task.Run(() => { ditPlayer.Load(); });
            await Task.Run(() => { dahPlayer.Load(); });

        }

        public string Text { get; set; }

        public void Play()
        {
            ambientPlayer?.Play();
            Task task = Task.Run((Action)PlayMorseCode);
        }

        public void Stop()
        {
            Pause();
            // Reset state
            nextTextIndex = 0;
        }



        public void Pause()
        {
            ditPlayer?.Stop();
            dahPlayer?.Stop();
            ambientPlayer?.Stop();
        }

        /// <summary>
        /// This is the core player function.
        /// It will run until cancelled, or it reaches the end of the text.
        /// </summary>
        private async void PlayMorseCode()
        {


            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);

            //await Task.Delay(67 * 3);
            await Task.Delay(1000);

            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(67);

            //await Task.Delay(67 * 3);
            await Task.Delay(1000);

            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(67);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(67);

        }
    }
}
