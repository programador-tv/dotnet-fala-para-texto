using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace FalaParaTexto.Core
{
    public class AzureSpeechToTextService
    {
        private string _baseDirectory;
        private string _speechKey;
        private string _speechRegion;

        public AzureSpeechToTextService(string speechKey, string speechRegion)
        {
            _baseDirectory = Directory.GetCurrentDirectory() + "/audios/";
            _speechKey = speechKey;
            _speechRegion = speechRegion;
        }

        public async Task<string> SpeechToTextAsync(string wavFile)
        {
            var speechConfig = SpeechConfig.FromSubscription(_speechKey, _speechRegion);
            speechConfig.SpeechRecognitionLanguage = "pt-BR";
            string result;
            // Utilizar o arquivo WAV recém-salvo
            using (var audioConfig = AudioConfig.FromWavFileInput(_baseDirectory + wavFile))
            using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig))
            {
                Console.WriteLine("Speak into your microphone.");
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
                result = OutputSpeechRecognitionResult(speechRecognitionResult);
            }
            return result;
        }

        static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                    return speechRecognitionResult.Text;
                case ResultReason.NoMatch:
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
            return string.Empty;
        }

    }
}