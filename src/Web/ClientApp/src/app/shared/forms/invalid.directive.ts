import { Directive, ElementRef, Input, OnChanges, OnDestroy } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Subscription } from 'rxjs';

@Directive({
  standalone: false,
  selector: '[appInvalid]'
})
export class InvalidDirective implements OnChanges, OnDestroy {
  @Input('appInvalid') control: AbstractControl | null = null;

  private subscription: Subscription | null = null;

  constructor(private element: ElementRef<HTMLElement>) {}

  ngOnChanges(): void {
    this.subscription?.unsubscribe();
    this.subscription = this.control?.events.subscribe(() => this.refresh()) ?? null;
    this.refresh();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private refresh(): void {
    const control = this.control;
    const invalid = !!control && (control.dirty || control.touched) && control.invalid;
    if (invalid) {
      this.element.nativeElement.setAttribute('aria-invalid', 'true');
    } else {
      this.element.nativeElement.removeAttribute('aria-invalid');
    }
  }
}
