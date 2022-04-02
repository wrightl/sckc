import { Component, Input, OnInit } from '@angular/core';
import { CalendarEvent, CalendarEventMonth } from '../models/calendar-event';
import { EventsService } from '../services/events.service';

@Component({
  selector: 'app-event-calendar',
  templateUrl: './event-calendar.component.html',
  styleUrls: ['./event-calendar.component.scss'],
})
export class EventCalendarComponent implements OnInit {
  @Input()
  mode: 'normal' | 'small' = 'normal';

  isReady = false;
  events: CalendarEvent[] = [];
  groupedEvents: CalendarEventMonth[] = [];
  displayedColumns: string[] = ['day', 'date', 'name', 'book'];

  constructor(private eventsService: EventsService) {}

  ngOnInit(): void {
    if (this.mode === 'small') {
      this.eventsService.getEvents(10).subscribe((data: CalendarEvent[]) => {
        this.events = data;
        this.isReady = true;
      });
    } else {
      this.eventsService
        .getGroupedEvents()
        .subscribe((data: CalendarEventMonth[]) => {
          this.groupedEvents = data;
          this.isReady = true;
        });
    }
  }
}
