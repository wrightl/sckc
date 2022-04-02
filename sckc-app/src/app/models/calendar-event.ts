export interface CalendarEventMonth {
  Month: string;
  Events: CalendarEvent[];
}

export interface CalendarEvent {
  Description: string | null;
  EndDateTime: Date;
  EventId: string;
  IsSpecialEvent: boolean;
  LocaleDate: Date;
  StartDateAsString: string;
  StartDateTime: string;
  Status: string;
  Summary: string;
  Date: Date;
  Month: number;
  EventType: "river" | "pool" | "trip";
}
