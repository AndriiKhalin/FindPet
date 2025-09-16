import { Component, OnInit, ViewChild, ElementRef, inject } from '@angular/core';
import { RouterOutlet, RouterLink, Router} from "@angular/router";
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, FormGroup, FormControl,FormBuilder, Validators, ReactiveFormsModule, AbstractControl} from "@angular/forms";
import {NgClass,NgFor,NgIf,AsyncPipe} from "@angular/common";
import intlTelInput from 'intl-tel-input';
import { RoleService } from '../../services/role.service';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Role } from '../../interfaces/role';
import { Observable } from 'rxjs';
import { ValidationError } from '../../interfaces/validation-error';
import { HttpErrorResponse } from '@angular/common/http';
import { RegisterRequest } from '../../interfaces/register-request';
import { UploadComponent } from "../../components/upload/upload.component";



@Component({
    selector: 'app-register',
    standalone: true,
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
    imports: [FormsModule, RouterOutlet, AsyncPipe, RouterLink, MatIconModule, MatInputModule, NgClass, ReactiveFormsModule, NgFor, NgIf, UploadComponent]
})
export class RegisterComponent implements OnInit{
  roleService = inject(RoleService);
  authService = inject(AuthService);
  matSnackbar = inject(MatSnackBar);
  roles$!: Observable<Role[]>;
  showPassword: boolean = false;
  form! : FormGroup;
  router = inject(Router);
  errors!: ValidationError[];
  selectedFile: File | null = null;
  response: { filePath: string } = { filePath: '' };

  constructor(private formBuilder: FormBuilder){
    this.form = formBuilder.group({
        "name": ["", [Validators.required]],
        "email": ["", [ Validators.required,Validators.email]],
        "password": ['', [Validators.required]],
        "birthDate": ['', [Validators.required]],
        "phoneNumber":[ "", [Validators.required]],
        "photo" : [""],
        "role": [''],
        "acceptTerm":['',[Validators.requiredTrue,Validators.required]]
    });
}

togglePassword(): void {
  this.showPassword =!this.showPassword;
}
submit(){
  console.log(this.form);
}

uploadFinished = (event: { filePath: string }) => {
  this.response = event;
}

ngOnInit(): void {
  const input = document.querySelector("#phoneNumber") as HTMLInputElement;
  intlTelInput(input, {
    utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@23.0.12/build/js/utils.js",
    separateDialCode:true
  });


  this.roles$ = this.roleService.getRoles();
}

// onFileSelected(event: Event): void {
//   const input = event.target as HTMLInputElement;
//   if (input.files && input.files.length > 0) {
//     this.selectedFile = input.files[0];
//   }
// }

register() {
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


  const roles = this.form.get('role')?.value || [];

  console.log(this.response);
  console.log(this.response.filePath);

  const registrationData: RegisterRequest = {
    name: this.form.get('name')?.value,
    email: this.form.get('email')?.value,
    password: this.form.get('password')?.value,
    birthDate: this.form.get('birthDate')?.value,
    phoneNumber: this.form.get('phoneNumber')?.value,
    photo: this.response.filePath,
    role: this.form.get('role')?.value || null,
  };

    console.log(registrationData);
    // console.log(registrationData);
    console.log(roles);

  this.authService.register(registrationData).subscribe(
    {
      next: (response) => {
        console.log(response);

        this.matSnackbar.open(response.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
        this.router.navigate(['/login']);
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
      complete: () => console.log('Register success'),
    }
  );
  }

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
