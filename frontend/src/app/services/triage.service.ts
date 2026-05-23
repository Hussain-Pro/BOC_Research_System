import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TriagePaper {
  id: string;
  trackingNumber: string;
  title: string;
  abstract: string | null;
  researcherId: string;
  researcherName: string;
  departmentId: string;
  departmentName: string;
  directorateId: string;
  directorateName: string;
  categoryName: string;
  createdAt: string;
}

export interface EligibleEvaluator {
  id: string;
  fullName: string;
  employeeID: string;
  tier: number;
  activeLoad: number;
  lastAssignedDate: string | null;
  specializationName: string;
}

export interface TriageAssignPayload {
  researchId: string;
  mappedById: string;
  evaluatorIds: string[];
  memberIds: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TriageService {
  private apiUrl = 'https://localhost:7139/api/triage';

  constructor(private http: HttpClient) {}

  getTriagePapers(): Observable<TriagePaper[]> {
    return this.http.get<TriagePaper[]>(`${this.apiUrl}/papers`);
  }

  getEligibleEvaluators(researchId: string): Observable<EligibleEvaluator[]> {
    return this.http.get<EligibleEvaluator[]>(`${this.apiUrl}/evaluators?researchId=${researchId}`);
  }

  assignEvaluators(payload: TriageAssignPayload): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/assign`, payload);
  }
}
