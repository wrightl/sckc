import { Component } from '@angular/core';

@Component({
  selector: 'app-joining',
  templateUrl: './joining.component.html',
  styleUrls: ['./joining.component.scss'],
})
export class JoiningComponent {

  membershipyear: string;

  constructor() {
    this.membershipyear = (new Date()).getFullYear().toString();
  }
}
