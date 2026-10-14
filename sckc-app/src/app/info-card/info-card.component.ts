import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-info-card',
    templateUrl: './info-card.component.html',
    styleUrls: ['./info-card.component.scss'],
    standalone: false
})
export class InfoCardComponent {
  @Input() imageSource = '';
  @Input() link = '';
  @Input() content = '';
  @Input() title = '';
}
