import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { CategoriasClient, CategoriaDto, CreateCategoriaCommand, UpdateCategoriaCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

export const MESES = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
const MESES_CORTOS = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

@Component({
  standalone: false,
  selector: 'app-categorias',
  templateUrl: './categorias.component.html'
})
export class CategoriasComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;

  readonly meses = MESES;
  categorias = signal<CategoriaDto[] | null>(null);
  puedeCrear = this.authService.hasPermission(Permissions.CrearCategorias);
  columns: DataTableColumn[] = [
    { data: 'descripcion', title: 'Descripción' },
    ...MESES_CORTOS.map((mes, i) => ({
      data: `importes.${i}`,
      title: mes,
      className: 'dt-body-right',
      render: (value: number, type: string) => type === 'display' ? this.formatImporte(value) : value
    } as DataTableColumn))
  ];
  actions: DataTableAction[] = [
    ...(this.authService.hasPermission(Permissions.EditarCategorias) ? [{ name: 'edit', label: 'Modificar', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.EliminarCategorias) ? [{ name: 'delete', label: 'Eliminar', icon: 'trash' } as DataTableAction] : [])
  ];
  editor: { id?: number, descripcion?: string, importes: number[] } = { importes: this.importesVacios() };
  editorError = signal('');
  selected = signal<CategoriaDto | null>(null);

  constructor(
    private client: CategoriasClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.client.getCategorias().subscribe({
      next: result => this.categorias.set(result.categorias),
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const categoria = event.row as CategoriaDto;
    if (event.action === 'edit') {
      this.showEditDialog(categoria);
    } else if (event.action === 'delete') {
      this.confirmDelete(categoria);
    }
  }

  showNewDialog(): void {
    this.editor = { importes: this.importesVacios() };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('descripcion')?.focus(), 50);
  }

  showEditDialog(categoria: CategoriaDto): void {
    this.editor = { id: categoria.id, descripcion: categoria.descripcion, importes: [...categoria.importes] };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('descripcion')?.focus(), 50);
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.editor = { importes: this.importesVacios() };
    this.editorError.set('');
  }

  copiarEneroATodos(): void {
    this.editor.importes = this.editor.importes.map(() => this.editor.importes[0] ?? 0);
  }

  save(): void {
    if (this.editor.id) {
      this.update();
    } else {
      this.create();
    }
  }

  confirmDelete(categoria: CategoriaDto): void {
    this.selected.set(categoria);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteCategoria(id).subscribe({
      next: () => {
        this.categorias.update(cs => cs!.filter(c => c.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.firstError(error, 'No se pudo eliminar la categoría.'));
      }
    });
  }

  private create(): void {
    const command = { descripcion: this.editor.descripcion, importes: this.editor.importes.map(i => Number(i) || 0) } as CreateCategoriaCommand;
    this.client.createCategoria(command).subscribe({
      next: id => {
        const created = { id, descripcion: command.descripcion, importes: command.importes } as CategoriaDto;
        this.categorias.update(cs => [...(cs ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'No se pudo guardar la categoría.'))
    });
  }

  private update(): void {
    const command = { id: this.editor.id, descripcion: this.editor.descripcion, importes: this.editor.importes.map(i => Number(i) || 0) } as UpdateCategoriaCommand;
    this.client.updateCategoria(command.id, command).subscribe({
      next: () => {
        this.categorias.update(cs => cs!.map(c => c.id === command.id
          ? { ...c, descripcion: command.descripcion, importes: command.importes } as CategoriaDto
          : c));
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'No se pudo guardar la categoría.'))
    });
  }

  private importesVacios(): number[] {
    return MESES.map(() => 0);
  }

  private formatImporte(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 }).format(value ?? 0);
  }

  private firstError(error: any, fallback: string): string {
    try {
      const errors = JSON.parse(error.response).errors;
      const first = errors && Object.values(errors)[0] as string[];
      return first?.[0] ?? fallback;
    } catch {
      return fallback;
    }
  }
}
