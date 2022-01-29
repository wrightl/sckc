import { Injectable } from '@angular/core';
import { CalendarEvent } from './calendar-event';
import { MOCK_EVENTS } from './mock-events';

@Injectable({
  providedIn: 'root',
})
export class EventsService {
  constructor() {}

  getEvents(): CalendarEvent[] {
    return MOCK_EVENTS;
  }
}
