import { BookingItem } from './pool';

export const RIVERINFO: RiverInfo = {
  adultSessionCost: 4.5,
  adultEquipmentHire: 3.5,
  childSessionCost: 4,
  childEquipmentHire: 3.5,
  adultSessionCostNonMember: 6.5,
  adultEquipmentHireNonMember: 3.5,
  childSessionCostNonMember: 6,
  childEquipmentHireNonMember: 3.5,
};

export const RIVER_BOOKING_ITEMS: BookingItem[] = [
  {
    description: 'Adult Member - With Equipment Hire',
    cost: RIVERINFO.adultSessionCost + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Adult Non-Member - With Equipment Hire',
    cost: RIVERINFO.adultSessionCostNonMember + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Adult Member',
    cost: RIVERINFO.adultSessionCost,
    quantity: 0,
  },
  {
    description: 'Adult Non-Member',
    cost: RIVERINFO.adultSessionCostNonMember,
    quantity: 0,
  },
  {
    description: 'Child Member - With Equipment Hire',
    cost: RIVERINFO.childSessionCost + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Child Non-Member - With Equipment Hire',
    cost: RIVERINFO.childSessionCostNonMember + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Child Member',
    cost: RIVERINFO.childSessionCost,
    quantity: 0,
  },
  {
    description: 'Child Non-Member',
    cost: RIVERINFO.childSessionCostNonMember,
    quantity: 0,
  },
];

export interface RiverInfo {
  adultSessionCost: number;
  adultEquipmentHire: number;
  childSessionCost: number;
  childEquipmentHire: number;
  adultSessionCostNonMember: number;
  adultEquipmentHireNonMember: number;
  childSessionCostNonMember: number;
  childEquipmentHireNonMember: number;
}
