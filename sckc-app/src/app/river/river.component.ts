import { Component } from '@angular/core';
import { RIVERINFO } from '../data/river';

@Component({
  selector: 'app-river',
  templateUrl: './river.component.html',
  styleUrls: ['./river.component.scss']
})
export class RiverComponent {

  info = RIVERINFO;

  sessionFee: number = 1000000;

}
