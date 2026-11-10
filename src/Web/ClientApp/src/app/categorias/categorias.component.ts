import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, Validators } from '@angular/forms';
import { CategoriasClient, CategoriaDto, CreateCategoriaCommand, UpdateCategoriaCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';

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
  form = this.fb.group({
    id: [0],
    descripcion: ['', [Validators.required, Validators.maxLength(200)]],
    importes: this.fb.array(MESES.map(() => this.fb.control<number>(0, [Validators.required, Validators.min(0)])))
  });
  generalErrors = signal<string[]>([]);
  selected = signal<CategoriaDto | null>(null);

  constructor(
    private client: CategoriasClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService
  ) {}

  get isEditing(): boolean {
    return (this.form.value.id ?? 0) > 0;
  }

  get importes(): FormArray<FormControl<number | null>> {
    return this.form.get('importes') as FormArray<FormControl<number | null>>;
  }

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
    this.openEditor({ id: 0, descripcion: '', importes: MESES.map(() => 0) });
  }

  showEditDialog(categoria: CategoriaDto): void {
    this.openEditor({ id: categoria.id, descripcion: categoria.descripcion, importes: [...categoria.importes] });
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.generalErrors.set([]);
  }

  copiarEneroATodos(): void {
    const enero = Number(this.importes.at(0).value) || 0;
    this.importes.controls.forEach(c => c.setValue(enero));
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }
    if (this.isEditing) {
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
        alert(this.formErrors.firstMessage(error, 'No se pudo eliminar la categoría.'));
      }
    });
  }

  private openEditor(value: { id: number, descripcion: string, importes: number[] }): void {
    this.form.reset(value);
    this.generalErrors.set([]);
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('descripcion')?.focus(), 50);
  }

  private payload() {
    return {
      descripcion: this.form.value.descripcion,
      importes: this.importes.controls.map(c => Number(c.value) || 0)
    };
  }

  private create(): void {
    const command = this.payload() as CreateCategoriaCommand;
    this.client.createCategoria(command).subscribe({
      next: id => {
        const created = { id, descripcion: command.descripcion, importes: command.importes } as CategoriaDto;
        this.categorias.update(cs => [...(cs ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const command = { id: this.form.value.id, ...this.payload() } as UpdateCategoriaCommand;
    this.client.updateCategoria(command.id, command).subscribe({
      next: () => {
        this.categorias.update(cs => cs!.map(c => c.id === command.id
          ? { ...c, descripcion: command.descripcion, importes: command.importes } as CategoriaDto
          : c));
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private formatImporte(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 }).format(value ?? 0);
  }
}
