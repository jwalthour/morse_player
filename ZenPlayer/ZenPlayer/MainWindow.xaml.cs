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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            //System.IO.Stream s = a.GetManifestResourceStream("ZenPlayer.ZenAudio.346328__isteak__bright-tibetan-bell-ding-b-note-cleaner.wav");
            System.IO.Stream s = a.GetManifestResourceStream("ZenPlayer.ZenAudio.219159__jagadamba__frogblock03.wav");
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(s);
            player.Play();
        }
    }
}
