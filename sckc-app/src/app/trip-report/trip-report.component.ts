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

  get facts() {
    return [
      { icon: 'place', label: 'Trip', value: this.report.trip },
      { icon: 'flag', label: 'Start', value: this.report.start },
      { icon: 'straighten', label: 'Length', value: this.report.triplength },
      { icon: 'person', label: 'Organiser', value: this.report.organiser },
      { icon: 'school', label: 'Coaches', value: this.report.coaches },
      { icon: 'groups', label: 'Paddlers', value: this.report.paddlers },
      { icon: 'wb_sunny', label: 'Weather', value: this.report.weather },
    ];
  }
}
