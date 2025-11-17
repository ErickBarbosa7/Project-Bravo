import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NotyfService } from '../../../../shared/services/notyf.service';

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
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notyfService = inject(NotyfService); 

  public isLoading = signal(false);

  public loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onLogin() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.notyfService.warning('Por favor, completa el formulario correctamente.');
      return;
    }

    this.isLoading.set(true);

    const request: { email: string; password: string } = this.loginForm.value;

    this.authService.login(request)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.notyfService.success(`Bienvenido de nuevo, ${response.email}`);

          if (response.role === 'Gerente') {
            this.router.navigate(['/gerente']);
          } else {
            this.router.navigate(['/conductor']);
          }
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || 'Credenciales incorrectas. Intenta de nuevo.';
          this.notyfService.error(msg);
        }
      });
  }
}
