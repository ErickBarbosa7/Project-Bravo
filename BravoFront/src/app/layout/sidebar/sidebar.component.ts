import { NavLink } from './../../core/models/layout.model';
import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, Output } from "@angular/core";
import { RouterLink, RouterLinkActive } from "@angular/router";

@Component({
    selector:'app-sidebar',
    standalone:true,
    imports: [CommonModule, RouterLink,RouterLinkActive],
    templateUrl:'./sidebar.component.html',
    styleUrl:'./sidebar.component.scss'
})

export class SidebarComponent{
    // Recibe el email del usuario para mostrarlo
    @Input({required: false}) fullName: string = '';
    @Input({required: true}) userEmail: string = '';
    // Recibe la lista de enlaces
    @Input({required: true}) navLinks: NavLink[] = [];
    //Emite un evento cuando se hace clic en cerrar sesion
    @Output() onLogout = new EventEmitter<void>();

    //Metodo que se llama desde el Html
    logoutClicked(): void{
        this.onLogout.emit();
    }
}