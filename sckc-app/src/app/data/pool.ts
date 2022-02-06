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
