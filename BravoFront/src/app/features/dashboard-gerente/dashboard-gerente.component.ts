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
    // Inyectamos el servicio de autenticacion
    authService = inject(AuthService);
    currentUser = this.authService.currentUser; // señal para usuario actual

    // Definimos los enlaces del sidebar para el gerente
    gerenteLinks: NavLink[] = [
        { path: 'vehiculos', label: 'Vehículos', icon: 'pi pi-car' },
        { path: 'alertas', label: 'Alertas de Servicio', icon: 'pi pi-exclamation-triangle' },
        { path: 'incidentes', label: 'Incidentes', icon: 'pi pi-shield' }
    ];

    // Computed para mostrar el nombre completo en la UI
    get userFullName(): string {
        const user = this.authService.currentUser(); 
        return user ? `${user.firstName} ${user.paternalLastName}` : '';
    }

    // Metodo para cerrar sesion
    logout() {
        this.authService.logout(); // llama al metodo del servicio
    }
}
