import { Component, Inject, InjectionToken, Input, OnChanges, OnDestroy, Optional, signal } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Subscription } from 'rxjs';

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

export function controlErrorMessage(control: AbstractControl | null, defaults: FieldErrorMessages, overrides: Record<string, string>): string | null {
  if (!control || !control.invalid || !(control.dirty || control.touched) || !control.errors) {
    return null;
  }
  const [key, value] = Object.entries(control.errors)[0];
  if (overrides[key]) {
    return overrides[key];
  }
  const render = defaults[key];
  return render ? render(value) : 'Invalid value.';
}

@Component({
  standalone: false,
  selector: 'app-field-error',
  template: `@if (message()) {<small class="field-error">{{ message() }}</small>}`
})
export class FieldErrorComponent implements OnChanges, OnDestroy {
  @Input({ required: true }) control: AbstractControl | null = null;
  @Input() messages: Record<string, string> = {};

  readonly message = signal<string | null>(null);

  private readonly defaults: FieldErrorMessages;
  private subscription: Subscription | null = null;

  constructor(@Optional() @Inject(FIELD_ERROR_MESSAGES) overrides: FieldErrorMessages | null) {
    this.defaults = { ...DEFAULT_MESSAGES, ...(overrides ?? {}) };
  }

  ngOnChanges(): void {
    this.subscription?.unsubscribe();
    this.subscription = this.control?.events.subscribe(() => this.refresh()) ?? null;
    this.refresh();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private refresh(): void {
    this.message.set(controlErrorMessage(this.control, this.defaults, this.messages));
  }
}
