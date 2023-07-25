using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

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
        return;
    }
    var caminhoDestino = Directory.GetCurrentDirectory() + "/audios/" + Guid.NewGuid() + ".wav";
    using var stream = File.Create(caminhoDestino);
    await arquivo.CopyToAsync(stream);
    stream.Close();
    var format = new WaveFormat(16000, 16, 1); // 16 kHz sample rate, 16-bit samples, 1 channel (Mono)
    var rawSource = new WaveFormatConversionStream(format, new WaveFileReader(caminhoDestino));

    // Salvar o áudio no formato WAV / PCM com as configurações corretas
    using (var output = new WaveFileWriter(caminhoDestino, format))
    {
        var buffer = new byte[1024];
        int bytesRead;
        do
        {
            bytesRead = rawSource.Read(buffer, 0, buffer.Length);
            output.Write(buffer, 0, bytesRead);
        } while (bytesRead > 0);
    }


    var speechConfig = SpeechConfig.FromSubscription("3cdb22a42bea4c01848a5a81cd92284b", "eastus");
    speechConfig.SpeechRecognitionLanguage = "pt-BR";

    using var audioConfig = AudioConfig.FromWavFileInput(caminhoDestino);
    using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

    Console.WriteLine("Speak into your microphone.");
    var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
    OutputSpeechRecognitionResult(speechRecognitionResult);

});
// This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"

static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
{
    switch (speechRecognitionResult.Reason)
    {
        case ResultReason.RecognizedSpeech:
            Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
            break;
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
}

app.Run();
