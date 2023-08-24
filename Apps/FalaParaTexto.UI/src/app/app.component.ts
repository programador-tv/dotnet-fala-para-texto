import { HttpClient } from "@angular/common/http";
import { Component } from "@angular/core";
import { respostaServidor } from "src/Interfaces/interface";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
})
export class AppComponent {
  mediaRecorder: any;
  recordedChunks: any[] = [];
  isRecording = false;
  audioRecorded = false;
  audioUrl: string | undefined;
  stoppedRecord: boolean = false;
  formData = new FormData();
  text?: string;

  constructor(private http: HttpClient) {}

  startRecording() {
    console.log("Gravando");
    this.recordedChunks = [];
    this.isRecording = true;

    navigator.mediaDevices
      .getUserMedia({ audio: true })
      .then((stream) => {
        this.mediaRecorder = new MediaRecorder(stream);

        this.mediaRecorder.ondataavailable = (event: {
          data: { size: number };
        }) => {
          if (event.data.size > 0) {
            this.recordedChunks.push(event.data);
          }
        };

        this.mediaRecorder.onstop = () => {
          const audioBlob = new Blob(this.recordedChunks, {
            type: "audio/webm",
          });
          this.formData.append("audioFile", audioBlob, "recorded_audio.webm");
          this.audioUrl = URL.createObjectURL(audioBlob);
          this.audioRecorded = true;
        };

        this.mediaRecorder.start();
      })
      .catch((error) => {
        console.error("Erro ao acessar o microfone: ", error);
      });
  }

  stopRecording() {
    if (this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      this.stoppedRecord = true;
      this.audioRecorded = true;

    }
  }

  clickouPararGravacao() {
    this.audioRecorded = true;
    //Mostra o reprodutor
  }

  sendAudio() {
    try {
      this.mediaRecorder.stop();
      const formData = new FormData();
  
      const audioBlob = new Blob(this.recordedChunks, { type: "audio/webm" });
      formData.append("audioFile", audioBlob, "recorded_audio.webm");
  
      return this.http
        .post("http://localhost:5076/", formData)
        .subscribe((data: respostaServidor) => {
          // Limpa a variável recordedChunks após o envio
          this.text = data.text;
          console.log(data);
          this.recordedChunks = [];
        });
    } catch (e) {
      return window.alert("Nenhum áudio foi gravado!")
    }
  }


}
