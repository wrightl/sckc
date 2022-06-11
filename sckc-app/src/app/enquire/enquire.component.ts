import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { CalendarEvent } from '../models/calendar-event';

@Component({
  selector: 'app-enquire',
  templateUrl: './enquire.component.html',
  styleUrls: ['./enquire.component.scss']
})
export class EnquireComponent {

  booking!: CalendarEvent;

  name = new FormControl('', [Validators.required]);
  telno = new FormControl('');
  email = new FormControl('', [Validators.required, Validators.email]);
  message = new FormControl('', [Validators.required]);
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
    this.telno.markAsTouched();
    this.email.markAsTouched();
    this.message.markAsTouched();
    this.name.updateValueAndValidity();
    this.telno.updateValueAndValidity();
    this.email.updateValueAndValidity();
    this.message.updateValueAndValidity();

    if (
      this.name.errors ||
      this.email.errors ||
      this.message.errors
    )
      return false;

    this.http
      .post(`${environment.baseApiUrl}BookingEnquiry`, {
        name: this.name.value,
        email: this.email.value,
        telno: this.telno.value,
        message: this.message.value,
      })
      .subscribe(
        (result) => {
          this.response = result.toString();

          this.name.reset();
          this.email.reset();
          this.telno.reset();
          this.message.reset();
        },
        (error) =>
          (this.error =
            'Unfortunately an error occurred. Please try again, or send an email to <a href="mailto:enquiries@sheffieldcitykayakclub.co.uk">enquiries@sheffieldcitykayakclub.co.uk</a>')
      );

    return false;
  }

}
