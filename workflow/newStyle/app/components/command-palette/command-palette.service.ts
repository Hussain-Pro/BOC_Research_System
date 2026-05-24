import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommandPaletteService {
  private openSubject = new Subject<void>();
  open$ = this.openSubject.asObservable();

  open(): void {
    this.openSubject.next();
  }
}
