import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { CobradoresClient, CobradorDto, CreateCobradorCommand, UpdateCobradorCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

type CobradorEditor = { id?: number, nombre?: string, apellido?: string, telefono?: string, celular?: string, email?: string, comision?: number | null };

@Component({
  standalone: false,
  selector: 'app-cobradores',
  templateUrl: './cobradores.component.html'
})
export class CobradoresComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;

  cobradores = signal<CobradorDto[] | null>(null);
  puedeCrear = this.authService.hasPermission(Permissions.CrearCobradores);
  columns: DataTableColumn[] = [
    { data: 'apellido', title: 'Apellido' },
    { data: 'nombre', title: 'Nombre' },
    { data: 'telefono', title: 'Teléfono' },
    { data: 'celular', title: 'Celular' },
    { data: 'email', title: 'Email' },
    { data: 'comision', title: 'Comisión', className: 'dt-body-right', render: (value: number | null, type: string) => type === 'display' ? (value === null || value === undefined ? '' : `${value} %`) : (value ?? '') } as DataTableColumn
  ];
  actions: DataTableAction[] = [
    ...(this.authService.hasPermission(Permissions.EditarCobradores) ? [{ name: 'edit', label: 'Modificar', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.EliminarCobradores) ? [{ name: 'delete', label: 'Eliminar', icon: 'trash' } as DataTableAction] : [])
  ];
  editor: CobradorEditor = {};
  editorError = signal('');
  selected = signal<CobradorDto | null>(null);

  constructor(
    private client: CobradoresClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.client.getCobradores().subscribe({
      next: result => this.cobradores.set(result.cobradores),
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const cobrador = event.row as CobradorDto;
    if (event.action === 'edit') {
      this.showEditDialog(cobrador);
    } else if (event.action === 'delete') {
      this.confirmDelete(cobrador);
    }
  }

  showNewDialog(): void {
    this.editor = {};
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('apellido')?.focus(), 50);
  }

  showEditDialog(cobrador: CobradorDto): void {
    this.editor = { ...cobrador };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('apellido')?.focus(), 50);
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.editor = {};
    this.editorError.set('');
  }

  save(): void {
    if (this.editor.id) {
      this.update();
    } else {
      this.create();
    }
  }

  confirmDelete(cobrador: CobradorDto): void {
    this.selected.set(cobrador);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteCobrador(id).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.filter(c => c.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.firstError(error, 'No se pudo eliminar el cobrador.'));
      }
    });
  }

  private payload() {
    return {
      nombre: this.editor.nombre,
      apellido: this.editor.apellido,
      telefono: this.editor.telefono || undefined,
      celular: this.editor.celular || undefined,
      email: this.editor.email || undefined,
      comision: this.editor.comision === null || this.editor.comision === undefined || (this.editor.comision as any) === '' ? undefined : Number(this.editor.comision)
    };
  }

  private create(): void {
    const command = this.payload() as CreateCobradorCommand;
    this.client.createCobrador(command).subscribe({
      next: id => {
        const created = { id, ...command } as CobradorDto;
        this.cobradores.update(cs => [...(cs ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'No se pudo guardar el cobrador.'))
    });
  }

  private update(): void {
    const command = { id: this.editor.id, ...this.payload() } as UpdateCobradorCommand;
    this.client.updateCobrador(command.id, command).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.map(c => c.id === command.id ? { ...c, ...command } as CobradorDto : c));
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'No se pudo guardar el cobrador.'))
    });
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
