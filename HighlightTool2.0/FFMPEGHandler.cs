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
using NAudio.MediaFoundation;
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
        private int Mp3Counter = 0;
        private int TrimCounter = 0;

        public FFMPEGHandler()
        {
            MediaFoundationApi.Startup();
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = FFMPEG_BIN, TemporaryFilesFolder = FFMPEG_TMP });
        }

        public string[] CutVideo(string path)
        {
            var inputFile = new MediaFile { Filename = path };
            var outputFile = new MediaFile { };

            string[] videos = new string[20];

            using var engine = new Engine(FFMPEG_EXE);
            
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
            
            return videos;
        }

        public string CutTo10Min(string path, string videoName)
        {
            var inputFile = new MediaFile { Filename = path};
            var outputFile = new MediaFile { Filename = FINALFOLDER + videoName + ".mp4"};


            using var engine = new Engine(FFMPEG_EXE);

            engine.GetMetadata(inputFile);

            var options = new ConversionOptions();

            //options.CutMedia(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(600));
            //engine.Convert(inputFile, outputFile, options);

           // DeleteTempFiles();

            return FINALFOLDER + videoName + ".mp4";
        }

        public String CombineAudio(string path)
        {
            var first = new AudioFileReader(path);
            var second = new AudioFileReader(path);
            var third = new AudioFileReader(path);
            var fourth = new AudioFileReader(path);
            var fifth = new AudioFileReader(path);

            var playlist = new ConcatenatingSampleProvider(new[] { first, second, third, fourth, fifth });

            WaveFileWriter.CreateWaveFile16(TEMPFOLDER + "stretchedAudio.wav", playlist);

            return TEMPFOLDER + "stretchedAudio.wav";
        }

        public string ConcatenateVideos(string[] videos, string videoName)
        {
            FFMpeg.Join(FINALFOLDER + videoName + ".mp4", videos);
            return FINALFOLDER + videoName + ".mp4";
        }

        public TimeSpan GetDurationDifference(string path, string path2)
        {
            WaveFileReader wf = new WaveFileReader(path);
            WaveFileReader wf2 = new WaveFileReader(path2);

            TimeSpan ts = wf2.TotalTime - wf.TotalTime;
            return ts;
        }

        public string ConvertWAVToMp3(string path)
        {
            string mp3FilePath = Path.Combine(TEMPFOLDER, "convertedToMP3" + Mp3Counter + ".mp3");
            using (var reader = new WaveFileReader(path))
            {
                try
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, mp3FilePath);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Mp3Counter++;

            return mp3FilePath;
        } 

        public string ExtractAudio(string path)
        {
            FFMpeg.ExtractAudio(path, TEMPFOLDER + "tempAudio.mp3");
            
            return TEMPFOLDER + "tempAudio.mp3";
        }

        public WaveFormat GetWavFormat(string path)
        {
            var reader1 = new AudioFileReader(path);
            WaveFormat wavFormat = reader1.WaveFormat;
            return wavFormat;
        }
        public string ChangeWavFormat(string path, WaveFormat format)
        {
            var newFormat = new WaveFormat(44800, 32, 1);
            using (var reader = new WaveFileReader(path))
            {
                using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                {
                    WaveFileWriter.CreateWaveFile(TEMPFOLDER + "HZ.wav", conversionStream);
                }
            }
            return TEMPFOLDER + "HZ.wav";
        }
        public string MixAudio(string extractedAudio, string backgroundAudio, float volume)
        {
            var reader1 = new AudioFileReader(extractedAudio);
            var reader2 = new AudioFileReader(backgroundAudio);

            if (reader1.WaveFormat != reader2.WaveFormat)
            {
               // string newAudio = ChangeWavFormat(backgroundAudio, reader1.WaveFormat);
               // reader2 = new AudioFileReader(newAudio);
            }

            using (reader1)
            using (reader2)    
            {
                //0.025f
                reader2.Volume = volume;
                var mixer = new MixingSampleProvider(new[] { reader1, reader2 });
                WaveFileWriter.CreateWaveFile16(TEMPFOLDER + "tempMixed.wav", mixer);
            }

            return TEMPFOLDER + "tempMixed.wav";
        }

        public string Mute(string path)
        {
            FFMpeg.Mute(path, TEMPFOLDER + "mutedVideo.mp4");

            return TEMPFOLDER + "mutedVideo.mp4";
        }

        public string ReplaceAudio(string videoPath, string mixedAudio, string videoName)
        {
            FFMpeg.ReplaceAudio(videoPath, mixedAudio, TEMPFOLDER + videoName + ".mp4");

            return TEMPFOLDER + videoName + ".mp4";
        }

        public String TrimWavFile(string wavPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (WaveFileReader reader = new WaveFileReader(wavPath))
            {
                using WaveFileWriter writer = new WaveFileWriter(TEMPFOLDER + "TrimmedAudio" + TrimCounter + ".wav", reader.WaveFormat);
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                int endPos = (int)reader.Length - endBytes;

                TrimWavFile(reader, writer, startPos, endPos);
                TrimCounter++;
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
        public string ConvertMP3ToWav(string path)
        {
            string infile = path;
            string outfile = TEMPFOLDER + "convertedToWav.wav";
            using (var reader = new Mp3FileReader(infile))
            {
                WaveFileWriter.CreateWaveFile(outfile, reader);
            }
            return outfile;
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
