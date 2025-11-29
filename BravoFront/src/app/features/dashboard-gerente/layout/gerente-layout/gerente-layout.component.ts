import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { KpiCardComponent } from '../../components/kpi-card/kpi-card.component';

@Component({
  selector: 'app-gerente-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, KpiCardComponent],
  templateUrl: './gerente-layout.component.html',
  styleUrls: ['./gerente-layout.component.scss']
})
export class GerenteLayoutComponent {
  private authService = inject(AuthService);

  // Hacemos que el nombre sea REACTIVO
  public userName = computed(() => {
    return this.authService.getFullName(); 
  });

  public today = new Date();

  // Metodo para cerrar sesion
  logout() {
    this.authService.logout();
  }
}