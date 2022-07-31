using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace HighlightTool2._0
{
    internal class FFMPEGHandler : IFFMPEGHandler
    {
        private const string FFMPEG_BIN = @".\ffmpeg\bin\";
        private const string FFMPEG_EXE = @".\ffmpeg\bin\ffmpeg.exe";
        private const string FFMPEG_TMP = @".\ffmpeg\tmp\";
        private const string TEMPFOLDER = @".\Temporary Files\";
        private const string FINALFOLDER = @".\Final Videos\";

        public FFMPEGHandler(string videoName)
        {

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = FFMPEG_BIN, TemporaryFilesFolder = FFMPEG_TMP });
        }

        public string[] CutVideo(string path)
        {
            var inputFile = new MediaFile { Filename = path };
            var outputFile = new MediaFile { };

            String[] videos = new String[20];

            using (var engine = new Engine(FFMPEG_EXE))
            {
                engine.GetMetadata(inputFile);
                int length = (int)GetVideoLength(path).TotalSeconds;
                int sections = length / 19;
                int total = 0;
                var options = new ConversionOptions(); 
                for (int i = 0; i < 20; i++)
                {
                    outputFile = new MediaFile { Filename = TEMPFOLDER + "video" + i + ".mp4" };
                    if (i == 19)
                    {
                        total = total - 30;
                    }
                    videos[i] = TEMPFOLDER + "video" + i + ".mp4";
                    options.CutMedia(TimeSpan.FromSeconds(total), TimeSpan.FromSeconds(30));
                    engine.Convert(inputFile, outputFile, options);
                    total = total + sections;
                }
            }
            return videos;
        }

        public string ConcatenateVideos(string[] videos)
        {
            FFMpeg.Join(TEMPFOLDER + "CatVideo.mp4", videos);
            return TEMPFOLDER + "CatVideo.mp4";
        }

        public string ConvertWAVToMp3(string path)
        {
            throw new NotImplementedException();
        } 

        public string ExtractAudio(string path)
        {
            FFMpeg.ExtractAudio(path, TEMPFOLDER + "tempAudio.mp3");
            return TEMPFOLDER + "tempAudio.mp3";
        }

        public string MixAudio(string extractedAudio, string backgroundAudio)
        {
            using (var reader1 = new AudioFileReader(extractedAudio))
            using (var reader2 = new AudioFileReader(backgroundAudio))
            {
                reader2.Volume = 0.025f;
                var mixer = new MixingSampleProvider(new[] { reader1, reader2 });
                WaveFileWriter.CreateWaveFile16(TEMPFOLDER + "tempMixed.wav", mixer);
            }
            return TEMPFOLDER + TEMPFOLDER + "tempMixed.wav";
        }

        public string Mute(string path)
        {
            FFMpeg.Mute(path, TEMPFOLDER + "mutedVideo.mp4");
            return TEMPFOLDER + "mutedVideo.mp4";
        }

        public string ReplaceAudio(string videoPath, string mixedAudio, string videoName)
        {
            FFMpeg.ReplaceAudio(videoPath, mixedAudio, FINALFOLDER + videoName + ".mp4");
            throw new NotImplementedException();
        }

        public String TrimWavFile(string wavPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (WaveFileReader reader = new WaveFileReader(wavPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(TEMPFOLDER + "TrimmedAudio.wav", reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    int endPos = (int)reader.Length - endBytes;

                    TrimWavFile(reader, writer, startPos, endPos);
                }
            }
            return TEMPFOLDER + "TrimmedAudio.wav";
        }

        private void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.WriteData(buffer, 0, bytesRead);
                    }
                }
            }
        }

        private TimeSpan GetVideoLength(string path)
        {
            var mediaInfo = FFProbe.Analyse(path);
            TimeSpan length = mediaInfo.Duration;
            return length;
        }

        private void DeleteTempFiles()
        {
            DirectoryInfo di = new DirectoryInfo(TEMPFOLDER);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public bool IsAudioLonger(string path1, string path2)
        {

            throw new NotImplementedException();
        }
    }
}
