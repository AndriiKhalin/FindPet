import { Component, inject, OnInit } from '@angular/core';
import { FormsModule, FormGroup, FormControl,FormBuilder, Validators, ReactiveFormsModule} from "@angular/forms";
import { RouterOutlet, RouterLink,Router} from "@angular/router";
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../../services/auth.service';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';


@Component({
    selector: 'app-login',
    imports: [MatCardModule, MatFormFieldModule, MatCheckboxModule, RouterOutlet, MatSnackBarModule, RouterLink, NgClass, ReactiveFormsModule, NgFor, NgIf],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit{
  authService = inject(AuthService);
  showPassword: boolean = false;
  matSnackBar = inject(MatSnackBar);
  form : FormGroup;
  router = inject(Router);

  constructor(private formBuilder: FormBuilder){
      this.form = formBuilder.group({
          "email": ["", [ Validators.required]],
          "password": ['', [Validators.required]]
      });
  }

  login() {
    this.authService.login(this.form.value).subscribe({
      next: (response) => {
        this.matSnackBar.open(response.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.matSnackBar.open(error.error.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
      },
    });
  }


togglePassword(): void {
  this.showPassword =!this.showPassword;
}

ngOnInit(){
}

submit(){
  console.log(this.form);
}
}
