import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { CalendarEvent } from '../models/calendar-event';

@Component({
  selector: 'app-member-check',
  templateUrl: './member-check.component.html',
  styleUrls: ['./member-check.component.scss']
})
export class MemberCheckComponent implements OnInit {

  booking!: CalendarEvent;

  constructor(
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
}
