import { AbstractControl, FormGroupDirective, FormResetEvent } from '@angular/forms';
import { Subscription } from 'rxjs';

export class ControlStateWatcher {
  private subscriptions = new Subscription();
  private submitted = false;

  constructor(private readonly onChange: () => void) {}

  watch(control: AbstractControl | null, form: FormGroupDirective | null): void {
    this.subscriptions.unsubscribe();
    this.subscriptions = new Subscription();
    this.submitted = form?.submitted ?? false;

    if (control) {
      this.subscriptions.add(control.events.subscribe(event => {
        if (event instanceof FormResetEvent) {
          this.submitted = false;
        }
        this.onChange();
      }));
    }

    if (form) {
      this.subscriptions.add(form.ngSubmit.subscribe(() => {
        this.submitted = true;
        this.onChange();
      }));
    }

    this.onChange();
  }

  shouldShow(control: AbstractControl | null): boolean {
    return !!control && control.invalid && (control.dirty || this.submitted);
  }

  dispose(): void {
    this.subscriptions.unsubscribe();
  }
}
