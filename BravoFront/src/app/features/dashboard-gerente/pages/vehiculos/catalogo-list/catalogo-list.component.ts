import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';
import { CatalogoVehiculo } from '../../../../../core/models/vehiculo.model';
import { AlertService } from '../../../../../shared/services/alert.service';

@Component({
  selector: 'app-catalogo-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './catalogo-list.component.html',
  styleUrls: ['./catalogo-list.component.scss']
})
export class CatalogoListComponent implements OnInit {
  private vehiculoService = inject(VehiculoService);
  private alertService = inject(AlertService);

  public catalogo = signal<CatalogoVehiculo[]>([]);
  public categorias = signal<any[]>([]);
  public isLoading = signal(true);
  
  // Modals state
  public showModal = signal(false);
  public isEdit = signal(false);
  public currentItem = signal<Partial<CatalogoVehiculo>>({});

  public showDeleteModal = signal(false);
  public itemToDelete = signal<number | null>(null);

  // Nueva categoria state
  public isCreatingCategoria = signal(false);
  public nuevaCategoriaNombre = signal('');

  ngOnInit() {
    this.cargarCatalogo();
    this.cargarCategorias();
  }

  cargarCatalogo() {
    this.isLoading.set(true);
    this.vehiculoService.getCatalogo().subscribe({
      next: (data) => {
        this.catalogo.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.alertService.error('No se pudo cargar el catálogo.');
        this.isLoading.set(false);
      }
    });
  }

  cargarCategorias() {
    this.vehiculoService.getCategoriasVehiculo().subscribe({
      next: (data) => this.categorias.set(data),
      error: () => console.error('Error al cargar categorias')
    });
  }

  abrirModalNuevo() {
    this.isEdit.set(false);
    this.isCreatingCategoria.set(false);
    this.currentItem.set({
      marca: '', modelo: '', anio: new Date().getFullYear(),
      categoria: this.categorias().length > 0 ? this.categorias()[0].nombre : 'General', 
      intervaloServicioKm: 10000, fotoUrl: ''
    });
    this.showModal.set(true);
  }

  abrirModalEditar(item: CatalogoVehiculo) {
    this.isEdit.set(true);
    this.isCreatingCategoria.set(false);
    this.currentItem.set({ ...item });
    this.showModal.set(true);
  }

  cerrarModal() {
    this.showModal.set(false);
  }

  toggleCrearCategoria() {
    this.isCreatingCategoria.set(!this.isCreatingCategoria());
    this.nuevaCategoriaNombre.set('');
  }

  guardarCategoria() {
    const nombre = this.nuevaCategoriaNombre().trim();
    if (!nombre) return;
    
    this.vehiculoService.crearCategoriaVehiculo({ nombre }).subscribe({
      next: (cat) => {
        this.categorias.update(c => [...c, cat]);
        this.currentItem.update(item => ({...item, categoria: cat.nombre}));
        this.isCreatingCategoria.set(false);
        this.alertService.success('Categoría creada');
      },
      error: () => this.alertService.error('Error al crear categoría')
    });
  }

  guardar() {
    const item = this.currentItem();
    if (!item.marca || !item.modelo) {
      this.alertService.warning('Marca y Modelo son obligatorios.');
      return;
    }

    if (this.isEdit() && item.id) {
      this.vehiculoService.editarEnCatalogo(item.id, item).subscribe({
        next: () => {
          this.alertService.success('Plantilla actualizada.');
          this.cerrarModal();
          this.cargarCatalogo();
        },
        error: () => this.alertService.error('Error al actualizar.')
      });
    } else {
      this.vehiculoService.crearEnCatalogo(item).subscribe({
        next: () => {
          this.alertService.success('Plantilla guardada.');
          this.cerrarModal();
          this.cargarCatalogo();
        },
        error: () => this.alertService.error('Error al guardar la plantilla.')
      });
    }
  }

  confirmarEliminar(id: number) {
    this.itemToDelete.set(id);
    this.showDeleteModal.set(true);
  }

  cerrarDeleteModal() {
    this.showDeleteModal.set(false);
    this.itemToDelete.set(null);
  }

  eliminarDefinitivo() {
    const id = this.itemToDelete();
    if (id) {
      this.vehiculoService.eliminarDelCatalogo(id).subscribe({
        next: () => {
          this.alertService.success('Plantilla eliminada.');
          this.cerrarDeleteModal();
          this.cargarCatalogo();
        },
        error: () => {
          this.alertService.error('Error al eliminar.');
          this.cerrarDeleteModal();
        }
      });
    }
  }
}
