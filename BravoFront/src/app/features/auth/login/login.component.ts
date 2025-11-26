import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthService } from '../services/auth.service';
// Servicio para mostrar alertas y notificaciones
import { AlertService } from '../../../shared/services/alert.service'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterLink
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  // Inyectamos dependencias con inject()
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private alertService = inject(AlertService); 

  // Controla el estado de carga del login
  public isLoading = signal(false);

  // Controla si la contrasena se muestra o no
  public showPassword = signal(false);
  
  // Formulario reactivo para login
  public loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]], // Email obligatorio y valido
    password: ['', [Validators.required]]                 // Password obligatorio
  });

  // Metodo que se ejecuta al enviar el formulario
  onLogin() {
    // Validamos el formulario
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched(); // Marca todos los campos para mostrar errores
      this.alertService.warning('Por favor, completa el formulario correctamente.');
      return;
    }

    this.isLoading.set(true); // Mostramos loader

    // Creamos el request tipado
    const request = { 
        email: this.loginForm.value.email, 
        password: this.loginForm.value.password 
    };

    this.authService.login(request)
      .pipe(
        finalize(() => this.isLoading.set(false)) // Ocultamos loader al terminar
      )
      .subscribe({
        next: (response) => {
          const nombre = response.firstName || response.email;
          this.alertService.success(`Bienvenido de nuevo, ${nombre}`);

          // Redirigimos segun el rol
          if (response.role === 'Gerente') {
            this.router.navigate(['/gerente']);
          } else {
            this.router.navigate(['/conductor']);
          }
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || 'Credenciales incorrectas. Intenta de nuevo.';
          this.alertService.error(msg);
        }
      });
  }

  // Metodo para alternar visibilidad de la contrasena
  togglePasswordVisibility() {
    this.showPassword.update(value => !value);
  }
}
