import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-trips',
    templateUrl: './trips.component.html',
    styleUrls: ['./trips.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class TripsComponent {
  constructor() {}
}
