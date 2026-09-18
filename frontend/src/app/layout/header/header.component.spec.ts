import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { HeaderComponent } from './header.component';
import { AuthService } from '@core/services/auth.service';

describe('HeaderComponent', () => {
  let fixture: ComponentFixture<HeaderComponent>;
  let component: HeaderComponent;
  let authServiceSpy: {
    isAuthenticated: ReturnType<typeof vi.fn>;
    currentUser: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    authServiceSpy = {
      isAuthenticated: vi.fn().mockReturnValue(false),
      currentUser: vi.fn().mockReturnValue(null),
      logout: vi.fn().mockReturnValue(of(undefined))
    };

    await TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
  });

  it('should render Sign In button when not authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const signInBtn = compiled.querySelector('.btn-signin');
    expect(signInBtn).not.toBeNull();
    expect(signInBtn?.textContent?.trim()).toBe('Sign In');
    expect(compiled.querySelector('.nav-links')).toBeNull();
  });

  it('should render user profile and sign out button when authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.currentUser.mockReturnValue({
      id: 1,
      name: 'Elena Conti',
      email: 'elena.conti@openx.ch',
      role: 'manager'
    });

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.btn-signin')).toBeNull();

    const userName = compiled.querySelector('.user-name');
    expect(userName?.textContent?.trim()).toBe('Elena Conti');

    const userRole = compiled.querySelector('.user-role');
    expect(userRole?.textContent?.trim()).toBe('MANAGER');

    const logoutBtn = compiled.querySelector<HTMLButtonElement>('.btn-logout');
    expect(logoutBtn).not.toBeNull();

    logoutBtn?.click();
    expect(authServiceSpy.logout).toHaveBeenCalled();
  });
});
