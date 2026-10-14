import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-activities',
    templateUrl: './activities.component.html',
    styleUrls: ['./activities.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ActivitiesComponent {
  constructor() {}
}
