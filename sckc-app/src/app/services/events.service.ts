import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { CalendarEvent } from '../models/calendar-event';
// import { MOCK_EVENTS } from './mock-events';

@Injectable({
  providedIn: 'root',
})
export class EventsService {
  constructor(private http: HttpClient) {}

  getEvents() {
    //return MOCK_EVENTS;
    return this.http.get<CalendarEvent[]>(`${environment.baseApiUrl}events`);
  }
}
