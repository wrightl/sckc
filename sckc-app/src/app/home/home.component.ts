import { Component } from '@angular/core';
import { HOME_IMAGES } from '../data/home-images';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

  images = HOME_IMAGES;

  constructor() { }

}
