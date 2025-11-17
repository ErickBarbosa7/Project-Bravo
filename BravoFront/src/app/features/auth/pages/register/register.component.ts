import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';

import { finalize } from 'rxjs';
import { RegisterRequest } from '../../models/auth.model'; 
import { AuthService } from '../../services/auth.service'; 
import { NotyfService } from '../../../../shared/services/notyf.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
    // Ya no se incluyen módulos de PrimeNG
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  // --- Inyección de Dependencias ---
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notyfService = inject(NotyfService);

  // --- Signal para el Estado Local ---
  public isLoading = signal(false);

  // Formulario
  public registerForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    // El rol se asigna por defecto. Ya no es visible para el usuario.
    role: ['Conductor', [Validators.required]] 
  });

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.notyfService.warning('Por favor, completa todos los campos.');
      return;
    }

    this.isLoading.set(true);

    const request: RegisterRequest = this.registerForm.value;

    this.authService.register(request)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: () => {
          this.notyfService.success('Cuenta creada exitosamente. Por favor inicia sesión.');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || 'No se pudo crear la cuenta.';
          this.notyfService.error(msg);
        }
      });
  }
}
