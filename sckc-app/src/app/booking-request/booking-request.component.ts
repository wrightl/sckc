import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { BookingRequestConfComponent } from '../booking-request-conf/booking-request-conf.component';
import { CalendarEvent } from '../models/calendar-event';

@Component({
  selector: 'app-booking-request',
  templateUrl: './booking-request.component.html',
  styleUrls: ['./booking-request.component.scss'],
})
export class BookingRequestComponent implements OnInit {
  booking!: CalendarEvent;

  name = new FormControl('', [Validators.required]);
  number = new FormControl('');
  names = new FormControl('');
  email = new FormControl('', [Validators.required, Validators.email]);
  telno = new FormControl('');
  message = new FormControl('');
  error = '';
  response = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.booking = window.history.state;
    if (
      !this.booking ||
      !this.booking?.Summary ||
      new Date(this.booking.StartDateTime) < new Date()
    ) {
      this.router.navigate(['/events']);

      this._snackBar.open(`That event cannot be booked`, undefined, {
        duration: 5000,
        verticalPosition: 'top',
      });
    }
  }

  getErrorMessage(field: FormControl) {
    if (field.hasError('required')) {
      return 'You must enter a value';
    }

    return field.hasError('email') ? 'Not a valid email' : '';
  }

  sendMessage() {
    this.error = '';
    this.response = '';

    this.name.markAsTouched();
    this.number.markAsTouched();
    this.names.markAsTouched();
    this.email.markAsTouched();
    this.telno.markAsTouched();
    this.message.markAsTouched();
    this.name.updateValueAndValidity();
    this.number.updateValueAndValidity();
    this.names.updateValueAndValidity();
    this.email.updateValueAndValidity();
    this.telno.updateValueAndValidity();
    this.message.updateValueAndValidity();

    if (
      this.name.errors ||
      this.number.errors ||
      this.names.errors ||
      this.email.errors ||
      this.telno.errors ||
      this.message.errors
    )
      return false;

    this._sendBookingRequest();

    return false;
  }

  private _sendBookingRequest() {
    this.http
      .post(`${environment.baseApiUrl}BookingRequest`, {
        event: this.booking.Summary,
        date: this.booking.LocaleDate,
        name: this.name.value,
        number: this.number.value,
        names: this.names.value,
        email: this.email.value,
        telno: this.telno.value,
        message: this.message.value,
      })
      .subscribe(
        (result) => {
          const response = result.toString();
          this._snackBar.openFromComponent(BookingRequestConfComponent, {
            duration: 5000,
            verticalPosition: 'top',
            data: { message: response },
          });

          this.router.navigate(['/events']);
        },
        (error) =>
          (this.error =
            'Unfortunately an error occurred. Please try again, or send an email to <a href="mailto:bookingrequest@sheffieldcitykayakclub.co.uk">bookingrequest@sheffieldcitykayakclub.co.uk</a>')
      );
  }
}
