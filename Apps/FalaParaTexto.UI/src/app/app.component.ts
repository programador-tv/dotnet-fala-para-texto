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
  recordedAudio = false;
  audioUrl: string | undefined;
  formData = new FormData();
  transcribedText?: string;

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
          this.recordedAudio = true;
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
      this.recordedAudio = true;
    }
  }

  sendAudio() {
    try {
      this.mediaRecorder.stop();
      const formData = new FormData();
  
      const audioBlob = new Blob(this.recordedChunks, { type: "audio/webm" });
      formData.append("audioFile", audioBlob, "recorded_audio.webm");
  
      return this.http
        .post("http://localhost/", formData)
        .subscribe((data: respostaServidor) => {
          this.transcribedText = data.text;
          this.recordedChunks = [];
        });
    } catch (e) {
      return window.alert("Nenhum áudio foi gravado!")
    }
  }

  copyTranscribedText() {
    const copiedTranscribedText = document.getElementById('textoParaCopiar');
    
    if (copiedTranscribedText) {
      const selectionRange = document.createRange();
      selectionRange.selectNodeContents(copiedTranscribedText);
    
      const select = window.getSelection();
      if (select) { 
        select.removeAllRanges();
        select.addRange(selectionRange);
    
        document.execCommand('copy');
    
        select.removeAllRanges();
    
        alert('Texto copiado!');
      }
    }
  }
}
