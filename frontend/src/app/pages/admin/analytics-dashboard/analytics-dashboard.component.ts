import { Component, OnInit } from '@angular/core';
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
  imports: [CommonModule, RouterModule, NgApexchartsModule],
  templateUrl: './analytics-dashboard.component.html',
  styleUrl: './analytics-dashboard.component.scss'
})
export class AnalyticsDashboardComponent implements OnInit {

  metrics = {
    totalReceived: 142,
    underEvaluation: 38,
    completed: 94,
    slaBreaches: 10
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
      colors: ['#0d6efd'],
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth' },
      xaxis: {
        categories: ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو']
      },
      title: {
        text: 'معدل إنجاز البحوث',
        align: 'right',
        style: { fontSize: '16px', color: '#263238', fontWeight: 'bold' }
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
      colors: ['#ffc107', '#198754', '#dc3545'],
      title: {
        text: 'حالة البحوث الكلية',
        align: 'right',
        style: { fontSize: '16px', color: '#263238', fontWeight: 'bold' }
      }
    };
  }

  ngOnInit(): void {
  }
}
