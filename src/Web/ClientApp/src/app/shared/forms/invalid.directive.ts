import { Directive, HostBinding, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Directive({
  standalone: false,
  selector: '[appInvalid]'
})
export class InvalidDirective {
  @Input('appInvalid') control: AbstractControl | null = null;

  @HostBinding('attr.aria-invalid')
  get ariaInvalid(): string | null {
    const control = this.control;
    if (!control || !(control.dirty || control.touched)) {
      return null;
    }
    return control.invalid ? 'true' : null;
  }
}
