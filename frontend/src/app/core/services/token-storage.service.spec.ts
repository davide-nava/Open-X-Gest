import { TestBed } from '@angular/core/testing';
import { PLATFORM_ID } from '@angular/core';
import { TokenStorageService } from './token-storage.service';
import { User } from '../models/user.model';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TokenStorageService,
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    });

    service = TestBed.inject(TokenStorageService);
    service.clearAll();
  });

  afterEach(() => {
    service.clearAll();
  });

  it('should store, retrieve and remove authentication token', () => {
    expect(service.getToken()).toBeNull();

    service.setToken('test-jwt-token');
    expect(service.getToken()).toBe('test-jwt-token');

    service.removeToken();
    expect(service.getToken()).toBeNull();
  });

  it('should store, retrieve and remove user profile', () => {
    const user: User = {
      id: 42,
      name: 'Test Employee',
      email: 'employee@open-x-gest.internal',
      role: 'employee',
      department: 'Engineering'
    };

    expect(service.getUser()).toBeNull();

    service.setUser(user);
    expect(service.getUser()).toEqual(user);

    service.removeUser();
    expect(service.getUser()).toBeNull();
  });

  it('should handle corrupted user JSON gracefully without throwing', () => {
    (service as unknown as { memoryStorage: Map<string, string> }).memoryStorage.set('openx_gest_auth_user', 'invalid-json-content{{{');

    expect(service.getUser()).toBeNull();
  });

  it('clearAll should clear both token and user profile', () => {
    service.setToken('token-to-clear');
    service.setUser({
      id: 1,
      name: 'Admin',
      email: 'admin@openx.ch',
      role: 'admin'
    });

    service.clearAll();

    expect(service.getToken()).toBeNull();
    expect(service.getUser()).toBeNull();
  });
});
