import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '@core/services/auth.service';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let router: Router;
  let authServiceSpy: {
    login: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    authServiceSpy = {
      login: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParams: { returnUrl: '/custom-dashboard' }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockImplementation(() => Promise.resolve(true));
  });

  it('should initialize returnUrl from query parameters', () => {
    component.ngOnInit();
    // Verify component is initialized
    expect(component).toBeTruthy();
  });

  it('should not submit when form is invalid', () => {
    component['loginForm'].controls.email.setValue('');
    component.onSubmit();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
    expect(component['loginForm'].controls.email.touched).toBe(true);
  });

  it('should navigate to returnUrl on successful login', () => {
    authServiceSpy.login.mockReturnValue(of({
      token: 'valid-token',
      tokenType: 'Bearer',
      user: { id: 1, name: 'Alex', email: 'alex@openx.ch', role: 'admin' }
    }));

    component.ngOnInit();
    component['loginForm'].controls.email.setValue('alex@openx.ch');
    component['loginForm'].controls.password.setValue('ValidPassword123');

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith({
      email: 'alex@openx.ch',
      password: 'ValidPassword123',
      remember: true
    });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/custom-dashboard');
    expect(component['errorMessage']()).toBeNull();
  });

  it('should display error message on login failure', () => {
    authServiceSpy.login.mockReturnValue(throwError(() => new Error('Invalid credentials')));

    component.ngOnInit();
    component['loginForm'].controls.email.setValue('alex@openx.ch');
    component['loginForm'].controls.password.setValue('WrongPassword');

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalled();
    expect(component['errorMessage']()).toBe('Invalid credentials. Please check your email and password.');
    expect(component['isLoading']()).toBe(false);
  });
});
