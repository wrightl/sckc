import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-club-history',
    templateUrl: './club-history.component.html',
    styleUrls: ['./club-history.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ClubHistoryComponent {
  constructor() {}
}
