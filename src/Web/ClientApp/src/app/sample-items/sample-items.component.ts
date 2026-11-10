import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { SampleItemsClient, SampleItemDto, CreateSampleItemCommand, UpdateSampleItemCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

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
    { data: 'description', title: 'Description' },
    { data: 'amount', title: 'Amount', className: 'dt-body-right', render: (value, type) => type === 'display' ? this.formatAmount(value) : value }
  ];
  actions: DataTableAction[] = [
    ...(this.authService.hasPermission(Permissions.EditSampleItems) ? [{ name: 'edit', label: 'Edit', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.DeleteSampleItems) ? [{ name: 'delete', label: 'Delete', icon: 'trash' } as DataTableAction] : [])
  ];
  editor: { id?: number, description?: string, amount?: number } = {};
  editorError = signal('');
  selected = signal<SampleItemDto | null>(null);

  constructor(
    private client: SampleItemsClient,
    private authService: AuthService
  ) {}

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
    this.editor = { amount: 0 };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('description')?.focus(), 50);
  }

  showEditDialog(sampleItem: SampleItemDto): void {
    this.editor = { id: sampleItem.id, description: sampleItem.description, amount: sampleItem.amount };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('description')?.focus(), 50);
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
        this.sampleItems.update(cs => cs!.filter(c => c.id !== id));
        this.closeDeleteDialog();
      },
      error: error => console.error(error)
    });
  }

  private create(): void {
    const command = { description: this.editor.description, amount: this.editor.amount ?? 0 } as CreateSampleItemCommand;
    this.client.createSampleItem(command).subscribe({
      next: id => {
        const created = { id, description: command.description, amount: command.amount } as SampleItemDto;
        this.sampleItems.update(cs => [...cs!, created]);
        this.closeEditDialog();
      },
      error: error => this.handleValidationError(error)
    });
  }

  private update(): void {
    const command = { id: this.editor.id, description: this.editor.description, amount: this.editor.amount ?? 0 } as UpdateSampleItemCommand;
    this.client.updateSampleItem(command.id, command).subscribe({
      next: () => {
        this.sampleItems.update(cs => cs!.map(c => c.id === command.id
          ? { ...c, description: command.description, amount: command.amount } as SampleItemDto
          : c));
        this.closeEditDialog();
      },
      error: error => this.handleValidationError(error)
    });
  }

  private formatAmount(value: number): string {
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value ?? 0);
  }

  private handleValidationError(error: any): void {
    try {
      const errors = JSON.parse(error.response).errors;
      const first = errors && Object.values(errors)[0] as string[];
      this.editorError.set(first?.[0] ?? 'Unable to save the sample item.');
    } catch {
      this.editorError.set('Unable to save the sample item.');
    }
    setTimeout(() => document.getElementById('description')?.focus(), 50);
  }
}
