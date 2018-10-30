﻿using System;
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
        }

        private void Player_OnProgress(double fracCompleted)
        {
            // Perform on GUI thread
            Dispatcher.BeginInvoke(
               new Action(() =>
               {
                   ProgressPlayback.Value = fracCompleted * ProgressPlayback.Maximum;
               })
            );
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
