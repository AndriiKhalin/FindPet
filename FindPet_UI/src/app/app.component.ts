import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { NavbarComponent } from './components/navbar/navbar.component';
import { FootbarComponent } from './components/footbar/footbar.component';
import { HomeComponent } from './pages/home/home.component';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet,MatButtonModule,NavbarComponent,HomeComponent,FootbarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'FindPet_UI';
}
