import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-busy-button',
  templateUrl: './busy-button.component.html',
  styleUrls: ['./busy-button.component.scss'],
})
export class BusyButtonComponent {
  @Input() busy = false;
  @Input() colour = 'accent';
  @Input() label = '';

  // eslint-disable-next-line @angular-eslint/no-output-native
  @Output() clickButton: EventEmitter<any> = new EventEmitter();

  onClick() {
    this.clickButton.emit();
  }
}
