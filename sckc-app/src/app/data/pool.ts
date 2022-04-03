export const POOLINFO: PoolInfo = {
  adultHalfSession: 10,
  adultFullSession: 16,
  childHalfSession: 4,
  childFullSession: 7,
  adultHalfSessionNonMember: 12,
  adultFullSessionNonMember: 18,
  childHalfSessionNonMember: 6,
  childFullSessionNonMember: 9,
};

export const POOL_BOOKING_ITEMS: BookingItem[] = [
  {
    description: 'Adult Member',
    cost: POOLINFO.adultFullSession,
    quantity: 0,
  },
  {
    description: 'Adult Non-Member',
    cost: POOLINFO.adultFullSessionNonMember,
    quantity: 0,
  },
  {
    description: 'Child Member',
    cost: POOLINFO.childFullSession,
    quantity: 0,
  },
  {
    description: 'Child Non-Member',
    cost: POOLINFO.childFullSessionNonMember,
    quantity: 0,
  },
];

export interface PoolInfo {
  adultHalfSession: number;
  adultFullSession: number;
  childHalfSession: number;
  childFullSession: number;
  adultHalfSessionNonMember: number;
  adultFullSessionNonMember: number;
  childHalfSessionNonMember: number;
  childFullSessionNonMember: number;
}

export interface BookingItem {
  description: string;
  quantity: number;
  cost: number;
}
