import { Injectable } from '@angular/core';
import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';

@Injectable({
  providedIn: 'root'
})
export class OnboardingService {

  constructor() { }

  startHomeTour(userRole: string, force = false) {
    const tourKey = `boc_tour_home_${userRole}`;
    if (!force && localStorage.getItem(tourKey)) {
      return;
    }

    const driverObj = driver({
      showProgress: true,
      doneBtnText: 'إنهاء',
      nextBtnText: 'التالي',
      prevBtnText: 'السابق',
      popoverClass: 'driverjs-theme-boc',
      steps: this.getStepsForRole(userRole),
      onDestroyStarted: () => {
        if (!driverObj.hasNextStep() || confirm("هل أنت متأكد أنك تريد تخطي الجولة التعريفية؟")) {
          driverObj.destroy();
          localStorage.setItem(tourKey, 'completed');
        }
      },
    });

    // Small delay to ensure DOM is fully rendered
    setTimeout(() => {
      driverObj.drive();
    }, 500);
  }

  private getStepsForRole(role: string): any[] {
    const commonSteps = [
      { element: '.navbar', popover: { title: 'القائمة العلوية', description: 'من هنا يمكنك التنقل بين أقسام النظام الرئيسية، وتعديل ملفك الشخصي.', side: 'bottom', align: 'start' } },
    ];

    if (role === 'Admin') {
      return [
        ...commonSteps,
        { element: '.card-title:contains("الإحصائيات المتقدمة")', popover: { title: 'الإحصائيات', description: 'من هنا يمكنك مراقبة أداء النظام وعرض تقارير اللجان.', side: 'bottom' } },
        { element: '.card-title:contains("فرز الأبحاث")', popover: { title: 'الفرز (Triage)', description: 'كمدير، لديك صلاحيات الدخول لشاشة الفرز ومراقبة عمل اللجان.', side: 'bottom' } }
      ];
    }

    if (role === 'Researcher' || role === '') {
      return [
        ...commonSteps,
        { element: '#home-dashboard .bi-file-earmark-plus', popover: { title: 'تقديم بحث جديد', description: 'ابدأ من هنا لرفع بحثك للجنة.', side: 'bottom' } },
        { element: '#home-dashboard .bi-clock-history', popover: { title: 'تتبع مسار البحث', description: 'تابع حالة بحثك والمراحل التي وصل إليها.', side: 'bottom' } }
      ];
    }

    if (['Chairman', 'Supervisor'].includes(role)) {
      return [
        ...commonSteps,
        { element: '.bi-funnel', popover: { title: 'لوحة فرز الأبحاث', description: 'الواجهة الأهم لك! هنا تصلك الأبحاث الجديدة لتقوم بتوزيعها على المقيمين.', side: 'bottom' } },
        { element: '.bi-clipboard-check', popover: { title: 'مساحة العمل', description: 'إذا قمت بتعيين بحث لنفسك لتقييمه، ستجده هنا.', side: 'bottom' } }
      ];
    }

    if (role === 'Evaluator' || role === 'Member') {
      return [
        ...commonSteps,
        { element: '.bi-clipboard-check', popover: { title: 'مساحة عمل المقيم', description: 'هنا ستجد الأبحاث المسندة إليك لتقييمها. يرجى الانتباه لمؤقت الـ 10 أيام (SLA).', side: 'bottom' } }
      ];
    }

    if (role === 'Secretary') {
      return [
        ...commonSteps,
        { element: '.bi-journal-text', popover: { title: 'إدارة المحاضر', description: 'من هنا تقوم بجدولة اجتماعات اللجنة وصياغة محاضر الاجتماع النهائية.', side: 'bottom' } }
      ];
    }

    return commonSteps;
  }
}
