import { Component, Inject, Input } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';

@Component({
  selector: 'app-booking-request-conf',
  templateUrl: './booking-request-conf.component.html',
  styleUrls: ['./booking-request-conf.component.scss']
})
export class BookingRequestConfComponent {

  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any){
    this.message = this.message || data.message;
  }

  @Input() message = '';
}
