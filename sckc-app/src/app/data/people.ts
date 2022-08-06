export const PEOPLE: Person[] = [
  {
    name: 'Dave Wright',
    role: 'Chairman',
    longText: "<p>David has been involved in outdoor activities for over 40 years; he only started getting involved in paddlesports 12 or so years ago but wishes he'd started much sooner!</p><p>He enjoys stand up paddle boarding, canoeing and kayaking, he is a Level 3 British Canoeing Coach and gets great satisfaction out of introducing new people to the sport.</p><p>David holds a Certificate in Outdoor First Aid, is a British Canoeing Mentor and Coach Developer, is DBS Certified and holds a Certificate in Safeguarding and Protecting Children  </p>",
    emailAddress: 'chairman@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/dave.jpg',
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
      '<p>First tried kayaking as a scout back in the 80’s generally on murky ponds whilst on camp. Followed by sporadic river trips whilst at college.  Skip 30 years and you could say Paddlesport has become an obsession.</p><p>Currently Working as a Canoe/Kayak & Raft Guide for Rapid Horizons.</p><p>Volunteer Water Search and Rescue Technician</p><br><p>Qualifications:</p><p><ul><li>Paddlesport Instructor</li><li>Paddlesport Touring Leader</li><li>FREC 3 First Aid</li><li>Level 1 Safeguarding (adults & children)</li><li>DBS',
    emailAddress: 'welfare@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/gary.jpeg',
  },
  {
    name: 'Andy Green',
    role: 'Equipment Officer',
    longText: '<p>I first sat in a kayak on the river tees in the now infamous Barnard Castle 20+ years ago, joined the local canoe club and was hooked. Since then I’ve paddled mainly within mainland Europe, but have enjoyed a few slightly more exotic destinations. This has mainly been whitewater kayak but recently I’ve been paddling more canoe, sea kayak and SUP which brings a new set of opportunities to paddlesports for me and my family.</p><p>After many years of paddling with mates I thought it was about time I joined a club again so joined Sheffield City Kayak club just before lockdown and was welcomed by the great bunch of paddlers and coaches, unfortunate timing for me as things wound down for the covid pandemic and then post lockdown I joined the committee to help bring things back up to speed.</p><p>I’ve had so many fantastic experiences in and around the river, I love the idea of opening up these experiences to more people which is why I continue to coach and river lead. P.s. the number of boats you should have is “b+1”, where “b” is the number of boats you currently own. </p>',
    emailAddress: 'equipment@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/andy.png',
  },
  {
    name: 'Neil Webster',
    role: 'Safety Officer',
    longText: '<p>Neil joined the club early 2022 but has kayaked in his younger years with the scouts and recently sea kayaked whilst working in jersey in the Channel Islands. Neil has worked in the construction industry for over 30 years in senior roles that heavily involved health and safety in some form or another on a daily basis and achieved several safety qualifications and awards </p><p>He looks forward to the Challenge of safety office with the club</p>',
    emailAddress: 'safety@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/neilw.png',
  },
  // {
  //   name: '????',
  //   role: 'Trip Coordinator',
  //   longText: '',
  //   emailAddress: 'trips@sheffieldcitykayakclub.co.uk',
  //   imageUrl: 'assets/river.png',
  // },
  {
    name: 'Lee Wright',
    role: 'Treasurer',
    longText: "<p>Lee joined the club back in 2012 after watching the kayaking events at the London Olympics and deciding he and his wife needed a joint hobby.</p><p>Since then he's got into white water rivers and paddles both locally and further afield, with his favourite paddling destination so far being Slovenia which he experienced on a week long trip in 2022.</p>",
    emailAddress: 'treasurer@sheffieldcitykayakclub.co.uk',
    imageUrl: 'assets/lee.jpg',
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
