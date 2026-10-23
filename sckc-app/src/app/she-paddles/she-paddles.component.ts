import { Component, ChangeDetectionStrategy } from '@angular/core';
import { PEOPLE } from '../data/people';

@Component({
    selector: 'app-she-paddles',
    templateUrl: './she-paddles.component.html',
    styleUrls: ['./she-paddles.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ShePaddlesComponent {
  // The ambassador's card comes from the same data as the Club Officials page, so the two never drift apart
  ambassador = PEOPLE.find(person => person.roles.includes('She Paddles Club Ambassador'));
}
