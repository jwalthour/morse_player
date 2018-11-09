using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZenPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Player player = new Player();

        private const int SYMBOL_INTERVAL_MIN = 0;
        private const int SYMBOL_INTERVAL_MAX = 3000;

        private const int LETTERS_EITHER_SIDE = 50;

        public MainWindow()
        {
            InitializeComponent();
            ButtonPlay.IsEnabled = false;
            player.OnStateChanged += Player_OnStateChanged;
            player.OnProgress += Player_OnProgress;
            ButtonPause.IsEnabled = false;
            ButtonStop.IsEnabled = false;
            ButtonPlay.IsEnabled = true;
            TextToPlay.IsEnabled = true;

            //LabelSymbolIntervalMin.Content = SYMBOL_INTERVAL_MIN.ToString();
            //LabelSymbolIntervalMax.Content = SYMBOL_INTERVAL_MAX.ToString();

            SliderSymbolInterval.Minimum = SYMBOL_INTERVAL_MIN;
            SliderSymbolInterval.Maximum = SYMBOL_INTERVAL_MAX;

            ClearPlayingText();

            foreach(Player.DitDahSettings dds in Player.AvailableDitDahSettings) {
                ComboBoxDitDahSel.Items.Add(dds);
            }
            ComboBoxDitDahSel.SelectionChanged += ComboBoxDitDahSel_SelectionChanged;
            ComboBoxDitDahSel.SelectedIndex = 0;
        }

        private void ComboBoxDitDahSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            player.LoadDitDahFiles((Player.DitDahSettings)ComboBoxDitDahSel.SelectedItem);
        }

        private void Player_OnProgress(int nextLetterToPlay)
        {
            // Perform on GUI thread
            Dispatcher.BeginInvoke(
               new Action(() =>
               {
                   ProgressPlayback.Value = ((double)(nextLetterToPlay) / TextToPlay.Text.Length) * ProgressPlayback.Maximum;

                   UpdatePlayingText(nextLetterToPlay);
               })
            );
        }

        /// <summary>
        /// Clear the text areas displaying the currently-playing character
        /// </summary>
        private void ClearPlayingText()
        {
            TextBlockPastText.Text = "";
            TextBlockFutureText.Text = "";
            TextBlockCurLetter.Text = "";
            TextBlockCurSymbol.Text = "";
        }

        /// <summary>
        /// Update the text for the next playing char
        /// </summary>
        /// <param name="i">Index into TextToPlay.Text for the next letter to play</param>
        private void UpdatePlayingText(int i)
        {
            // -1 to display the character that just played
            // 0 to display the character that's about to play
            const int OFFSET = -1;
            if ((i+OFFSET) > 0) {
                TextBlockPastText.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring(Math.Max(0, (i+OFFSET) - LETTERS_EITHER_SIDE), Math.Min((i+OFFSET), LETTERS_EITHER_SIDE));
            } else {
                TextBlockPastText.Text = "";
            }
            if ((i + OFFSET) > 0 && (i+OFFSET) < TextToPlay.Text.Length)
            {
                TextBlockCurLetter.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring((i+OFFSET), 1);
            } else
            {
                TextBlockCurLetter.Text = "";
                TextBlockCurSymbol.Text = "";
            }
            if ((i + OFFSET) > 0 && (i +OFFSET) < TextToPlay.Text.Length - 1) {
                TextBlockFutureText.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring((i+OFFSET) + 1, Math.Min(LETTERS_EITHER_SIDE, TextToPlay.Text.Length - (i+OFFSET) - 1));
            }
            else {
                TextBlockFutureText.Text = "";
            }

            if (TextBlockCurLetter.Text.Length > 0)
            {
                Player.MorseElement[] symbol = player.GetSymbolForLetter(TextBlockCurLetter.Text[0]);
                if (symbol == null)
                {
                    TextBlockCurSymbol.Text = "";
                }
                else
                {
                    string symStr = " ";
                    foreach (Player.MorseElement el in symbol)
                    {
                        symStr += (el == Player.MorseElement.DIT) ? "• " : "– ";
                    }
                    TextBlockCurSymbol.Text = symStr;
                }
            }
        }

        private void Player_OnStateChanged(Player.State newState)
        {
            // Perform on GUI thread
            Dispatcher.BeginInvoke(
               new Action(() =>
               {
                   switch (newState)
                   {
                       case Player.State.STOPPED:
                           ButtonPause.IsEnabled = false;
                           ButtonStop.IsEnabled = false;
                           ButtonPlay.IsEnabled = true;
                           TextToPlay.IsEnabled = true;
                           ComboBoxDitDahSel.IsEnabled = true;
                           ClearPlayingText();
                           break;
                       case Player.State.PLAYING:
                           ButtonPause.IsEnabled = true;
                           ButtonStop.IsEnabled = true;
                           ButtonPlay.IsEnabled = false;
                           TextToPlay.IsEnabled = false;
                           ComboBoxDitDahSel.IsEnabled = false;
                           break;
                       case Player.State.PAUSED:
                           ButtonPause.IsEnabled = false;
                           ButtonStop.IsEnabled = true;
                           ButtonPlay.IsEnabled = true;
                           TextToPlay.IsEnabled = false;
                           ComboBoxDitDahSel.IsEnabled = false;
                           break;
                   }

               })
            );
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            player.Text = TextToPlay.Text;
            player.Play();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.SymbolIntervalMs = (int)(e.NewValue);
        }

        private void CheckBoxLoop_Checked(object sender, RoutedEventArgs e)
        {
            player.Loop = (bool)(CheckBoxLoop.IsChecked);
        }
    }
}
