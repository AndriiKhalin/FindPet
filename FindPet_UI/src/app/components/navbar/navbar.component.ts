import { Component, inject, OnInit } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterOutlet, RouterLink, Router} from "@angular/router";
import { MatButtonModule } from '@angular/material/button';
import {MatDividerModule} from '@angular/material/divider';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { UserDetail } from '../../interfaces/user-detail';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [MatToolbarModule,MatIconButton,MatMenuModule,CommonModule,MatIconModule,MatSnackBarModule,RouterLink,RouterOutlet,MatButtonModule,MatDividerModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent implements OnInit{
  authService = inject(AuthService);
  accountDetail$!: Observable<UserDetail>;
  matSnackBar = inject(MatSnackBar);
  router = inject(Router);

  ngOnInit(): void {
    this.accountDetail$ = this.authService.getDetail();
  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

  logout = () => {
    this.authService.logout();
    this.matSnackBar.open('Logout success', 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
    });
    this.router.navigate(['/login']);
  };

  createImgPath = (serverPath: string) => {
    return this.authService.createImgPath(serverPath);
  }
}
