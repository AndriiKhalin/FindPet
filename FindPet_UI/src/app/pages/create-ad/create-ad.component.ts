import { Component, EventEmitter, inject, Output } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RoleService } from '../../services/role.service';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import {NgClass,NgFor,NgIf,AsyncPipe} from "@angular/common";
import { Role } from '../../interfaces/role';
import { Observable, of } from 'rxjs';
import { FormsModule, FormGroup, FormControl,FormBuilder, Validators, ReactiveFormsModule, AbstractControl} from "@angular/forms";
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { ValidationError } from '../../interfaces/validation-error';
import { RegisterRequest } from '../../interfaces/register-request';
import { PetResponse } from '../../interfaces/pet-response';
import { PetService } from '../../services/pet.service';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { UserDetail } from '../../interfaces/user-detail';
import { UploadComponent } from "../../components/upload/upload.component";
import { PetDetail } from '../../interfaces/pet-detail';



@Component({
  selector: 'app-create-ad',
  standalone: true,
  imports: [FormsModule,UploadComponent, RouterOutlet, AsyncPipe, RouterLink, MatIconModule, MatInputModule, NgClass, ReactiveFormsModule, NgFor, NgIf],
  templateUrl: './create-ad.component.html',
  styleUrl: './create-ad.component.scss'
})
export class CreateAdComponent {
  roleService = inject(RoleService);
  authService = inject(AuthService);
  petService=inject(PetService);
  matSnackbar = inject(MatSnackBar);
  form! : FormGroup;
  router = inject(Router);
  errors!: ValidationError[];
  selectedFile: File | null = null;
  response: { filePath: string } = { filePath: '' };
  petId!:string;
  existingPet!: Observable<PetDetail>;
  // @Output() onUploadFinished = new EventEmitter<{ filePath: string }>();

  constructor(private formBuilder: FormBuilder){
    this.form = formBuilder.group({
        "breed": ["", [Validators.required]],
        "nickname": ["", [ Validators.required]],
        "gender": ['', [Validators.required]],
        "color": ['', [Validators.required]],
        "size": ['', [Validators.required]],
        "specialMarks": ['', [Validators.required]],
        "photo" : [""],
        "description": ['', [Validators.required]],
        "lostDate": ['']
    });
}

submit(){
  console.log(this.form);
}

uploadFinished = (event: { filePath: string }) => {
  this.response = event;
}

ngOnInit(): void {
  // this.getPetDetails();

}

// onFileSelected(event: Event): void {
//   const input = event.target as HTMLInputElement;
//   if (input.files && input.files.length > 0) {
//     this.selectedFile = input.files[0];
//   }
// }

update() {
  // const formData = new FormData();
  // formData.append('name', this.form.get('name')?.value);
  // formData.append('email', this.form.get('email')?.value);
  // formData.append('password', this.form.get('password')?.value);
  // formData.append('birthDate', this.form.get('birthDate')?.value);
  // formData.append('phoneNumber', this.form.get('phoneNumber')?.value);
  // if (this.selectedFile) {
  //   formData.append('photo', this.selectedFile);
  // }
  // formData.append('role', this.form.get('role')?.value);

  console.log(this.response);
  console.log(this.response.filePath);

  const uploadData: PetResponse = {
    breed: this.form.get('breed')?.value,
    nickname: this.form.get('nickname')?.value,
    gender: this.form.get('gender')?.value,
    color: this.form.get('color')?.value,
    size: this.form.get('size')?.value,
    specialMarks: this.form.get('specialMarks')?.value,
    photo: this.response.filePath.replace(/\\/g, '/'),
    description: this.form.get('description')?.value,
    lostDate: this.form.get('lostDate')?.value || null,
  };

    console.log(uploadData);
    const user = this.authService.getUserDetail();

    console.log(user?.id);
    // console.log(registrationData);
    this.petService.update(user?.id, uploadData).subscribe(
    {
      next: (response) => {
        console.log(response);
     this.petId=response.id;
        this.matSnackbar.open("Create Ad success", 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
        this.router.navigate(['/searchAnimals']);
      },
      error: (err: HttpErrorResponse) => {
        if (err!.status === 400) {
          this.errors = err!.error;
          this.matSnackbar.open('Validations error', 'Close', {
            duration: 5000,
            horizontalPosition: 'center',
          });
        }
      },
      complete: () => console.log('Create Ad success'),
    }
  );
  }


  getPetDetails() {
    this.petService.getDetail(this.petId)
      .subscribe((data) => {
        console.log(data);
        this.existingPet = of(data);
      });
  }
  // uploadFile = (files: FileList) => {
  //   if (files.length === 0) {
  //     return;
  //   }
  //   let fileToUpload = <File>files[0];
  //   const formData = new FormData();
  //   formData.append('file', fileToUpload, fileToUpload.name);

  //   this.petService.upload(formData).subscribe({
  //     next:(response:any)=>{
  //       this.onUploadFinished.emit({ filePath: response.filePath });
  //     }
  //   });
  // }


// private passwordMatchValidator(
//     control: AbstractControl
//   ): { [key: string]: boolean } | null {
//     const password = control.get('password')?.value;
//     const confirmPassword = control.get('confirmPassword')?.value;

//     if (password !== confirmPassword) {
//       return { passwordMismatch: true };
//     }

//     return null;
//   }

}
