import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PetResponse } from '../interfaces/pet-response';
import { PetDetail } from '../interfaces/pet-detail';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PetService {
  apiUrl: string = environment.apiUrl;

  constructor(private http:HttpClient) { }



  update(userId: string, data: PetResponse): Observable<PetDetail> {
    return this.http
    .post<PetDetail>(`${this.apiUrl}Pet?userId=${userId}`, data);
  }

  upload(data:FormData):Observable<string>{
    return this.http.post<string>(`${this.apiUrl}Pet/uploadImage`,data);
  }

  getDetails = (): Observable<PetDetail[]> =>
    this.http.get<PetDetail[]>(`${this.apiUrl}Pet`);

  getDetail = (petId:string): Observable<PetDetail> =>
    this.http.get<PetDetail>(`${this.apiUrl}Pet/${petId}`);
}
