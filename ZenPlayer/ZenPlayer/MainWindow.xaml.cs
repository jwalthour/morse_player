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

        public MainWindow()
        {
            InitializeComponent();
            player.LoadFiles(Player.AvailableSettings[0]);
            ButtonPause.IsEnabled = false;
            ButtonStop.IsEnabled = false;
            ButtonPlay.IsEnabled = true;
            TextToPlay.IsEnabled = true;
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            player.Text = TextToPlay.Text;
            ButtonPause.IsEnabled = true;
            ButtonStop.IsEnabled = true;
            ButtonPlay.IsEnabled = false;
            TextToPlay.IsEnabled = false;
            player.Play();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();

            ButtonPause.IsEnabled = false;
            ButtonStop.IsEnabled = true;
            ButtonPlay.IsEnabled = true;
            TextToPlay.IsEnabled = false;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            ButtonPause.IsEnabled = false;
            ButtonStop.IsEnabled = false;
            ButtonPlay.IsEnabled = true;
            TextToPlay.IsEnabled = true;
        }
    }
}
