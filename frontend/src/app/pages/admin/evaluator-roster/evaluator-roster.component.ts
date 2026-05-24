import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../services/toast.service';
import { BocLayoutService } from '../../../services/boc-layout.service';

@Component({
  selector: 'app-evaluator-roster',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule],
  templateUrl: './evaluator-roster.component.html',
  styleUrls: ['./evaluator-roster.component.scss']
})
export class EvaluatorRosterComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  searchQuery = '';
  selectedSpecialty = '';

  roster = [
    {
      name: 'د. حسن إبراهيم (م. أقدم)',
      specialty: 'هندسة مكامن',
      tier: 'Tier 1',
      activeLoad: 0,
      completed: 0,
      slaViolations: 0
    },
    {
      name: 'د. فاطمة الزهراء (خبير)',
      specialty: 'معالجة كيميائية',
      tier: 'Tier 2',
      activeLoad: 0,
      completed: 12,
      slaViolations: 0
    },
    {
      name: 'أ. خالد منصور (ر. مهندسين)',
      specialty: 'هندسة حفر',
      tier: 'Tier 3',
      activeLoad: 3,
      completed: 45,
      slaViolations: 1
    }
  ];

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
    this.layoutService.setPage('سجل المقيمين', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'سجل المقيمين' }
    ]);
  }

  getTierClass(tier: string) {
    if (tier.includes('1')) return 'bg-success';
    if (tier.includes('2')) return 'bg-primary';
    return 'bg-warning text-dark';
  }

  get filteredRoster() {
    return this.roster.filter(r => {
      const matchSearch = r.name.includes(this.searchQuery) || r.specialty.includes(this.searchQuery);
      const matchSpecialty = this.selectedSpecialty ? r.specialty.includes(this.selectedSpecialty) : true;
      return matchSearch && matchSpecialty;
    });
  }

  get totalActiveLoad(): number {
    return this.roster.reduce((sum, r) => sum + r.activeLoad, 0);
  }

  get totalCompleted(): number {
    return this.roster.reduce((sum, r) => sum + r.completed, 0);
  }
}

