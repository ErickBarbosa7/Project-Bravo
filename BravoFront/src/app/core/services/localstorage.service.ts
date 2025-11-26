import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalstorageService {

  // Claves locales
  private TOKEN_KEY = 'auth_token';
  private USER_KEY = 'auth_user';

  // Guardar token
  setToken(token: string) {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  // Obtener token
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Borrar token
  clearToken() {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // Guardar usuario
  setUser(user: any) {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  // Obtener usuario
  getUser() {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  // Borrar usuario
  clearUser() {
    localStorage.removeItem(this.USER_KEY);
  }

  // Limpiar todo
  clearAll() {
    localStorage.clear();
  }
}
