import { Component, inject, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { OnboardingService } from '../../services/onboarding.service';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexTitleSubtitle,
  ApexDataLabels,
  ApexStroke,
  ApexGrid
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  grid: ApexGrid;
  stroke: ApexStroke;
  title: ApexTitleSubtitle;
  colors: string[];
};

@Component({
  selector: 'app-home-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, NgApexchartsModule],
  templateUrl: './home-dashboard.component.html',
  styleUrls: ['./home-dashboard.component.scss']
})
export class HomeDashboardComponent implements OnInit, AfterViewInit {
  authService = inject(AuthService);
  onboardingService = inject(OnboardingService);
  userRole: string = '';
  userName: string = '';
  public submissionsChartOptions: Partial<ChartOptions> | any;

  constructor() {
    this.submissionsChartOptions = {
      series: [
        {
          name: "البحوث المقدمة",
          data: [1, 2, 4, 3, 5, 8, 12]
        }
      ],
      chart: {
        height: 300,
        type: "area",
        fontFamily: 'Tajawal, sans-serif',
        toolbar: { show: false }
      },
      colors: ['#0f2a38'],
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: "smooth"
      },
      xaxis: {
        categories: ["يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو"]
      },
      title: {
        text: "نشاط تقديم البحوث",
        align: "right",
        style: {
          fontSize:  '16px',
          fontWeight:  'bold',
          color:  '#263238'
        },
      }
    };
  }

  ngOnInit() {
    this.userRole = this.authService.getRole();
    const user = this.authService.currentUser();
    this.userName = user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'مستخدم النظام';
  }

  ngAfterViewInit() {
    this.onboardingService.startHomeTour(this.userRole);
  }
}
