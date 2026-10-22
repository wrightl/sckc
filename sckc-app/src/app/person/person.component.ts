import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Person } from '../data/people';

// Bios longer than this start collapsed behind a "Read more" toggle
const LONG_BIO_LENGTH = 350;

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

  expanded = false;

  get isLong() {
    return this.person.longText.length > LONG_BIO_LENGTH;
  }
}
