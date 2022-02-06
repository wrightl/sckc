import { Component } from '@angular/core';
import { EventsService } from '../services/events.service';
import { CalendarEvent } from '../models/calendar-event';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.scss'],
})
export class EventsComponent {
  isReady = false;
  events: CalendarEvent[] = [];
  displayedColumns: string[] = ['date', 'name', 'book'];

  constructor(eventsService: EventsService) {
    eventsService.getEvents().subscribe((data: CalendarEvent[]) => {
      this.events = data;
      this.isReady = true;
    });
  }
}
