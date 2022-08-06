import { BookingItem } from '../data/pool';
import { BaseBooking } from './enquiry';

export interface Booking extends BaseBooking {
  memberno: string;
  items: BookingItem[];
  payNow: boolean;
}
