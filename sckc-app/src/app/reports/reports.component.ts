import { Component, OnInit } from '@angular/core';
import { ReportsService } from '../services/reports.service';
import { Report } from '../models/report';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {

  isReady = false;
  reports: Report[] = [];

  constructor(private service: ReportsService) { }

  ngOnInit(): void {
    this.service.getPhotoSets()
      .subscribe((data: Report[]) => {
        this.reports = data;
        this.isReady = true;
      });
  }

}
