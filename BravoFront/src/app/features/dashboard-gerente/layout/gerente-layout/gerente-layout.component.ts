import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-gerente-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './gerente-layout.component.html',
  styleUrls: ['./gerente-layout.component.scss']
})
export class GerenteLayoutComponent {
  private authService = inject(AuthService);

  // Obtenemos el nombre real del usuario logueado
  public userName = this.authService.getFullName(); 
  
  // Metodo para cerrar sesion
  logout() {
    this.authService.logout();
  }
}