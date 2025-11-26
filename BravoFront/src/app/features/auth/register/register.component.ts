import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs';
import { RegisterRequest } from '../models/auth.model'; 
import { AuthService } from '../services/auth.service'; 
import { AlertService } from '../../../shared/services/alert.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  // Inyectamos dependencias
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private alertService = inject(AlertService);

  // Signals para manejar estados locales
  public isLoading = signal(false);      // Loader mientras se procesa el registro
  public showPassword = signal(false);   // Controla visibilidad de la password

  // Formulario reactivo de registro
  public registerForm: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    paternalLastName: ['', [Validators.required, Validators.minLength(2)]],
    maternalLastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Conductor', [Validators.required]] // Valor por defecto
  });

  // Metodo que se ejecuta al enviar el formulario
  onSubmit() {
    // Validamos formulario
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched(); // Marca todos los campos para mostrar errores
      this.alertService.warning('Por favor, completa todos los campos.');
      return;
    }

    this.isLoading.set(true); // Mostramos loader

    const request: RegisterRequest = this.registerForm.value;

    this.authService.register(request)
      .pipe(
        finalize(() => this.isLoading.set(false)) // Ocultamos loader al terminar
      )
      .subscribe({
        next: () => {
          this.alertService.success('Cuenta creada exitosamente. Por favor inicia sesión.');
          this.router.navigate(['/login']); // Redirigimos al login
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || 'No se pudo crear la cuenta.';
          this.alertService.error(msg);
        }
      });
  }

  // Alterna visibilidad de la password
  togglePasswordVisibility() {
    this.showPassword.update(value => !value);
  }
}
