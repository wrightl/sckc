import { Component, Input } from '@angular/core';
import { Person } from '../data/people';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss'],
})
export class PersonComponent {
  @Input()
  person!: Person;
}
