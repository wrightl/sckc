import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReportsService } from '../services/reports.service';
import { Report } from '../models/report';

@Component({
    selector: 'app-reports',
    templateUrl: './reports.component.html',
    styleUrls: ['./reports.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ReportsComponent implements OnInit {

  isReady = false;
  error = false;
  errorDetail = '';
  reports: Report[] = [];

  constructor(private service: ReportsService) { }

  ngOnInit(): void {
    this.service.getReports()
      .subscribe({
        next: (data: Report[]) => {
          this.reports = data;
          this.isReady = true;
        },
        error: (err: HttpErrorResponse) => {
          // The API returns problem details ({ detail, exception, traceId }) for server errors
          console.error('Failed to load trip reports', err);
          const problem = err.error;
          this.errorDetail = [
            `${err.status} ${err.statusText}`.trim(),
            problem?.exception,
            problem?.detail ?? (typeof problem === 'string' ? problem : err.message),
            problem?.traceId && `trace ${problem.traceId}`
          ].filter(Boolean).join(' - ');
          this.error = true;
          this.isReady = true;
        }
      });
  }

}
