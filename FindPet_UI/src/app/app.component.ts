import { Component, Inject, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { NavbarComponent } from './components/navbar/navbar.component';
import { FootbarComponent } from './components/footbar/footbar.component';
import { HomeComponent } from './pages/home/home.component';
import { AuthService } from './services/auth.service';
import { isPlatformBrowser } from '@angular/common';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet,MatButtonModule,NavbarComponent,HomeComponent,FootbarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit ,OnDestroy{
  private isBrowser: boolean;

  constructor(
    private authService: AuthService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }
  ngOnDestroy(): void {
    if (this.isBrowser) {
      window.addEventListener('beforeunload', this.clearTokenOnUnload);
    }
  }

  clearTokenOnUnload = (event: BeforeUnloadEvent) => {
    this.authService.logout();
  }
  ngOnInit(): void {
    // Удаляем токен при старте приложения
    //this.authService.logout();

    // Перенаправляем на начальную страницу
    this.router.navigate(['/']);  // замените '/start' на ваш маршрут начальной страницы
  }
}
