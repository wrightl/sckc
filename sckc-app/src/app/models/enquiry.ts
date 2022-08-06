import { CalendarEventType } from './calendar-event';

export interface BaseBooking {
  event: string;
  date: Date;
  name: string;
  email: string;
  telno: string;
  message: string;
}

export interface Enquiry extends BaseBooking {
  type: CalendarEventType;
}
