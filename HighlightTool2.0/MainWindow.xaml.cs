using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using NAudio.Wave;

namespace HighlightTool2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string videoPath;
        private string audioPath;
        private float volumeSliderValue;
        public MainWindow()
        {
            InitializeComponent();
            CreateFolders();
        }

        private void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.videoPath = openFileDialog.FileName;
                VideoTextBox.Text = videoPath;
            }
        }

        //private void AudioButton_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        this.audioPath = openFileDialog.FileName;
        //        MusicTextBox.Text = audioPath;
        //    }
        //}

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                FFMPEGHandler ffh = new FFMPEGHandler();
                Dispatcher.Invoke(() => ProgressBar.Value = 15);
                string[] cutVideos = ffh.CutVideo(videoPath);
                Dispatcher.Invoke(() => ProgressBar.Value = 50);
                string concVideo = ffh.ConcatenateVideos(cutVideos, Dispatcher.Invoke(() => NameBox.Text));
                Dispatcher.Invoke(() => ProgressBar.Value = 75);
                Dispatcher.Invoke(() => ProgressBar.Value = 100);
            });
        }

        //private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    volumeSliderValue = Convert.ToSingle(Math.Round(VolumeSlider.Value, 3));
        //    VolumeBox.Text = volumeSliderValue + "";
        //}

        private void CreateFolders()
        {
            string tmpFolder = @".\Temporary Files\";
            string finalFolder = @".\Final Videos\";

            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }

            if (!Directory.Exists(finalFolder))
            {
                Directory.CreateDirectory(finalFolder);
            }
        }
    }
}
