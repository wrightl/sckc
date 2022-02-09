export const DOCUMENTS: Document[] = [
  {
    name: 'Club Code of Conduct',
    link: '/code_of_conduct',
  },
  {
    name: 'Coaches and Club Officials Code of Conduct',
    link: './Documents/CoachesOfficialsCodeofConduct.pdf',
  },
];

export interface Document {
  name: string;
  link: string;
}
