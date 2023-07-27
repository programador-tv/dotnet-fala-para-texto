using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Diagnostics;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build(); var speechKey = configuration["SPEECH_KEY"]; var speechRegion = configuration["SPEECH_REGION"];
app.UseCors(builder => builder
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());

app.MapPost("/", async (HttpContext httpContext) =>
{
    var arquivo = httpContext.Request.Form.Files.GetFile("audioFile");
    if (arquivo == null)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("Nenhum arquivo foi enviado.");
    }

    var webmPath = Directory.GetCurrentDirectory() + "/audios/" + Guid.NewGuid() + ".webm";
    var wavPath = Directory.GetCurrentDirectory() + "/audios/" + Guid.NewGuid() + ".wav";


    // Salvar o áudio como WAV no disco
    using (var stream = File.Create(webmPath))
    {
        await arquivo.CopyToAsync(stream);
    }
    // Converter o arquivo MP3 para WAV usando o ffmpeg
    var ffmpegPath = "ffmpeg"; // Defina o caminho para o executável ffmpeg
    var ffmpegArgs = $"-i \"{webmPath}\" -acodec pcm_s16le -ac 1 -ar 16000 \"{wavPath}\"";

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

    var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
    speechConfig.SpeechRecognitionLanguage = "pt-BR";
    string result;
    // Utilizar o arquivo WAV recém-salvo
    using (var audioConfig = AudioConfig.FromWavFileInput(wavPath))
    using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig))
    {
        Console.WriteLine("Speak into your microphone.");
        var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        result = OutputSpeechRecognitionResult(speechRecognitionResult);
    }
    if (!string.IsNullOrEmpty(result))
    {
        var json = JsonConvert.SerializeObject(new {text=result});
        await httpContext.Response.WriteAsync(json);
    }
    else
    {
        await httpContext.Response.WriteAsync("Não foi possível transcrever o áudio");
    }
});

// O código da função OutputSpeechRecognitionResult deve estar dentro do contexto
// do programa principal para funcionar corretamente
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


app.Run();
