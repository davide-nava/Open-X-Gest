import { Routes } from '@angular/router';

export const ATTENDANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/monthly-report/monthly-report.component').then(
        (m) => m.MonthlyReportComponent
      ),
    title: 'Monthly Attendance Report | Open-X-Gest'
  },
  {
    path: 'monthly-report',
    redirectTo: '',
    pathMatch: 'full'
  }
];
