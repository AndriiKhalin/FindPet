import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

  constructor() { }

  setItem(key: string, value: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(key, value);
    }
  }

  getItem(key: string): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(key);
    } else {
      return null;
    }
  }

  removeItem(key: string): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(key);
    }
  }

  clear(): void {
    if (typeof window !== 'undefined') {
      localStorage.clear();
    }
  }
}
