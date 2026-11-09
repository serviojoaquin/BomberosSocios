import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { CategoriesClient, CategoryDto, CreateCategoryCommand, UpdateCategoryCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'app-categories',
  templateUrl: './categories.component.html'
})
export class CategoriesComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;

  categories = signal<CategoryDto[] | null>(null);
  canCreate$ = this.authService.hasPermission$(Permissions.CreateCategories);
  canEdit$ = this.authService.hasPermission$(Permissions.EditCategories);
  canDelete$ = this.authService.hasPermission$(Permissions.DeleteCategories);
  canAct$ = combineLatest([this.canEdit$, this.canDelete$]).pipe(map(([e, d]) => e || d));
  editor: { id?: number, description?: string, amount?: number } = {};
  editorError = signal('');
  selected = signal<CategoryDto | null>(null);

  constructor(
    private client: CategoriesClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.client.getCategories().subscribe({
      next: result => this.categories.set(result.categories),
      error: error => console.error(error)
    });
  }

  showNewDialog(): void {
    this.editor = { amount: 0 };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('description')?.focus(), 50);
  }

  showEditDialog(category: CategoryDto): void {
    this.editor = { id: category.id, description: category.description, amount: category.amount };
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

  confirmDelete(category: CategoryDto): void {
    this.selected.set(category);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteCategory(id).subscribe({
      next: () => {
        this.categories.update(cs => cs!.filter(c => c.id !== id));
        this.closeDeleteDialog();
      },
      error: error => console.error(error)
    });
  }

  private create(): void {
    const command = { description: this.editor.description, amount: this.editor.amount ?? 0 } as CreateCategoryCommand;
    this.client.createCategory(command).subscribe({
      next: id => {
        const created = { id, description: command.description, amount: command.amount } as CategoryDto;
        this.categories.update(cs => this.sorted([...cs!, created]));
        this.closeEditDialog();
      },
      error: error => this.handleValidationError(error)
    });
  }

  private update(): void {
    const command = { id: this.editor.id, description: this.editor.description, amount: this.editor.amount ?? 0 } as UpdateCategoryCommand;
    this.client.updateCategory(command.id, command).subscribe({
      next: () => {
        this.categories.update(cs => this.sorted(cs!.map(c => c.id === command.id
          ? { ...c, description: command.description, amount: command.amount } as CategoryDto
          : c)));
        this.closeEditDialog();
      },
      error: error => this.handleValidationError(error)
    });
  }

  private sorted(categories: CategoryDto[]): CategoryDto[] {
    return [...categories].sort((a, b) => a.description.localeCompare(b.description));
  }

  private handleValidationError(error: any): void {
    try {
      const errors = JSON.parse(error.response).errors;
      const first = errors && Object.values(errors)[0] as string[];
      this.editorError.set(first?.[0] ?? 'Unable to save the category.');
    } catch {
      this.editorError.set('Unable to save the category.');
    }
    setTimeout(() => document.getElementById('description')?.focus(), 50);
  }
}
