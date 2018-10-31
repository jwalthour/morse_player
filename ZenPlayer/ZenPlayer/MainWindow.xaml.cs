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
            player.LoadFiles(Player.AvailableSettings[0]);
            player.OnStateChanged += Player_OnStateChanged;
            player.OnProgress += Player_OnProgress;
            ButtonPause.IsEnabled = false;
            ButtonStop.IsEnabled = false;
            ButtonPlay.IsEnabled = true;
            TextToPlay.IsEnabled = true;

            LabelSymbolIntervalMin.Content = SYMBOL_INTERVAL_MIN.ToString();
            LabelSymbolIntervalMax.Content = SYMBOL_INTERVAL_MAX.ToString();

            SliderSymbolInterval.Minimum = SYMBOL_INTERVAL_MIN;
            SliderSymbolInterval.Maximum = SYMBOL_INTERVAL_MAX;

            ClearPlayingText();
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
            if (i > 0) {
                TextBlockPastText.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring(Math.Max(0, i - LETTERS_EITHER_SIDE), Math.Min(i, LETTERS_EITHER_SIDE));
            } else {
                TextBlockPastText.Text = "";
            }
            TextBlockCurLetter.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring(i, 1);
            if (i < TextToPlay.Text.Length - 1) {
                TextBlockFutureText.Text = TextToPlay.Text.Replace("\r\n", "  ").Substring(i + 1, Math.Min(LETTERS_EITHER_SIDE, TextToPlay.Text.Length - i - 1));
            }
            else {
                TextBlockFutureText.Text = "";
            }

            Player.MorseElement[] symbol = player.GetSymbolForLetter(TextBlockCurLetter.Text[0]);
            if(symbol == null)
            {
                TextBlockCurSymbol.Text = "";
            }
            else
            {
                string symStr = " ";
                foreach(Player.MorseElement el in symbol)
                {
                    symStr += (el == Player.MorseElement.DIT) ? "• " : "– ";
                }
                TextBlockCurSymbol.Text = symStr;
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
                           ClearPlayingText();
                           break;
                       case Player.State.PLAYING:
                           ButtonPause.IsEnabled = true;
                           ButtonStop.IsEnabled = true;
                           ButtonPlay.IsEnabled = false;
                           TextToPlay.IsEnabled = false;
                           break;
                       case Player.State.PAUSED:
                           ButtonPause.IsEnabled = false;
                           ButtonStop.IsEnabled = true;
                           ButtonPlay.IsEnabled = true;
                           TextToPlay.IsEnabled = false;
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
    }
}
