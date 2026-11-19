import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly api = inject(ApiClient);

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/api/auth/login', body);
  }

  register(body: RegisterRequest): Observable<RegisterResponse> {
    return this.api.post<RegisterResponse>('/api/auth/register', body);
  }
}