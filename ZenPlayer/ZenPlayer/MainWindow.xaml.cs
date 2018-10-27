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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream ditStream = a.GetManifestResourceStream("ZenPlayer.ZenAudio.219159__jagadamba__frogblock03.wav");
            System.IO.Stream dahStream = a.GetManifestResourceStream("ZenPlayer.ZenAudio.trimmed_346328__isteak__bright-tibetan-bell-ding-b-note-cleaner.wav");
            System.Media.SoundPlayer ditPlayer = new System.Media.SoundPlayer(ditStream);
            System.Media.SoundPlayer dahPlayer = new System.Media.SoundPlayer(dahStream);
            await Task.Run(() => { ditPlayer.Load(); });
            await Task.Run(() => { dahPlayer.Load(); });

            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);

            await Task.Delay(450);

            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { dahPlayer.PlaySync(); });
            await Task.Delay(150);
            await Task.Run(() => { ditPlayer.PlaySync(); });
            await Task.Delay(150);

        }

        //private async void PlayChar(char c)
        //{

        //}
    }
}
