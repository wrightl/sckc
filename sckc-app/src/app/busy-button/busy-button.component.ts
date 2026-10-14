import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-busy-button',
    templateUrl: './busy-button.component.html',
    styleUrls: ['./busy-button.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class BusyButtonComponent {
  @Input() busy = false;
  @Input() colour = 'accent';
  @Input() label = '';

  @Output() clickButton: EventEmitter<any> = new EventEmitter();

  onClick() {
    this.clickButton.emit();
  }
}
