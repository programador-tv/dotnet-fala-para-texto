using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FalaParaTexto.Core
{
    public class AudioProcessManager
    {
        private string _baseDirectory;
        public AudioProcessManager()
        {
            _baseDirectory = Directory.GetCurrentDirectory() + "/audios/";
        }

        public async Task ConvertWebmToWavOnFileSystemAsync(string originFile, string destinFile)
        {
            var ffmpegPath = "ffmpeg"; // Defina o caminho para o executável ffmpeg
            var ffmpegArgs = $"-i \"{_baseDirectory + originFile}\" -acodec pcm_s16le -ac 1 -ar 16000 \"{_baseDirectory + destinFile}\"";

            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();
            await ffmpegProcess.WaitForExitAsync();
        }
    }
}