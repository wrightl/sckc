import { Component, ChangeDetectionStrategy } from '@angular/core';
import { POOLINFO } from '../data/pool';
import { RIVERINFO } from '../data/river';

@Component({
    selector: 'app-new-to-the-club',
    templateUrl: './new-to-the-club.component.html',
    styleUrls: ['./new-to-the-club.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class NewToTheClubComponent {
  river = RIVERINFO;
  pool = POOLINFO;
}
