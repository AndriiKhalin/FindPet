import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { UserDetail } from '../../interfaces/user-detail';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss'
})
export class AccountComponent implements OnInit {
  authService = inject(AuthService);
  accountDetail$!: Observable<UserDetail>;

  ngOnInit() {
    this.accountDetail$ = this.authService.getDetail();
    this.accountDetail$.subscribe({
      next: (data) => {
        console.log("Received user detail:", data);
      },
      error: (error) => {
        console.error("Error fetching user detail:", error);
      }
    });
  }

}
