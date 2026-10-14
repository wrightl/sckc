import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Person } from '../data/people';

@Component({
    selector: 'app-person',
    templateUrl: './person.component.html',
    styleUrls: ['./person.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PersonComponent {
  @Input()
  person!: Person;

  getMailTo() {
    return `mailto:${this.person.emailAddress}`;
  }
}
