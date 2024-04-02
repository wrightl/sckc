export const POOLINFO: PoolInfo = {
  adultThirtyMinuteSession: 6,
  adultSixtMinuteSession: 12,
  adultFullSession: 16,
  childThirtyMinuteSession: 3,
  childSixtyMinuteSession: 6,
  childFullSession: 7,
};

export const POOL_BOOKING_ITEMS: BookingItem[] = [
  {
    description: 'Adult Member',
    cost: POOLINFO.adultFullSession,
    quantity: 0,
  },
  {
    description: 'Child Member',
    cost: POOLINFO.childFullSession,
    quantity: 0,
  },
];

export interface PoolInfo {
  adultThirtyMinuteSession: number;
  adultSixtMinuteSession: number;
  adultFullSession: number;
  childThirtyMinuteSession: number;
  childSixtyMinuteSession: number;
  childFullSession: number;
}

export interface BookingItem {
  description: string;
  quantity: number;
  cost: number;
}
