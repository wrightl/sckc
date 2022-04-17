import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { CalendarEvent, CalendarEventMonth } from '../models/calendar-event';
// import { MOCK_EVENTS } from './mock-events';

@Injectable({
  providedIn: 'root',
})
export class EventsService {
  constructor(private http: HttpClient) {}

  getEvents(count: number) {
    return this.http.get<CalendarEvent[]>(
      `${environment.baseApiUrl}GetEvents?count=${count}`
    );
  }

  getGroupedEvents() {
    return this.http.get<CalendarEventMonth[]>(
      `${environment.baseApiUrl}GetGroupedEvents`
    );
  }

  getEventsOfType(type: string, count: number) {
    return this.http.get<CalendarEvent[]>(
      `${environment.baseApiUrl}getEventsOfType?type=${type}&count=${count}`
    );
  }
}
