import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink, RouterLinkActive, RouterOutlet } from "@angular/router";
import { AuthService } from "../auth/services/auth.service";
import { NavLink } from "../../core/models/layout.model";
import { SidebarComponent } from "../../layout/sidebar/sidebar.component";

@Component({
    selector: 'app-dashboard-gerente',
    standalone:true,
    imports: [
        CommonModule,
        RouterOutlet,
        RouterLink,
        RouterLinkActive,
        SidebarComponent
    ],
    templateUrl: './dashboard-gerente.component.html',
    styleUrls: ['./dashboard-gerente.component.scss']
})
export class DashboardGerenteComponent {
    authService = inject(AuthService);
    currentUser = this.authService.currentUser;

    // Define los datos para el Sidebar ---
  gerenteLinks: NavLink[] = [
    { path: 'vehiculos', label: 'Vehículos', icon: 'pi pi-car' },
    { path: 'alertas', label: 'Alertas de Servicio', icon: 'pi pi-exclamation-triangle' },
    { path: 'incidentes', label: 'Incidentes', icon: 'pi pi-shield' }
   
  ];
  
    // El componente padre maneja el logout ---
  logout() {
    // Llama al servicio que importaste
    this.authService.logout(); 
  }
}