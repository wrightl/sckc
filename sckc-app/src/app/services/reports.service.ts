import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Report } from '../models/report';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {

  constructor(private http: HttpClient) { }

  getReports() {
    return this.http.get<Report[]>(`${environment.baseApiUrl}reports`);
  }

  // The report is held back until someone accepts it from the review email; resolves to a thank-you message
  submitReport(submission: Record<string, string>) {
    return this.http.post<string>(`${environment.baseApiUrl}reports`, submission);
  }
}
