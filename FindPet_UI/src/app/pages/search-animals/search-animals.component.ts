import { Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PetService } from '../../services/pet.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink, RouterOutlet} from '@angular/router';
import { PetDetail } from '../../interfaces/pet-detail';
import { Observable } from 'rxjs';
import {NgClass,NgFor,NgIf,AsyncPipe,CommonModule} from "@angular/common";
import { FormsModule, FormGroup, FormControl,FormBuilder, Validators, ReactiveFormsModule, AbstractControl} from "@angular/forms";


@Component({
  selector: 'app-search-animals',
  standalone: true,
  imports: [NgClass,ReactiveFormsModule,CommonModule,FormsModule,NgFor,NgIf,AsyncPipe,RouterLink,RouterOutlet],
  templateUrl: './search-animals.component.html',
  styleUrl: './search-animals.component.scss'
})
export class SearchAnimalsComponent implements OnInit{
  petService=inject(PetService);
  matSnackbar = inject(MatSnackBar);
  router = inject(Router);
  pets$!: Observable<PetDetail[]>;

constructor(private http: HttpClient ) {

}
  ngOnInit(): void {
    this.pets$=this.petService.getDetails();
  }

  // getPetsDetails() {
  //   this.petService.getDetails()
  //     .subscribe((data) => {
  //       console.log(data);
  //       this.pets = data;
  //     });
  // }
}
