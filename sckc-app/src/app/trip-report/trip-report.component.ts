import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Report } from '../models/report';

@Component({
    selector: 'app-trip-report',
    templateUrl: './trip-report.component.html',
    styleUrls: ['./trip-report.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class TripReportComponent {

  @Input() report!: Report;


}
