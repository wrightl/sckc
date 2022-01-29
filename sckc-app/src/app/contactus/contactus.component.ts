import { Component, Inject, OnInit } from '@angular/core';
import { PEOPLE } from '../data/data';

@Component({
  selector: 'app-contactus',
  templateUrl: './contactus.component.html',
  styleUrls: ['./contactus.component.scss'],
})
export class ContactusComponent {
  people = PEOPLE;

  constructor() {}
}
