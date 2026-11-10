import { Component, Inject, InjectionToken, Input, Optional } from '@angular/core';
import { AbstractControl } from '@angular/forms';

export type FieldErrorMessages = Record<string, (error: any) => string>;

export const FIELD_ERROR_MESSAGES = new InjectionToken<FieldErrorMessages>('FIELD_ERROR_MESSAGES');

const DEFAULT_MESSAGES: FieldErrorMessages = {
  required: () => 'This field is required.',
  maxlength: e => `Maximum length is ${e.requiredLength} characters.`,
  minlength: e => `Minimum length is ${e.requiredLength} characters.`,
  min: e => `Minimum value is ${e.min}.`,
  max: e => `Maximum value is ${e.max}.`,
  email: () => 'Enter a valid email address.',
  pattern: () => 'The value has an invalid format.',
  serverError: e => String(e)
};

@Component({
  standalone: false,
  selector: 'app-field-error',
  template: `@if (message) {<small class="field-error">{{ message }}</small>}`
})
export class FieldErrorComponent {
  @Input({ required: true }) control: AbstractControl | null = null;
  @Input() messages: Record<string, string> = {};

  private readonly defaults: FieldErrorMessages;

  constructor(@Optional() @Inject(FIELD_ERROR_MESSAGES) overrides: FieldErrorMessages | null) {
    this.defaults = { ...DEFAULT_MESSAGES, ...(overrides ?? {}) };
  }

  get message(): string | null {
    const control = this.control;
    if (!control || !control.invalid || !(control.dirty || control.touched) || !control.errors) {
      return null;
    }
    const [key, value] = Object.entries(control.errors)[0];
    if (this.messages[key]) {
      return this.messages[key];
    }
    const render = this.defaults[key];
    return render ? render(value) : 'Invalid value.';
  }
}
