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
            public int DitDuration;
            public int DahDuration;
            public int AmbientDuration;
            public int SymbolInterval;
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
                SymbolInterval = 2000,
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
            for (int i = nextTextIndex; i < Text.Length; ++i)
            {
                char c = Char.ToUpper(Text[i]);
                int symbolTime = 0;
                if (c == ' ')
                {
                    // Strict morse code timing says a space is a silent Dah,
                    // which is the duration of 3 dits.
                    symbolTime = activeSettings.DitDuration * 3;
                    await Task.Delay(symbolTime);
                }
                else if (symbols.ContainsKey(c))
                {
                    MorseElement[] seq = symbols[c];
                    foreach (MorseElement el in seq)
                    {
                        switch (el)
                        {
                            case MorseElement.DIT:
                                symbolTime += activeSettings.DitDuration;
                                await Task.Run(() => { ditPlayer.PlaySync(); });
                                break;
                            case MorseElement.DAH:
                                symbolTime += activeSettings.DahDuration;
                                await Task.Run(() => { dahPlayer.PlaySync(); });
                                break;
                        }
                        // Morse code timing says to leave the duration of one dit
                        // between dits and dahs.
                        symbolTime += activeSettings.DitDuration;
                        await Task.Delay(activeSettings.DitDuration);
                    }
                }
                else
                {
                    // Symbol not recognized.
                    // Skip.
                }

                if(symbolTime < activeSettings.SymbolInterval)
                {
                    await Task.Delay(activeSettings.SymbolInterval - symbolTime);
                }
            }
            
        }

        public enum MorseElement
        {
            DIT,
            DAH,
        }

        private static readonly Dictionary<char, MorseElement[]> symbols = new Dictionary<char, MorseElement[]>
        {
            {'0', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH,    }},
            {'1', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH,    }},
            {'2', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH,    }},
            {'3', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DAH,    }},
            {'4', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH,    }},
            {'5', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,    }},
            {'6', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,    }},
            {'7', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,    }},
            {'8', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT,    }},
            {'9', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT,    }},
            {'.', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DIT, MorseElement.DAH, MorseElement.DIT, MorseElement.DAH}},
            {',', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DAH}},
            {':', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT}},
            {'-', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH}},
            {'\'', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT}},
            {'/', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DIT,    }},
            {'?', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT}},
            {'A', new MorseElement[] {MorseElement.DIT, MorseElement.DAH,                   }},
            {'B', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,         }},
            {'C', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DAH, MorseElement.DIT,         }},
            {'D', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT,              }},
            {'E', new MorseElement[] {MorseElement.DIT,                        }},
            {'F', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DAH, MorseElement.DIT,         }},
            {'G', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DIT,              }},
            {'H', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,         }},
            {'I', new MorseElement[] {MorseElement.DIT, MorseElement.DIT,                   }},
            {'J', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DAH,         }},
            {'K', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DAH,              }},
            {'L', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT,         }},
            {'M', new MorseElement[] {MorseElement.DAH, MorseElement.DAH,                   }},
            {'N', new MorseElement[] {MorseElement.DAH, MorseElement.DIT,                   }},
            {'O', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DAH,              }},
            {'P', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DAH, MorseElement.DIT,         }},
            {'Q', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DAH,         }},
            {'R', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DIT,              }},
            {'S', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT,              }},
            {'T', new MorseElement[] {MorseElement.DAH,                        }},
            {'U', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DAH,              }},
            {'V', new MorseElement[] {MorseElement.DIT, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH,         }},
            {'W', new MorseElement[] {MorseElement.DIT, MorseElement.DAH, MorseElement.DAH,              }},
            {'X', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DIT, MorseElement.DAH,         }},
            {'Y', new MorseElement[] {MorseElement.DAH, MorseElement.DIT, MorseElement.DAH, MorseElement.DAH,         }},
            {'Z', new MorseElement[] {MorseElement.DAH, MorseElement.DAH, MorseElement.DIT, MorseElement.DIT          }},

        };
    }
}
