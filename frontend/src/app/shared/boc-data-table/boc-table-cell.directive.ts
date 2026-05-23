import { Directive, Input, TemplateRef } from '@angular/core';

@Directive({
  selector: '[bocTableCellDef]',
  standalone: true
})
export class BocTableCellDefDirective {
  @Input('bocTableCellDef') column = '';

  constructor(public templateRef: TemplateRef<{ $implicit: unknown }>) {}
}
