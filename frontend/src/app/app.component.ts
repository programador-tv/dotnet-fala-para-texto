import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  mediaRecorder: any;
  recordedChunks: any[] = [];
  isRecording = false;
  audioRecorded = false;
  audioUrl: string | undefined;
  showConsole: boolean = false;
  stoppedRecord:boolean = false;
  formData = new FormData();

  constructor(private http : HttpClient){}

  startRecording() {
    console.log("Gravando")
    this.recordedChunks = [];
    this.isRecording = true;

    navigator.mediaDevices.getUserMedia({ audio: true })
      .then(stream => {
        this.mediaRecorder = new MediaRecorder(stream);

        this.mediaRecorder.ondataavailable = (event: { data: { size: number; }; }) => {
          if (event.data.size > 0) {
            this.recordedChunks.push(event.data);
          }
        };

        this.mediaRecorder.onstop = () => {
          const audioBlob = new Blob(this.recordedChunks, { type: 'audio/wav' });
          this.formData.append('audioFile', audioBlob, 'recorded_audio.wav'); 
          this.audioUrl = URL.createObjectURL(audioBlob);
          this.audioRecorded = true;
        
        };

        this.mediaRecorder.start();
      })
      .catch(error => {
        console.error('Erro ao acessar o microfone: ', error);
      });
  }

  stopRecording() {
    if (this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      this.stoppedRecord = true;
      this.audioRecorded=true;
      this.showConsole = true;
    }
  }
  

  clickouPararGravacao(){
    this.audioRecorded = true;
    //Mostra o reprodutor
    this.showConsole = true;
    
  }

  sendAudio() {
    return this.http.post("http://localhost:5076/",this.formData).subscribe();
  }

  playRecordedAudio() {
    this.showConsole=true;
    const audioElement = new Audio(this.audioUrl);
    // audioElement.play();
  }



}
