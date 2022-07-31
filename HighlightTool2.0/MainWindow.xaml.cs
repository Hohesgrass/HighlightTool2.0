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
using Microsoft.Win32;

namespace HighlightTool2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string videoPath;
        private string audioPath;
        public MainWindow()
        {
            InitializeComponent();
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

        private void AudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.audioPath = openFileDialog.FileName;
                MusicTextBox.Text = audioPath;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            FFMPEGHandler ffh = new FFMPEGHandler();

            string[] cutVideos = ffh.CutVideo(videoPath);
            string concVideo = ffh.ConcatenateVideos(cutVideos);

            string extrMP3 = ffh.ExtractAudio(concVideo);
            string extractedWav = ffh.ConvertMP3ToWav(extrMP3);
            string VideoAudioMp3 = ffh.ConvertWAVToMp3(extractedWav);

            string audioConverted = ffh.ConvertMP3ToWav(audioPath);
            TimeSpan ts = ffh.GetDurationDifference(extractedWav, audioConverted);
            string trimmedBackgroundAudio = ffh.TrimWavFile(audioConverted, new TimeSpan(0, 0, 0), ts);
            string BackgroundAudioMp3 = ffh.ConvertWAVToMp3(trimmedBackgroundAudio);

            string mixedAudio = ffh.MixAudio(VideoAudioMp3, BackgroundAudioMp3);
            string mutedVideo = ffh.Mute(concVideo);
            string replacedAudio = ffh.ReplaceAudio(mutedVideo, mixedAudio, NameBox.Text);

        }
    }
}
