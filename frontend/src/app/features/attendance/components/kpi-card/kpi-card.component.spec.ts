import { ComponentFixture, TestBed } from '@angular/core/testing';
import { KpiCardComponent } from './kpi-card.component';

describe('KpiCardComponent', () => {
  let fixture: ComponentFixture<KpiCardComponent>;
  let component: KpiCardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KpiCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(KpiCardComponent);
    component = fixture.componentInstance;
  });

  it('should render title, value, subtitle and badge', () => {
    fixture.componentRef.setInput('title', 'Total Worked Hours');
    fixture.componentRef.setInput('value', '168.5h');
    fixture.componentRef.setInput('subtitle', 'Target: 160h');
    fixture.componentRef.setInput('badge', '+8.5h');
    fixture.componentRef.setInput('tone', 'success');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    const titleEl = compiled.querySelector('.kpi-title');
    expect(titleEl?.textContent?.trim()).toBe('Total Worked Hours');

    const valueEl = compiled.querySelector('.kpi-value');
    expect(valueEl?.textContent?.trim()).toBe('168.5h');

    const subtitleEl = compiled.querySelector('.kpi-subtitle');
    expect(subtitleEl?.textContent?.trim()).toBe('Target: 160h');

    const badgeEl = compiled.querySelector('.kpi-badge');
    expect(badgeEl?.textContent?.trim()).toBe('+8.5h');

    const card = compiled.querySelector('.kpi-card');
    expect(card?.classList.contains('tone-success')).toBe(true);
    expect(card?.getAttribute('aria-label')).toBe('Total Worked Hours: 168.5h, Target: 160h');
  });

  it('should render default tone when not specified', () => {
    fixture.componentRef.setInput('title', 'Pending Reviews');
    fixture.componentRef.setInput('value', 3);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const card = compiled.querySelector('.kpi-card');
    expect(card?.classList.contains('tone-default')).toBe(true);

    const badgeEl = compiled.querySelector('.kpi-badge');
    expect(badgeEl).toBeNull();
  });
});
