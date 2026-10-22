import { Component, ChangeDetectionStrategy } from '@angular/core';
import { HOME_IMAGES } from '../data/home-images';

@Component({
    selector: 'app-landing',
    templateUrl: './landing.component.html',
    styleUrls: ['./landing.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class LandingComponent {
  // A different hero photo on each visit
  heroImage = HOME_IMAGES[Math.floor(Math.random() * HOME_IMAGES.length)];
}
