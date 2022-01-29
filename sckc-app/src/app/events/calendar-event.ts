export interface CalendarEvent {
  description: string | null;
  endDateTime: Date;
  eventId: string;
  isSpecialEvent: boolean;
  localeDate: Date;
  startDateAsString: string;
  startDateTime: string;
  status: string;
  summary: string;
  date: Date;
  month: number;
}
