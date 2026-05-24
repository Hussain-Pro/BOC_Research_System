import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'boc-form-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => BocFormFieldComponent),
    multi: true
  }],
  template: `
    <div class="form-group">
      <label class="form-label" *ngIf="label">
        {{ label }}
        <span class="required" *ngIf="required">*</span>
      </label>
      <div class="form-input-wrapper">
        <i *ngIf="icon" class="form-input-icon bi" [ngClass]="icon"></i>
        <input
          [type]="showPassword ? 'text' : type"
          class="form-input"
          [class.has-icon]="icon"
          [class.has-icon-end]="type === 'password'"
          [placeholder]="placeholder"
          [value]="value"
          [attr.dir]="dir"
          [disabled]="disabled"
          (input)="onInput($event)"
          (blur)="onTouched()"
        >
        <button
          *ngIf="type === 'password'"
          type="button"
          class="form-input-eye"
          (click)="showPassword = !showPassword"
          tabindex="-1"
          [attr.aria-label]="showPassword ? 'إخفاء كلمة المرور' : 'إظهار كلمة المرور'"
        >
          <i class="bi" [class.bi-eye]="!showPassword" [class.bi-eye-slash]="showPassword"></i>
        </button>
      </div>
      <span class="form-helper" *ngIf="hint">{{ hint }}</span>
    </div>
  `,
  styles: [`
    :host { display: block; }

    .form-input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .form-input-icon {
      position: absolute;
      right: 12px;
      color: var(--text-muted);
      font-size: 15px;
      pointer-events: none;
      z-index: 1;
    }

    html[dir="ltr"] .form-input-icon {
      right: auto;
      left: 12px;
    }

    .form-input.has-icon {
      padding-right: 36px;
    }

    html[dir="ltr"] .form-input.has-icon {
      padding-right: 12px;
      padding-left: 36px;
    }

    .form-input.has-icon-end {
      padding-left: 36px;
    }

    html[dir="ltr"] .form-input.has-icon-end {
      padding-left: 12px;
      padding-right: 36px;
    }

    .form-input-eye {
      position: absolute;
      left: 10px;
      background: none;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 4px;
      font-size: 15px;
      display: flex;
      align-items: center;
      transition: color var(--motion-fast);
    }

    html[dir="ltr"] .form-input-eye {
      left: auto;
      right: 10px;
    }

    .form-input-eye:hover { color: var(--text-primary); }
  `]
})
export class BocFormFieldComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';
  @Input() placeholder = '';
  @Input() icon = '';
  @Input() hint = '';
  @Input() dir = 'rtl';
  @Input() required = false;

  value = '';
  disabled = false;
  showPassword = false;

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
