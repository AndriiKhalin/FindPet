import { HttpClient, HttpEventType, HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.scss'
})
export class UploadComponent{
  authService = inject(AuthService);
  matSnackbar = inject(MatSnackBar);
  @Output() onUploadFinished = new EventEmitter<{ filePath: string }>();
  constructor(private http: HttpClient) {}

  // uploadFile(files: FileList, type: 'animal' | 'user') {
  //   const file = files.item(0);
  //   const formData = new FormData();
  //   formData.append('file', file, file.name);

  //   const url = type === 'animal' ? 'api/animal/upload' : 'api/user/upload';

  //   this.http.post(url, formData).subscribe(response => {
  //     console.log('Upload successful', response);
  //   }, error => {
  //     console.error('Upload failed', error);
  //   });

    uploadFile = (files: FileList) => {
      if (files.length === 0) {
        return;
      }
      let fileToUpload = <File>files[0];
      const formData = new FormData();
      formData.append('file', fileToUpload, fileToUpload.name);

      this.authService.upload(formData).subscribe({
        next:(response:any)=>{
          this.onUploadFinished.emit({ filePath: response.filePath });
        }
      });
    }
  }
