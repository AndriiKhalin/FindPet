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
    imports: [RouterOutlet, MatButtonModule, NavbarComponent, HomeComponent, FootbarComponent],
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
    // Remove token at application startup
    //this.authService.logout();

    // Redirect to the home page
    this.router.navigate(['/']);  // replace '/start' with your home route if needed
  }
}
