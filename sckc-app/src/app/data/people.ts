export const PEOPLE: Person[] = [
  {
    name: 'Dave Wright',
    role: 'Chairman',
    longText:
      "",
    emailAddress: 'chairman@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: 'Steve Cheetham',
    role: 'Membershp Secretary',
    longText:
      "<p>Since Steve started kayaking in the 1970's he has enjoyed many aspects of the sport, especially recreational paddling on placid and white water in the uk, and on continental rivers during most summer holidays. As a slalomist he has previously reached division 1, he passed the advanced proficiency test (inland) and qualified as senior instructor (inland) in the 1980's and is currently a BC level 3 coach.</p><p>Steve also holds certificates in outdoor first aid and safeguarding children</p><p>Steve is DBS certified</p>",
    emailAddress: 'membership@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/stevecheetham.jpg',
  },
  {
    name: 'Gary Edwards',
    role: 'Welfare Officer',
    longText:
      '',
    emailAddress: 'welfare@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: 'Andy Green',
    role: 'Equipment Officer',
    longText: '',
    emailAddress: 'equipment@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: 'Neil Webster',
    role: 'Safety Officer',
    longText: '',
    emailAddress: 'safety@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: '????',
    role: 'Trip Coordinator',
    longText:
      '',
    emailAddress: 'trips@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: 'Lee Wright',
    role: 'Treasurer',
    longText: '',
    emailAddress: 'treasurer@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
  {
    name: 'Neil Taylor',
    role: 'Publicity Officer',
    longText: '',
    emailAddress: 'publicity@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/river.png',
  },
];

export interface Person {
  name: string;
  role: string;
  longText: string;
  emailAddress: string;
  imageUrl: string;
}
