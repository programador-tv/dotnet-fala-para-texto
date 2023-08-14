# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

COPY ./FalaParaTexto.Core/ FalaParaTexto.Core/
COPY ./FalaParaTexto.API/ FalaParaTexto.API/

RUN dotnet publish ./FalaParaTexto.API/FalaParaTexto.API.csproj -c release -o /out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

RUN apt-get update && apt-get install -y ffmpeg

WORKDIR /app
COPY --from=build /out ./

EXPOSE 80

ENTRYPOINT ["dotnet", "FalaParaTexto.API.dll"]