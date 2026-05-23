import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Attendee {
  memberId: string;
  fullName: string;
  roleName: string;
  attended: boolean;
}

export interface MeetingPaper {
  id: string;
  title: string;
  trackingNumber: string;
  researcherName: string;
  finalScore: number | null;
  state: string;
  evaluatorScores: number[];
  chairmanScore: number | null;
}

export interface MeetingVote {
  researchId: string;
  memberId: string;
  memberName: string;
  voteValue: string;
}

export interface MeetingDetails {
  id: string;
  meetingNumber: string;
  title: string;
  scheduledDate: string;
  location: string;
  status: string;
  attendees: Attendee[];
  papers: MeetingPaper[];
  votes: MeetingVote[];
  minutesId: string | null;
  minutesContent: string | null;
  minutesStatus: string;
}

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private apiUrl = 'https://localhost:7143/api/meetings';

  constructor(private http: HttpClient) {}

  getMeetingDetails(meetingId: string): Observable<MeetingDetails> {
    return this.http.get<MeetingDetails>(`${this.apiUrl}/${meetingId}`);
  }

  castVote(meetingId: string, researchId: string, memberId: string, voteValue: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/vote`, {
      meetingId,
      researchId,
      memberId,
      voteValue
    });
  }

  submitGrade(researchId: string, chairmanId: string, minutesId: string, score: number, comments: string | null): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/grade`, {
      researchId,
      chairmanId,
      meetingMinutesId: minutesId,
      score,
      comments
    });
  }

  freezeMinutes(minutesId: string, frozenById: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/freeze-minutes`, {
      minutesId,
      frozenById
    });
  }
}
