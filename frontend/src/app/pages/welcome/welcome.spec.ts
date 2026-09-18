import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Welcome } from './welcome';

describe('Welcome', () => {
  let fixture: ComponentFixture<Welcome>;
  let component: Welcome;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Welcome]
    }).compileComponents();

    fixture = TestBed.createComponent(Welcome);
    component = fixture.componentInstance;
  });

  it('should create the welcome component', () => {
    expect(component).toBeTruthy();
  });

  it('should render welcome text', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('p')?.textContent).toContain('welcome works!');
  });
});
