import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { LoginRequest, RegisterRequest, UserToken } from '../models/auth.model';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { LocalstorageService } from '../../../core/services/localstorage.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  // Inyecciones de Angular
  private http = inject(HttpClient);
  private router = inject(Router);
  private localStorage = inject(LocalstorageService);
  private apiUrl = environment.apiUrl;

  // Token del usuario en un signal para reactividad
  private _token = signal<string | null>(null);
  token = this._token;

  // Usuario actual en signal
  public currentUser = signal<UserToken | null>(null);

  constructor() {
    this.loadUserFromStorage(); // Cargar info del localStorage al iniciar
  }

  // Carga token y usuario del localStorage si existe
  private loadUserFromStorage() {
    const token = this.localStorage.getToken();
    const user = this.localStorage.getUser();

    if (token && user) {
      this._token.set(token);
      this.currentUser.set(user);
    } else {
      this._token.set(null);
      this.currentUser.set(null);
    }
  }

  // LOGIN: devuelve el token y usuario, guarda en signal y localStorage
  login(request: LoginRequest): Observable<UserToken> {
    return this.http.post<UserToken>(`${this.apiUrl}/api/auth/login`, request).pipe(
      tap((res) => {
        this._token.set(res.token);
        this.currentUser.set(res);

        this.localStorage.setToken(res.token);
        this.localStorage.setUser(res);
      })
    );
  }

  // REGISTRO: llama al endpoint de registro
  register(request: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/api/auth/register`, request);
  }

  // LOGOUT: limpia todo y redirige a login
  logout() {
    this._token.set(null);
    this.currentUser.set(null);

    this.localStorage.clearAll();

    this.router.navigate(['/login']);
  }

  // Devuelve el nombre completo del usuario actual
  getFullName(): string {
    const user = this.currentUser();
    return user ? `${user.firstName} ${user.paternalLastName}` : '';
  }

  // Helpers para saber si esta logueado y obtener rol
  isLoggedIn(): boolean {
    return !!this._token();
  }
  // Devuelve el rol
  getUserRole(): string | null {
    return this.currentUser()?.role ?? null;
  }
}
