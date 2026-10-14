import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MEMBERSHIP_INFO } from '../data/membership';

@Component({
    selector: 'app-joining',
    templateUrl: './joining.component.html',
    styleUrls: ['./joining.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class JoiningComponent {
  membershipyear: string;
  info = MEMBERSHIP_INFO;

  constructor() {
    this.membershipyear = new Date().getFullYear().toString();
  }
}
