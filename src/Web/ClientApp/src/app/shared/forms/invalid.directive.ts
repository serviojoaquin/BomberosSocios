import { Directive, ElementRef, Input, OnChanges, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { ControlStateWatcher } from './form-state';

@Directive({
  standalone: false,
  selector: '[appInvalid]'
})
export class InvalidDirective implements OnChanges, OnDestroy {
  @Input('appInvalid') control: AbstractControl | null = null;

  private readonly watcher = new ControlStateWatcher(() => this.refresh());

  constructor(
    private readonly element: ElementRef<HTMLElement>,
    @Optional() private readonly form: FormGroupDirective | null
  ) {}

  ngOnChanges(): void {
    this.watcher.watch(this.control, this.form);
  }

  ngOnDestroy(): void {
    this.watcher.dispose();
  }

  private refresh(): void {
    if (this.watcher.shouldShow(this.control)) {
      this.element.nativeElement.setAttribute('aria-invalid', 'true');
    } else {
      this.element.nativeElement.removeAttribute('aria-invalid');
    }
  }
}
