import { Component, ChangeDetectionStrategy } from '@angular/core';
import { PEOPLE } from '../data/people';

@Component({
    selector: 'app-people',
    templateUrl: './people.component.html',
    styleUrls: ['./people.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PeopleComponent {
  people = PEOPLE;

}
