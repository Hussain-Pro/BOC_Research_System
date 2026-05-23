import { Injectable, signal } from '@angular/core';

export interface BreadcrumbItem {
  label: string;
  route?: string;
}

@Injectable({ providedIn: 'root' })
export class BocLayoutService {
  readonly pageTitle = signal('');
  readonly breadcrumbs = signal<BreadcrumbItem[]>([]);

  setPage(title: string, breadcrumbs: BreadcrumbItem[] = []): void {
    this.pageTitle.set(title);
    this.breadcrumbs.set(breadcrumbs);
    document.title = `${title} | نظام إدارة البحوث - BOC`;
  }

  clearPage(): void {
    this.pageTitle.set('');
    this.breadcrumbs.set([]);
  }
}
