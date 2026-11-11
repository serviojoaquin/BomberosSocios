import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { SampleItemsClient, SampleItemDto, CreateSampleItemCommand, UpdateSampleItemCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  standalone: false,
  selector: 'app-sample-items',
  templateUrl: './sample-items.component.html'
})
export class SampleItemsComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;

  sampleItems = signal<SampleItemDto[] | null>(null);
  canCreate = this.authService.hasPermission(Permissions.CreateSampleItems);
  columns: DataTableColumn[] = [
    { data: 'description', title: 'sampleItems.descriptionLabel' },
    { data: 'amount', title: 'sampleItems.amount', className: 'dt-body-right', render: (value, type) => type === 'display' ? this.formatAmount(value) : value }
  ];
  actions: DataTableAction[] = [
    ...(this.authService.hasPermission(Permissions.EditSampleItems) ? [{ name: 'edit', label: 'common.edit', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.DeleteSampleItems) ? [{ name: 'delete', label: 'common.delete', icon: 'trash' } as DataTableAction] : [])
  ];
  form = this.fb.group({
    id: [0],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    amount: [0, [Validators.required, Validators.min(0)]]
  });
  generalErrors = signal<string[]>([]);
  selected = signal<SampleItemDto | null>(null);

  constructor(
    private client: SampleItemsClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  get isEditing(): boolean {
    return (this.form.value.id ?? 0) > 0;
  }

  ngOnInit(): void {
    this.client.getSampleItems().subscribe({
      next: result => this.sampleItems.set(result.sampleItems),
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const sampleItem = event.row as SampleItemDto;
    if (event.action === 'edit') {
      this.showEditDialog(sampleItem);
    } else if (event.action === 'delete') {
      this.confirmDelete(sampleItem);
    }
  }

  showNewDialog(): void {
    this.openEditor({ id: 0, description: '', amount: 0 });
  }

  showEditDialog(sampleItem: SampleItemDto): void {
    this.openEditor({ id: sampleItem.id, description: sampleItem.description, amount: sampleItem.amount });
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.form.reset({ id: 0, description: '', amount: 0 });
    this.generalErrors.set([]);
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

  confirmDelete(sampleItem: SampleItemDto): void {
    this.selected.set(sampleItem);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteSampleItem(id).subscribe({
      next: () => {
        this.sampleItems.update(items => items!.filter(i => i.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.formErrors.firstMessage(error, this.translate.instant('sampleItems.deleteError')));
      }
    });
  }

  private openEditor(value: { id: number, description: string, amount: number }): void {
    this.form.reset(value);
    this.generalErrors.set([]);
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('description')?.focus(), 50);
  }

  private create(): void {
    const command = { description: this.form.value.description, amount: Number(this.form.value.amount) } as CreateSampleItemCommand;
    this.client.createSampleItem(command).subscribe({
      next: id => {
        const created = { id, description: command.description, amount: command.amount } as SampleItemDto;
        this.sampleItems.update(items => [...(items ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const command = { id: this.form.value.id, description: this.form.value.description, amount: Number(this.form.value.amount) } as UpdateSampleItemCommand;
    this.client.updateSampleItem(command.id, command).subscribe({
      next: () => {
        this.sampleItems.update(items => items!.map(i => i.id === command.id
          ? { ...i, description: command.description, amount: command.amount } as SampleItemDto
          : i));
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private formatAmount(value: number): string {
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value ?? 0);
  }
}
