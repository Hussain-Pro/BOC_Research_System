import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BocTimelineStep {
  title: string;
  description: string;
  date?: Date | string;
  isCompleted: boolean;
  isActive: boolean;
  requiresAction?: boolean;
}

@Component({
  selector: 'boc-vertical-timeline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './boc-vertical-timeline.component.html',
  styleUrl: './boc-vertical-timeline.component.scss'
})
export class BocVerticalTimelineComponent {
  @Input() steps: BocTimelineStep[] = [];

  readonly expandedIndex = signal<number | null>(null);

  toggleStep(index: number): void {
    this.expandedIndex.update(current => (current === index ? null : index));
  }

  isExpanded(index: number): boolean {
    const expanded = this.expandedIndex();
    return expanded === index || (expanded === null && this.steps[index]?.isActive);
  }

  stepState(step: BocTimelineStep): 'completed' | 'active' | 'pending' {
    if (step.isCompleted) return 'completed';
    if (step.isActive) return 'active';
    return 'pending';
  }
}
