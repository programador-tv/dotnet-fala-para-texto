using System.IO;
using Microsoft.AspNetCore.Http;

namespace FalaParaTexto.Core;

public class AudioFileManager
{
    private readonly string _baseDirectory;
    public AudioFileManager(){
        _baseDirectory = Directory.GetCurrentDirectory() + "/audios/";
    }

    public async Task SaveFile(IFormFile file, string fileName)
    {
        using (var stream = File.Create(_baseDirectory + fileName))
        {
            await file.CopyToAsync(stream);
        }
    }

}
