import { Component } from '@angular/core';
import { CalendarEvent } from './calendar-event';
import { EventsService } from './events.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.scss'],
})
export class EventsComponent {
  events: CalendarEvent[];
  displayedColumns: string[] = ['date', 'name', 'book'];

  constructor(eventsService: EventsService) {
    this.events = eventsService.getEvents();
  }
}
