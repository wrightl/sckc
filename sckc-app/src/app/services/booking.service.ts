import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Booking } from '../models/booking';
import { Enquiry } from '../models/enquiry';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  constructor(private http: HttpClient) {}

  enquire(enquiry: Enquiry) {
    return this.http.post(`${environment.baseApiUrl}BookingEnquiry`, {
      ...enquiry,
      isLive: environment.production,
    });
  }

  book(booking: Booking) {
    return this.http.post(`${environment.baseApiUrl}Book`, {
      ...booking,
      isLive: environment.production,
    });
  }
}
