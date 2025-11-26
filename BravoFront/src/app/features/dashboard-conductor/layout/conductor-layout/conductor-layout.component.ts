import { CommonModule } from "@angular/common";
import { Component, computed, inject } from "@angular/core";
import { RouterLink, RouterOutlet } from "@angular/router";
import { AuthService } from "../../../auth/services/auth.service";

@Component({
    selector: 'app-conductor-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, RouterLink],
    templateUrl: './conductor-layout.component.html',
    styleUrls: ['./conductor-layout.component.scss']
})
export class ConductorLayoutComponent {
    // Inyectamos el servicio de autenticacion
    private auth = inject(AuthService);

    // Computed para mostrar el nombre del usuario en la UI
    public userName = computed(() => {
        const user = this.auth.currentUser();
        if (!user) return 'Conductor';  // fallback si no hay usuario
        const nombre = user.firstName || (user as any).FirstName;
        const apellido = user.paternalLastName || (user as any).PaternalLastName;

        // Devuelve "Nombre Apellido" si hay nombre, si no el email
        return nombre ? `${nombre} ${apellido}` : user.email;
    });

    // Metodo para cerrar sesion
    logout() { 
        this.auth.logout(); 
    }
}
