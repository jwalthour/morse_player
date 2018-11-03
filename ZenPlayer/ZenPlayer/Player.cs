using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZenPlayer
{
    class Player
    {
        public delegate void StateChanged(State newState);
        public event StateChanged OnStateChanged;
        public delegate void ProgressChanged(int nextLetterToPlay);
        public event ProgressChanged OnProgress;

        public enum State
        {
            STOPPED,
            PLAYING,
            PAUSED,
        };

        // Settings
        public int SymbolIntervalMs { get; set; }
        public bool Loop { get; set; }

        private State curState;
        public State CurState
        {
            get
            {
                return curState;
            }
            private set
            {
                curState = value;
                OnStateChanged?.Invoke(curState);
            }
        }
        // When the GUI calls Play() or Stop(), we cancel audio play, which happens slightly later.
        // This variable stores whether the intent is to pause or stop.
        private State nextState = State.STOPPED;

        /// <summary>
        /// A collection of all the settings required for playback.
        /// Yes, I know public member data is generally considered bad practice.
        /// Since this object has no such concept as invalid state, and merely
        /// serves as a collection of variables, I think it's OK to break dogma.
        /// </summary>
        public struct DitDahSettings
        {
            public string ConfigName;
            public string DitResourceName;
            public string DahResourceName;
            public int DitDuration;
            public int DahDuration;
            /// <summary>Duration used as a pause between dits/dahs</summary>
            public int SilentDitDuration;
            /// <summary>Duration used as a pause between symbols</summary>
            public int SilentDahDuration;

            public override string ToString()
            {
                return ConfigName;
            }

        }

        private DitDahSettings activeDitDahSettings;
        private int nextTextIndex = 0;

        /// <summary>
        /// A list of the options that are valid to choose.
        /// Ideally these would be read from a config file or something.
        /// </summary>
        public static readonly DitDahSettings[] AvailableDitDahSettings =
        {
            new DitDahSettings {
                ConfigName          = "440Hz tones",
                DitResourceName     = "ZenPlayer.ZenAudio.dit_100ms_440hz_tone.wav",
                DahResourceName     = "ZenPlayer.ZenAudio.dah_300ms_440hz_tone.wav",
                DitDuration = 100,
                DahDuration = 300,
                SilentDitDuration = 100,
                SilentDahDuration = 300,
            },
            new DitDahSettings {
                ConfigName          = "Chickadee",
                DitResourceName     = "ZenPlayer.ZenAudio.dit_230ms_chickadee_dee.wav",
                DahResourceName     = "ZenPlayer.ZenAudio.dah_456_ms_chickadee_bee.wav",
                DitDuration = 230,
                DahDuration = 456,
                SilentDitDuration = 10,
                SilentDahDuration = 100,
            },
            new DitDahSettings {
                ConfigName          = "Bell and wood",
                DitResourceName     = "ZenPlayer.ZenAudio.dit_67ms_wood_block.wav",
                DahResourceName     = "ZenPlayer.ZenAudio.dah_202ms_bell.wav",
                DitDuration = 67,
                DahDuration = 202,
                SilentDitDuration = 60,
                SilentDahDuration = 180,
            }
        };


        private System.Media.SoundPlayer ditPlayer = null;
        private System.Media.SoundPlayer dahPlayer = null;
        private System.Media.SoundPlayer ambientPlayer = null;
        CancellationTokenSource pauseTokenSource;
        CancellationToken pauseToken;

        public Player()
        {
            CurState = State.STOPPED;
        }

        /// <summary>
        /// Prepares to play the given settings by loading them into memory.
        /// </summary>
        /// <param name="settings"></param>
        public async void LoadDitDahFiles(DitDahSettings settings)
        {
            activeDitDahSettings = settings;
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream ditStream = a.GetManifestResourceStream(activeDitDahSettings.DitResourceName);
            System.IO.Stream dahStream = a.GetManifestResourceStream(activeDitDahSettings.DahResourceName);
            // TODO: do the same for ambient sounds
            ditPlayer = new System.Media.SoundPlayer(ditStream);
            dahPlayer = new System.Media.SoundPlayer(dahStream);
            await Task.Run(() => { ditPlayer.Load(); });
            await Task.Run(() => { dahPlayer.Load(); });

        }

        public string Text { get; set; }

        public void Play()
        {
            nextState = State.STOPPED;
            ambientPlayer?.Play();
            pauseTokenSource = new CancellationTokenSource();
            pauseToken = pauseTokenSource.Token;
            Task task = Task.Run((Action)PlayMorseCode);
        }

        /// <summary>
        /// Common parts of pause and stop
        /// </summary>
        private void CeasePlaying()
        {
            if (CurState == State.PLAYING)
            {
                pauseTokenSource.Cancel();
            }
            ditPlayer?.Stop();
            dahPlayer?.Stop();
            ambientPlayer?.Stop();
        }

        public void Stop()
        {
            nextState = State.STOPPED;
            CeasePlaying();
            // Reset state
            nextTextIndex = 0;
            OnProgress?.Invoke(0);
        }

        public void Pause()
        {
            nextState = State.PAUSED;
            CeasePlaying();
        }

        public MorseElement[] GetSymbolForLetter(char c)
        {
            c = Char.ToUpper(c);
            if (symbols.ContainsKey(c)) {
                return symbols[c];
            } else {
                return null;
            }
        }

        /// <summary>
        /// This is the core player function.
        /// It will run until cancelled, or it reaches the end of the text.
        /// </summary>
        private async void PlayMorseCode()
        {
            CurState = State.PLAYING;
            try
            {
                do
                {
                    for (; nextTextIndex < Text.Length && !pauseToken.IsCancellationRequested; ++nextTextIndex)
                    {
                        DateTime timeSymStart = DateTime.Now;

                        MorseElement[] seq = GetSymbolForLetter(Text[nextTextIndex]);
                        if (Text[nextTextIndex] == ' ')
                        {
                            // Strict morse code timing says a space is a silent Dah,
                            // which is the duration of 3 dits.
                            await Task.Delay(activeDitDahSettings.SilentDahDuration, pauseToken);
                        }
                        else if (seq != null)
                        {
                            foreach (MorseElement el in seq)
                            {
                                switch (el)
                                {
                                    case MorseElement.DIT:
                                        await Task.Run(() => { ditPlayer.PlaySync(); }, pauseToken);
                                        break;
                                    case MorseElement.DAH:
                                        await Task.Run(() => { dahPlayer.PlaySync(); }, pauseToken);
                                        break;
                                }
                                // Morse code timing says to leave the duration of one dit
                                // between dits and dahs.
                                await Task.Delay(activeDitDahSettings.SilentDitDuration, pauseToken);
                            }
                        }
                        else
                        {
                            // Symbol not recognized.
                            // Skip.
                        }
                        OnProgress?.Invoke(nextTextIndex + 1);
                        if (nextTextIndex < Text.Length - 1)
                        {
                            TimeSpan ts = DateTime.Now.Subtract(timeSymStart);

                            if (ts.TotalMilliseconds < SymbolIntervalMs)
                            {
                                await Task.Delay(SymbolIntervalMs - (int)ts.TotalMilliseconds, pauseToken);
                            }
                            else
                            {
                                await Task.Delay(activeDitDahSettings.SilentDahDuration);
                            }
                        }
                    }
                    nextTextIndex = 0;
                } while (Loop);
                CurState = nextState;
            }
            catch (TaskCanceledException)
            {
                CurState = nextState;
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
