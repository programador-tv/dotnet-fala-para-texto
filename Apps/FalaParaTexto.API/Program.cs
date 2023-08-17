using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Diagnostics;
using Newtonsoft.Json;
using FalaParaTexto.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build(); var speechKey = configuration["SPEECH_KEY"]; var speechRegion = configuration["SPEECH_REGION"];
app.UseCors(builder => builder
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());

var _audioManager = new AudioFileManager();
var _audioProcess = new AudioProcessManager();
var _azureSpeechToText = new AzureSpeechToTextService(speechKey,speechRegion);

app.MapPost("/", async (HttpContext httpContext) =>
{
    var arquivo = httpContext.Request.Form.Files.GetFile("audioFile");
    if (arquivo == null)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("Nenhum arquivo foi enviado.");
    }

    var webmFile = Guid.NewGuid() + ".webm";
    var wavFile = Guid.NewGuid() + ".wav";


    await _audioManager.SaveFile(arquivo, webmFile);

    await _audioProcess.ConvertWebmToWavOnFileSystemAsync(webmFile, wavFile);

    string result = await _azureSpeechToText.SpeechToTextAsync(wavFile);
    
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




app.Run();
