export const PEOPLE: Person[] = [
  {
    name: 'Tony Canning',
    role: 'Chairman & Equipment Officer',
    longText:
      "<p>Tony first got into kayaking in his teens at Bewerley Park outward bound school in north yorkshire in the 60's. After a long time away from it he went on a kayaking trip down the Wanganui river in new zealand and his interest of all things water-related had been rekindled. Tony is now a level 2 coach and is able to assess 1 & 2 star awards.</p><p>He is probably Sheffields first and last coracle maker.</p><p>As well as being chairman of sckc, he is a trustee on the Upper Don walk trust and special adviser to the board of the Sheffield river stewardship company.</p>",
    emailAddress: 'chairman@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/tonycanning.jpg',
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
    name: 'Kevin Plumb',
    role: 'Welfare Officer',
    longText:
      '<p>Kevin started kayaking around 2004 as something he could do with his kids. The girls soon surpassed his abilities and took up slalom competition and are both currently in div 1.</p><p>Kevin has gained his BC 3* and level 1 coaching award as well as UK sport child protection and coaching disabled courses. He is also a cub scout leader, nsra club airgun instructor, archery instructor and climbing wall instructor within scouting, and holds a rya level 2 in dingy sailing. Most of his spare time goes into either scouting or kayaking (especially as logistics support to his kids for slalom competitions all over the country).</p>',
    emailAddress: 'welfare@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/kevinplumb.jpg',
  },
  {
    name: 'Neil Furmidge',
    role: 'Trip Coordinator',
    longText:
      '<p>Neil is a paraglider and mountain biker who started kayaking again in 2010 after not paddling since childhood. Paddling white water, sea kayaks and open boats, Neil regularly heads off on paddling trips around the UK. Neil is a level 1 BC coach and holds an outdoor first aid certificate, safeguarding children, 4 star white water leader, 4 star sea kayak and 3 star canoe BCU awards.</p>',
    emailAddress: 'trips@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/neil.jpg',
  },
  {
    name: 'Dave Wright',
    role: 'Treasurer',
    longText: '',
    emailAddress: 'treasurer@sheffieldcitykayakclub.co.uk',
    imageUrl: '',
  },
];

export interface Person {
  name: string;
  role: string;
  longText: string;
  emailAddress: string;
  imageUrl: string;
}
