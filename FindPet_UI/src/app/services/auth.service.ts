import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { LoginRequest } from '../interfaces/login-request';
import { Observable ,map} from 'rxjs';
import { AuthResponse } from '../interfaces/auth-response';
import { HttpClient } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';
import { RegisterRequest } from '../interfaces/register-request';
import { UserDetail } from '../interfaces/user-detail';
import { LocalStorageService } from './local-storage.service';
import { User } from '../interfaces/user';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  apiUrl: string = environment.apiUrl;
  private tokenKey = 'token';

  constructor(private http:HttpClient,private localStorageService: LocalStorageService) { }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}Account/login`, data)
      .pipe(
        map((response) => {
          if (response.isSuccess) {
            this.localStorageService.setItem(this.tokenKey, response.token);
          }
          return response;
        })
      );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http
    .post<AuthResponse>(`${this.apiUrl}Account/register`, data);
  }

  upload(data:FormData):Observable<string>{
    return this.http.post<string>(`${this.apiUrl}User/uploadImage`,data);
  }

  getUserDetail = () => {
    const token = this.getToken();
    if (!token) return null;
    const decodedToken: any = jwtDecode(token);
    const userDetail = {
      id: decodedToken.nameid,
      fullName: decodedToken.name,
      email: decodedToken.email,
      roles: decodedToken.role || []
    };
    return userDetail;
  };

  getDetail = (): Observable<UserDetail> =>
    this.http.get<UserDetail>(`${this.apiUrl}Account/detail`);

  // getUser = ():Observable<User>=>
  //   this.http.get<UserDetail>(`${this.apiUrl}Account/detail`);

  isLoggedIn = (): boolean => {
    const token = this.getToken();
    if (!token) return false;
    return !this.isTokenExpired();
  };

  private isTokenExpired() {
    const token = this.getToken();
    if (!token) return true;
    const decoded = jwtDecode(token);
    const isTokenExpired = Date.now() >= decoded['exp']! * 1000;
    if (isTokenExpired) this.logout();
    return isTokenExpired;
  }

  logout = (): void => {
    this.localStorageService.removeItem(this.tokenKey);
  };

  getToken = (): string | null =>
    this.localStorageService.getItem(this.tokenKey) || '';

  public createImgPath(serverPath: string): string {
    return `https://localhost:7163/${serverPath}`;
  }
//   getUserPhoto(photoPath: string): Observable<Blob> {
//     return this.http.get(`${this.apiUrl}photo/${photoPath}`, { responseType: 'blob' });
// }
}
