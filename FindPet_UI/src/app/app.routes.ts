import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { CreateAdComponent } from './pages/create-ad/create-ad.component';
import { SearchAnimalsComponent } from './pages/search-animals/search-animals.component';
import { AccountComponent } from './pages/account/account.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path : "",
    component : HomeComponent
  },
  {
    path : "login",
    component : LoginComponent
  },
  {
    path : "register",
    component : RegisterComponent
  },
  {
    path: 'account/:id',
    component: AccountComponent,
    canActivate: [authGuard]
  },
  {
    path : "searchAnimals",
    component : SearchAnimalsComponent,
    canActivate: [authGuard]
  },
  {
    path : "createAd",
    component : CreateAdComponent,
    canActivate: [authGuard]
  }
];
