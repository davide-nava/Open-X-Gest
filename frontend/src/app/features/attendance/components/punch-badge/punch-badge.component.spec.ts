import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PunchBadgeComponent } from './punch-badge.component';
import { AttendancePunch } from '../../models/attendance.model';

describe('PunchBadgeComponent', () => {
  let fixture: ComponentFixture<PunchBadgeComponent>;
  let component: PunchBadgeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PunchBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PunchBadgeComponent);
    component = fixture.componentInstance;
  });

  it('should render IN punch with correct styling and labels', () => {
    const punch: AttendancePunch = {
      id: 'p_01',
      time: '08:45',
      type: 'in',
      device: 'Turnstile A',
      location: 'Main HQ'
    };

    fixture.componentRef.setInput('punch', punch);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('.punch-badge');
    expect(badge?.classList.contains('punch-in')).toBe(true);

    const typeEl = compiled.querySelector('.punch-type');
    expect(typeEl?.textContent?.trim()).toBe('IN');

    const timeEl = compiled.querySelector('.punch-time');
    expect(timeEl?.textContent?.trim()).toBe('08:45');

    expect(badge?.getAttribute('title')).toBe('IN at 08:45 | Device: Turnstile A | Location: Main HQ');
    expect(compiled.querySelector('.manual-tag')).toBeNull();
  });

  it('should render OUT punch with manual tag when marked isManual', () => {
    const punch: AttendancePunch = {
      id: 'p_02',
      time: '17:30',
      type: 'out',
      isManual: true,
      note: 'Manager override'
    };

    fixture.componentRef.setInput('punch', punch);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('.punch-badge');
    expect(badge?.classList.contains('punch-out')).toBe(true);

    const manualTag = compiled.querySelector('.manual-tag');
    expect(manualTag).not.toBeNull();
    expect(manualTag?.textContent?.trim()).toBe('*');

    expect(badge?.getAttribute('title')).toContain('(Manual adjustment)');
    expect(badge?.getAttribute('title')).toContain('Note: Manager override');
  });

  it('should render break punches with correct labels', () => {
    const breakStart: AttendancePunch = { id: 'p_03', time: '12:30', type: 'break_start' };
    fixture.componentRef.setInput('punch', breakStart);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.punch-type')?.textContent?.trim()).toBe('BRK-OUT');
    expect(compiled.querySelector('.punch-badge')?.classList.contains('punch-break-start')).toBe(true);

    const breakEnd: AttendancePunch = { id: 'p_04', time: '13:15', type: 'break_end' };
    fixture.componentRef.setInput('punch', breakEnd);
    fixture.detectChanges();

    expect(compiled.querySelector('.punch-type')?.textContent?.trim()).toBe('BRK-IN');
    expect(compiled.querySelector('.punch-badge')?.classList.contains('punch-break-end')).toBe(true);
  });
});
