import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexTitleSubtitle,
  ApexDataLabels,
  ApexStroke,
  ApexGrid,
  ApexNonAxisChartSeries
} from 'ng-apexcharts';
import { BocDataTableComponent } from '../../../shared/boc-data-table/boc-data-table.component';
import { BocLayoutService } from '../../../services/boc-layout.service';

export type AreaChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  grid: ApexGrid;
  stroke: ApexStroke;
  title: ApexTitleSubtitle;
  colors: string[];
};

export type DonutChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: string[];
  dataLabels: ApexDataLabels;
  title: ApexTitleSubtitle;
  colors: string[];
};

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NgApexchartsModule,
    BocDataTableComponent
  ],
  templateUrl: './analytics-dashboard.component.html',
  styleUrl: './analytics-dashboard.component.scss'
})
export class AnalyticsDashboardComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  metrics = {
    totalReceived: 142,
    underEvaluation: 38,
    completed: 94,
    slaBreaches: 10
  };

  recentReports = [
    { refId: 'BOC-RES-2026-112', title: 'تأثير حقن المياه على مكامن الرميلة', score: '91% (ممتاز)', completedDate: '2026/05/20' },
    { refId: 'BOC-RES-2026-110', title: 'المعالجات الكيميائية للنفط الثقيل', score: '65% (مقبول)', completedDate: '2026/05/18' }
  ];

  reportColumns = ['refId', 'title', 'score', 'completedDate'];
  reportColumnLabels: Record<string, string> = {
    refId: 'الرقم المرجعي',
    title: 'عنوان البحث',
    score: 'التقييم النهائي',
    completedDate: 'تاريخ الإنجاز'
  };

  public areaChartOptions: Partial<AreaChartOptions> | any;
  public donutChartOptions: Partial<DonutChartOptions> | any;

  constructor() {
    this.areaChartOptions = {
      series: [{
        name: 'البحوث المنجزة',
        data: [12, 19, 15, 25, 22, 30, 42]
      }],
      chart: {
        height: 350,
        type: 'area',
        fontFamily: 'Tajawal, sans-serif',
        toolbar: { show: false }
      },
      colors: ['#0F2A38'],
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth' },
      grid: { borderColor: 'rgba(74, 96, 122, 0.15)' },
      xaxis: {
        categories: ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو']
      },
      title: {
        text: 'معدل إنجاز البحوث',
        align: 'right',
        style: { fontSize: '16px', color: '#0F2A38', fontWeight: 'bold' }
      }
    };

    this.donutChartOptions = {
      series: [38, 94, 10],
      chart: {
        type: 'donut',
        height: 350,
        fontFamily: 'Tajawal, sans-serif',
      },
      labels: ['قيد التقييم', 'مكتملة', 'متأخرة'],
      colors: ['#4A607A', '#0F2A38', '#dc3545'],
      title: {
        text: 'حالة البحوث الكلية',
        align: 'right',
        style: { fontSize: '16px', color: '#0F2A38', fontWeight: 'bold' }
      }
    };
  }

  ngOnInit(): void {
    this.layoutService.setPage('لوحة البيانات التنفيذية', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'التحليلات' }
    ]);
  }
}
