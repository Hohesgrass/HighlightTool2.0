using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightTool2._0
{
    internal interface IFFMPEGHandler
    {
        /// <summary>
        /// returns cutted video path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String[] CutVideo(String path);

        /// <summary>
        /// returns concatenated video path
        /// </summary>
        /// <param name="videos"></param>
        /// <returns></returns>
        public String ConcatenateVideos(String[] videos);

        /// <summary>
        /// returns path of extracted mp3
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String ExtractAudio(String path);

        /// <summary>
        /// returns path of mixed mp3
        /// </summary>
        /// <param name="patn"></param>
        /// <returns></returns>
        public String MixAudio(String path1, String path2);

        /// <summary>
        /// returns path to converted Audio (WAV to MP3)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String ConvertWAVToMp3(String path);

        /// <summary>
        /// returns path of muted Video
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String Mute(String path);

        /// <summary>
        /// returns path of final video
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String ReplaceAudio(String path, String mixedAudio, String videoName);

        /// <summary>
        /// returns path of trimmed wav file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String TrimWavFile(String path, TimeSpan cutFromStart, TimeSpan cutFromEnd);

        public Boolean IsAudioLonger(String path1, String path2);
    }
}
