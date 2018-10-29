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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            player.Text = TextToPlay.Text;
            player.Play();
        }

        //private async void PlayChar(char c)
        //{

        //}
    }
}
