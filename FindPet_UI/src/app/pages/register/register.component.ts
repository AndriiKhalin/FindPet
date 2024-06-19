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

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule,RouterOutlet,AsyncPipe, RouterLink,MatIconModule,MatInputModule,NgClass,ReactiveFormsModule,NgFor,NgIf],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit{
  roleService = inject(RoleService);
  authService = inject(AuthService);
  matSnackbar = inject(MatSnackBar);
  roles$!: Observable<Role[]>;
  showPassword: boolean = false;
  form : FormGroup;
  router = inject(Router);
  errors!: ValidationError[];

  constructor(private formBuilder: FormBuilder){
      this.form = formBuilder.group({
          "name": ["", [Validators.required]],
          "email": ["", [ Validators.required, Validators.email]],
          "password": ['', [Validators.required]],
          "confirmPassword" : ["",[Validators.required]],
          "phoneNumber":[ "", [Validators.required]],
          "roles": [''],
          "photo":['',[Validators.required]]
      },
      {
        validator: this.passwordMatchValidator,
      }
    );
  }

togglePassword(): void {
  this.showPassword =!this.showPassword;
}
submit(){
  console.log(this.form);
}

ngOnInit() {
  const input = document.querySelector("#phone") as HTMLInputElement;
  intlTelInput(input, {
    utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@23.0.12/build/js/utils.js",
    separateDialCode:true
  });

  this.roles$ = this.roleService.getRoles();
}

register() {
  this.authService.register(this.form.value).subscribe(
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

private passwordMatchValidator(
    control: AbstractControl
  ): { [key: string]: boolean } | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

}
