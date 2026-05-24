import { Component, OnInit, OnDestroy, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, Subscription, debounceTime } from 'rxjs';
import { CommandPaletteService } from './command-palette.service';

interface CmdItem {
  id: string;
  title: string;
  subtitle: string;
  icon: string;
  action: () => void;
}

interface CmdGroup {
  title: string;
  items: CmdItem[];
}

@Component({
  selector: 'app-command-palette',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './command-palette.component.html',
  styleUrls: ['./command-palette.component.scss']
})
export class CommandPaletteComponent implements OnInit, OnDestroy {
  @ViewChild('cmdInput') cmdInput!: ElementRef<HTMLInputElement>;

  isOpen = false;
  query = '';
  results: CmdGroup[] = [];
  selectedGroup = 0;
  selectedIndex = 0;

  private search$ = new Subject<string>();
  private subs: Subscription[] = [];
  private allItems: CmdItem[] = [];

  private router = inject(Router);
  private service = inject(CommandPaletteService);

  ngOnInit(): void {
    this.subs.push(
      this.service.open$.subscribe(() => this.open()),
      this.search$.pipe(debounceTime(100)).subscribe(q => this.performSearch(q))
    );

    this.initializeItems();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  private initializeItems(): void {
    this.allItems = [
      { id: 'research-1', title: 'تقييم أثر التضخم على القطاع المصرفي', subtitle: 'بانتظار التقييم • محمود العلي', icon: 'bi bi-file-text', action: () => this.router.navigate(['/research-timeline', '1']) },
      { id: 'research-2', title: 'دراسة السيولة في الأسواق الناشئة', subtitle: 'مسودة • فاطمة الزهراء', icon: 'bi bi-file-text', action: () => this.router.navigate(['/research-timeline', '2']) },
      { id: 'action-1', title: 'تقديم بحث جديد', subtitle: 'افتح نموذج التقديم', icon: 'bi bi-plus-circle', action: () => this.router.navigate(['/submit-research']) },
      { id: 'action-2', title: 'جدولة اجتماع لجنة', subtitle: 'اختر اللجنة والتاريخ', icon: 'bi bi-calendar-plus', action: () => this.router.navigate(['/meeting-scheduler']) },
      { id: 'action-3', title: 'عرض لوحة التحكم', subtitle: 'نظرة عامة على النظام', icon: 'bi bi-grid', action: () => this.router.navigate(['/dashboard']) },
      { id: 'action-4', title: 'سجل الباحث', subtitle: 'تاريخ الأبحاث السابقة', icon: 'bi bi-clock-history', action: () => this.router.navigate(['/researcher-history']) },
    ];
  }

  open(): void {
    this.isOpen = true;
    this.query = '';
    this.results = [];
    this.selectedGroup = 0;
    this.selectedIndex = 0;
    setTimeout(() => this.cmdInput?.nativeElement.focus(), 50);
  }

  close(): void {
    this.isOpen = false;
  }

  closeOnBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.close();
  }

  onSearch(): void {
    this.search$.next(this.query);
  }

  private performSearch(q: string): void {
    if (!q.trim()) {
      this.results = [];
      return;
    }

    const filtered = this.allItems.filter(item =>
      item.title.toLowerCase().includes(q.toLowerCase()) ||
      item.subtitle.toLowerCase().includes(q.toLowerCase())
    );

    const researchItems = filtered.filter(i => i.id.startsWith('research'));
    const actionItems = filtered.filter(i => i.id.startsWith('action'));

    this.results = [];
    if (researchItems.length) {
      this.results.push({ title: 'CMD_PALETTE.RESEARCH', items: researchItems });
    }
    if (actionItems.length) {
      this.results.push({ title: 'CMD_PALETTE.ACTIONS', items: actionItems });
    }

    this.selectedGroup = 0;
    this.selectedIndex = 0;
  }

  onKeydown(event: KeyboardEvent): void {
    const totalGroups = this.results.length;
    if (totalGroups === 0) return;

    const currentGroup = this.results[this.selectedGroup];
    const totalItems = currentGroup.items.length;

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (this.selectedIndex < totalItems - 1) {
          this.selectedIndex++;
        } else if (this.selectedGroup < totalGroups - 1) {
          this.selectedGroup++;
          this.selectedIndex = 0;
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (this.selectedIndex > 0) {
          this.selectedIndex--;
        } else if (this.selectedGroup > 0) {
          this.selectedGroup--;
          this.selectedIndex = this.results[this.selectedGroup].items.length - 1;
        }
        break;
      case 'Enter':
        event.preventDefault();
        if (currentGroup?.items[this.selectedIndex]) {
          this.execute(currentGroup.items[this.selectedIndex]);
        }
        break;
      case 'Escape':
        this.close();
        break;
    }
  }

  selectItem(group: CmdGroup, index: number): void {
    this.selectedGroup = this.results.indexOf(group);
    this.selectedIndex = index;
  }

  isSelected(group: CmdGroup, index: number): boolean {
    return this.results.indexOf(group) === this.selectedGroup && index === this.selectedIndex;
  }

  execute(item: CmdItem): void {
    item.action();
    this.close();
  }
}
