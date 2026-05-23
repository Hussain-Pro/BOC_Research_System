import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'boc-form-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => BocFormFieldComponent),
    multi: true
  }],
  template: `
    <mat-form-field appearance="outline" class="boc-form-field">
      <mat-label>{{ label }}</mat-label>
      <span matTextPrefix *ngIf="icon" class="me-2 text-primary"><i class="bi" [ngClass]="icon"></i></span>
      <input matInput [type]="type" [placeholder]="placeholder" [value]="value"
             [attr.dir]="dir" [disabled]="disabled" (input)="onInput($event)" (blur)="onTouched()">
      <mat-hint *ngIf="hint">{{ hint }}</mat-hint>
    </mat-form-field>
  `,
  styles: [`
    .boc-form-field { margin-bottom: 0.5rem; }
    :host ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  `]
})
export class BocFormFieldComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';
  @Input() placeholder = '';
  @Input() icon = '';
  @Input() hint = '';
  @Input() dir = 'rtl';

  value = '';
  disabled = false;
  onChange: (v: string) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(value: string): void { this.value = value ?? ''; }
  registerOnChange(fn: (v: string) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean): void { this.disabled = isDisabled; }

  onInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.value = val;
    this.onChange(val);
  }
}
