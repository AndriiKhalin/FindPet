import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path : "",
    component : HomeComponent,
    pathMatch: 'full'
  },
  {
    path: "login",
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: "register",
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'account/:id',
    loadComponent: () => import('./pages/account/account.component').then(m => m.AccountComponent),
    canActivate: [authGuard]
  },
  {
    path: "searchAnimals",
    loadComponent: () => import('./pages/search-animals/search-animals.component').then(m => m.SearchAnimalsComponent),
    canActivate: [authGuard]
  },
  {
    path: "createAd",
    loadComponent: () => import('./pages/create-ad/create-ad.component').then(m => m.CreateAdComponent),
    canActivate: [authGuard]
  }
];
