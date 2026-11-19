import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

type Params = Record<string, string | number | boolean | null | undefined>;

function toHttpParams(params?: Params): HttpParams | undefined {
  if (!params) {
    return undefined;
  }

  let result = new HttpParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== null && value !== undefined) {
      result = result.set(key, String(value));
    }
  }
  return result;
}

@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  get<T>(path: string, params?: Params): Observable<T> {
    return this.http.get<T>(this.baseUrl + path, { params: toHttpParams(params) });
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http.post<T>(this.baseUrl + path, body ?? null);
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(this.baseUrl + path, body);
  }

  patch<T>(path: string, body: unknown): Observable<T> {
    return this.http.patch<T>(this.baseUrl + path, body);
  }
}