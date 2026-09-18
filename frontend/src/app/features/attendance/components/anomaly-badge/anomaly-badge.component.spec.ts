import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AnomalyBadgeComponent } from './anomaly-badge.component';
import { AttendanceAnomaly } from '../../models/attendance.model';

describe('AnomalyBadgeComponent', () => {
  let fixture: ComponentFixture<AnomalyBadgeComponent>;
  let component: AnomalyBadgeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnomalyBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AnomalyBadgeComponent);
    component = fixture.componentInstance;
  });

  it('should render critical anomaly with code and message in full mode', () => {
    const anomaly: AttendanceAnomaly = {
      code: 'MISSING_CLOCK_OUT',
      message: 'Clock-out missing at end of shift',
      severity: 'critical',
      resolved: false
    };

    fixture.componentRef.setInput('anomaly', anomaly);
    fixture.componentRef.setInput('compact', false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('.anomaly-badge');
    expect(badge?.classList.contains('severity-critical')).toBe(true);

    const codeSpan = compiled.querySelector('.anomaly-code');
    expect(codeSpan?.textContent?.trim()).toBe('MISSING_CLOCK_OUT');

    const msgSpan = compiled.querySelector('.anomaly-message');
    expect(msgSpan?.textContent?.trim()).toBe('Clock-out missing at end of shift');

    expect(badge?.getAttribute('aria-label')).toBe('Anomaly [CRITICAL]: MISSING_CLOCK_OUT - Clock-out missing at end of shift');
  });

  it('should hide message when compact is true', () => {
    const anomaly: AttendanceAnomaly = {
      code: 'LATE_ARRIVAL',
      message: 'Late clock-in beyond threshold',
      severity: 'warning',
      resolved: false
    };

    fixture.componentRef.setInput('anomaly', anomaly);
    fixture.componentRef.setInput('compact', true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('.anomaly-badge');
    expect(badge?.classList.contains('severity-warning')).toBe(true);

    const msgSpan = compiled.querySelector('.anomaly-message');
    expect(msgSpan).toBeNull();
  });

  it('should apply severity-info class for info severity', () => {
    const anomaly: AttendanceAnomaly = {
      code: 'SHORT_BREAK',
      message: 'Informative note on break duration',
      severity: 'info',
      resolved: true
    };

    fixture.componentRef.setInput('anomaly', anomaly);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('.anomaly-badge');
    expect(badge?.classList.contains('severity-info')).toBe(true);
  });
});
