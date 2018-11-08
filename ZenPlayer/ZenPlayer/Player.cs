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
            public string[] DitResourceNames;
            public string[] DahResourceNames;
            /// <summary>Duration used as a pause between dits/dahs</summary>
            public int InterElementPauseDuration;
            /// <summary>Duration used as a pause between symbols</summary>
            public int MinInterSymbolDuration;

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
                DitResourceNames    = new string[] {"ZenPlayer.ZenAudio.dit_100ms_440hz_tone.wav" },
                DahResourceNames    = new string[] {"ZenPlayer.ZenAudio.dah_300ms_440hz_tone.wav"},
                InterElementPauseDuration = 100,
                MinInterSymbolDuration = 300,
            },
            new DitDahSettings {
                ConfigName          = "Chickadee",
                DitResourceNames    = new string[] {
                    "ZenPlayer.ZenAudio.dit_219ms_chickadee_dee.wav",
                    "ZenPlayer.ZenAudio.dit_219ms_chickadee_dee1.wav",
                    "ZenPlayer.ZenAudio.dit_222ms_chickadee_dee.wav",
                    "ZenPlayer.ZenAudio.dit_226ms_chickadee_dee.wav",
                    "ZenPlayer.ZenAudio.dit_229ms_chickadee_dee.wav",
                    "ZenPlayer.ZenAudio.dit_230ms_chickadee_dee.wav",
                },
                DahResourceNames    = new string[] {
                    "ZenPlayer.ZenAudio.dah_330ms_chickadee_fee.wav",
                    "ZenPlayer.ZenAudio.dah_364ms_chickadee_fee.wav",
                    "ZenPlayer.ZenAudio.dah_407ms_chickadee_fee.wav",
                    "ZenPlayer.ZenAudio.dah_442ms_chickadee_bee.wav",
                    "ZenPlayer.ZenAudio.dah_456ms_chickadee_bee.wav",
                    "ZenPlayer.ZenAudio.dah_463ms_chickadee_bee.wav",
                },
                InterElementPauseDuration = 0,
                MinInterSymbolDuration = 1000,
            },
            new DitDahSettings {
                ConfigName          = "Bell and wood",
                DitResourceNames    = new string[] {"ZenPlayer.ZenAudio.dit_67ms_wood_block.wav" },
                DahResourceNames    = new string[] {"ZenPlayer.ZenAudio.dah_202ms_bell.wav" },
                InterElementPauseDuration = 60,
                MinInterSymbolDuration = 180,
            }
        };


        private List<System.Media.SoundPlayer> ditPlayers = null;
        private List<System.Media.SoundPlayer> dahPlayers = null;
        private CancellationTokenSource pauseTokenSource;
        private CancellationToken pauseToken;
        private System.Media.SoundPlayer curDitPlayer = null;
        private System.Media.SoundPlayer curDahPlayer = null;
        private int nextDitIndex = 0;
        private int nextDahIndex = 0;

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
            ditPlayers = new List<System.Media.SoundPlayer>();
            dahPlayers = new List<System.Media.SoundPlayer>();
            activeDitDahSettings = settings;
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            foreach(string name in settings.DitResourceNames)
            {
                System.IO.Stream ditStream = a.GetManifestResourceStream(name);
                ditPlayers.Add(new System.Media.SoundPlayer(ditStream));
                // We could probably run all these in parallel.
                // But generally they're loading from the same disk, so I'm not sure that'd save any time.
                await Task.Run(() => { ditPlayers.Last().Load(); });
            }
            foreach (string name in settings.DahResourceNames)
            {
                System.IO.Stream dahStream = a.GetManifestResourceStream(name);
                dahPlayers.Add(new System.Media.SoundPlayer(dahStream));
                // We could probably run all these in parallel.
                // But generally they're loading from the same disk, so I'm not sure that'd save any time.
                await Task.Run(() => { dahPlayers.Last().Load(); });
            }
            ditPlayers.Shuffle();
            dahPlayers.Shuffle();
        }

        public string Text { get; set; }

        public void Play()
        {
            nextState = State.STOPPED;
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
            curDitPlayer?.Stop();
            curDahPlayer?.Stop();
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
                        OnProgress?.Invoke(nextTextIndex + 1);

                        MorseElement[] seq = GetSymbolForLetter(Text[nextTextIndex]);
                        if (Text[nextTextIndex] == ' ')
                        {
                            // Strict morse code timing says a space is a silent Dah,
                            // which is the duration of 3 dits.
                            await Task.Delay(activeDitDahSettings.MinInterSymbolDuration, pauseToken);
                        }
                        else if (seq != null)
                        {
                            foreach (MorseElement el in seq)
                            {
                                switch (el)
                                {
                                    case MorseElement.DIT:
                                        curDitPlayer = ditPlayers[nextDitIndex++];
                                        if(nextDitIndex >= ditPlayers.Count) // Can we do this with an iterator?
                                        {
                                            ditPlayers.Shuffle();
                                            nextDitIndex = 0;
                                        }
                                        await Task.Run(() => { curDitPlayer.PlaySync(); }, pauseToken);
                                        break;
                                    case MorseElement.DAH:
                                        curDahPlayer = dahPlayers[nextDahIndex++];
                                        if (nextDahIndex >= dahPlayers.Count)
                                        {
                                            dahPlayers.Shuffle();
                                            nextDahIndex = 0;
                                        }
                                        await Task.Run(() => { curDahPlayer.PlaySync(); }, pauseToken);
                                        break;
                                }
                                // Morse code timing says to leave the duration of one dit
                                // between dits and dahs.
                                await Task.Delay(activeDitDahSettings.InterElementPauseDuration, pauseToken);
                            }
                        }
                        else
                        {
                            // Symbol not recognized.
                            // Skip.
                        }
                        if (nextTextIndex < Text.Length - 1 || Loop)
                        {
                            TimeSpan ts = DateTime.Now.Subtract(timeSymStart);

                            if (ts.TotalMilliseconds + activeDitDahSettings.MinInterSymbolDuration < SymbolIntervalMs)
                            {
                                await Task.Delay(SymbolIntervalMs - (int)(ts.TotalMilliseconds + activeDitDahSettings.MinInterSymbolDuration), pauseToken);
                            }
                            else
                            {
                                await Task.Delay(activeDitDahSettings.MinInterSymbolDuration);
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
