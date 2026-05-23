import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ResearchService {
  private apiUrl = 'https://localhost:7143/api/research';

  constructor(private http: HttpClient) {}

  submitResearch(formData: FormData): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/submit`, formData);
  }
}
