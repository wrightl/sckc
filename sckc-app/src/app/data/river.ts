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
    description: 'Adult Session - Member - With Equipment',
    cost: RIVERINFO.adultSessionCost + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Adult Session - Member - No Equipment',
    cost: RIVERINFO.adultSessionCost,
    quantity: 0,
  },
  {
    description: 'Adult Session - Non-Member - With Equipment Hire',
    cost: RIVERINFO.adultSessionCostNonMember + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Adult Session - Non-Member',
    cost: RIVERINFO.adultSessionCostNonMember,
    quantity: 0,
  },
  {
    description: 'Child Session - Member - With Equipment Hire',
    cost: RIVERINFO.childSessionCost + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Child Session - Member',
    cost: RIVERINFO.childSessionCost,
    quantity: 0,
  },
  {
    description: 'Child Session - Non-Member - With Equipment Hire',
    cost: RIVERINFO.childSessionCostNonMember + RIVERINFO.adultEquipmentHire,
    quantity: 0,
  },
  {
    description: 'Child Session - Non-Member',
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
