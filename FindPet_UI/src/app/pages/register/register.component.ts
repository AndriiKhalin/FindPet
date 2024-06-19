import { Component, OnInit, ViewChild, ElementRef, inject } from '@angular/core';
import { RouterOutlet, RouterLink} from "@angular/router";
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, FormGroup, FormControl,FormBuilder, Validators, ReactiveFormsModule} from "@angular/forms";
import {NgClass,NgFor,NgIf} from "@angular/common";
import intlTelInput from 'intl-tel-input';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule,RouterOutlet, RouterLink,MatIconModule,MatInputModule,NgClass,ReactiveFormsModule,NgFor,NgIf],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit{
showPassword: boolean = false;
  form : FormGroup;

  constructor(private formBuilder: FormBuilder){
      this.form = formBuilder.group({
          "fullName": ["", [Validators.required]],
          "email": ["", [ Validators.required, Validators.email]],
          "password": ['', [Validators.required]],
          "userPhone":[ "", [Validators.required]]
      });
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
}

}
