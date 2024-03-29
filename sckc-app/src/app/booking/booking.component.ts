import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { MEMBERSHIP_INFO } from '../data/membership';
import { BookingItem, POOLINFO, POOL_BOOKING_ITEMS } from '../data/pool';
import { RIVERINFO, RIVER_BOOKING_ITEMS } from '../data/river';
import { CalendarEvent } from '../models/calendar-event';
import { BookingService } from '../services/booking.service';

@Component({
  selector: 'app-booking',
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.scss'],
})
export class BookingComponent implements OnInit {
  booking!: CalendarEvent;
  river_info = RIVERINFO;
  pool_info = POOLINFO;
  membership_info = MEMBERSHIP_INFO;

  displayedColumns: string[] = ['description', 'cost', 'quantity', 'total'];
  bookingItems: BookingItem[] = [];

  name = new FormControl('', [Validators.required]);
  number = new FormControl('');
  email = new FormControl('', [Validators.required, Validators.email]);
  memberno = new FormControl('', [Validators.required]);
  telno = new FormControl('');
  message = new FormControl('');
  error = '';
  response = '';

  isBusy = false;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private bookingService: BookingService,
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

    switch (this.booking.EventType) {
      case 'pool':
        this.bookingItems = POOL_BOOKING_ITEMS;
        break;
      case 'river':
        this.bookingItems = RIVER_BOOKING_ITEMS;
        break;
      case 'trip':
        break;
    }

    // Reset all quantities, bit crap
    this.bookingItems.forEach((x) => (x.quantity = 0));
  }

  getErrorMessage(field: FormControl) {
    if (field.hasError('required')) {
      return 'You must enter a value';
    }

    return field.hasError('email') ? 'Not a valid email' : '';
  }

  book() {
    this._book();
  }

  bookAndPay() {
    this._book(true);
  }

  private _book(payNow = false) {
    this.error = '';
    this.response = '';

    this.name.markAsTouched();
    this.email.markAsTouched();
    this.memberno.markAsTouched();
    this.telno.markAsTouched();
    this.message.markAsTouched();
    this.name.updateValueAndValidity();
    this.email.updateValueAndValidity();
    this.telno.updateValueAndValidity();
    this.memberno.updateValueAndValidity();
    this.message.updateValueAndValidity();

    if (this.getTotalQuantity() <= 0) {
      this.error = 'Please select the number of participants';
    }

    if (
      this.name.errors ||
      this.email.errors ||
      this.telno.errors ||
      this.memberno.errors ||
      this.message.errors ||
      this.error
    ) {
      const el = document.getElementsByClassName('router-container');
      if (el?.length > 0) el[0].scrollTop = 0;
      return false;
    }

    this.isBusy = true;

    this.bookingService
      .book({
        event: this.booking.Summary,
        date: this.booking.LocaleDate,
        name: this.name.value,
        email: this.email.value,
        memberno: this.memberno.value,
        telno: this.telno.value,
        message: this.message.value,
        items: this.bookingItems.filter((x) => x.quantity > 0),
        payNow: payNow,
      })
      .subscribe(
        (result) => {
          if (payNow) {
            const response = result.toString();
            this.document.location.href = response;
          } else {
            this.router.navigate(['/booking-conf']);
          }
          this.isBusy = false;
        },
        (error) => {
          this.error =
            'Unfortunately an error occurred. Please try again, or send an email to <a href="mailto:bookingrequest@sheffieldcitykayakclub.co.uk">bookingrequest@sheffieldcitykayakclub.co.uk</a>';
          this.isBusy = false;
        }
      );

    return false;
  }

  public getTotalQuantity() {
    return this.bookingItems
      .map((t) => t.quantity)
      .reduce((acc, value) => acc + value, 0);
  }

  public getItemTotal(row: BookingItem) {
    return row.cost * row.quantity;
  }

  public getTotalCost() {
    return this.bookingItems
      .map((t) => t.cost * t.quantity)
      .reduce((acc, value) => acc + value, 0);
  }

  decreaseQuantity(row: BookingItem) {
    row.quantity--;
  }

  increaseQuantity(row: BookingItem) {
    row.quantity++;
  }

  disableDecreaseQuantity(row: BookingItem) {
    return row.quantity <= 0;
  }

  disableIncreaseQuantity(row: BookingItem) {
    return row.quantity >= 4;
  }
}
