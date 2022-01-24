import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss'],
})
export class PersonComponent {
  @Input() name = '';
  @Input() role = '';
  @Input() imageUrl = '';
  @Input() longText = '';
  @Input() emailAddress = '';

  constructor() {}
}
