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
