// Invented data for the redesign prototypes. Nothing here calls the server and no module imports it.

export type Difficulty = 'Easy' | 'Moderate' | 'Hard';
export type Transport = 'Walking' | 'Bicycle' | 'Car';

// How the signed-in tourist relates to a tour; the search card and the details page show it.
export type TourRelation = 'none' | 'inCart' | 'purchased' | 'inProgress' | 'author';

export interface ProtoKeyPoint {
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  image: string;
  isOpen: boolean;
}

export interface ProtoTour {
  id: string;
  name: string;
  excerpt: string;
  description: string;
  difficulty: Difficulty;
  lengthKm: number;
  // Minutes per transport mode the author entered.
  times: Partial<Record<Transport, number>>;
  groupMin: number;
  groupMax: number;
  tags: string[];
  price: number;
  image: string;
  relation: TourRelation;
  keyPoints: ProtoKeyPoint[];
}

export const difficultyLabels: Record<Difficulty, string> = {
  Easy: 'Lako',
  Moderate: 'Srednje',
  Hard: 'Teško',
};

export const difficultyClasses: Record<Difficulty, string> = {
  Easy: 'badge badge--easy',
  Moderate: 'badge badge--moderate',
  Hard: 'badge badge--hard',
};

export const transportLabels: Record<Transport, string> = {
  Walking: 'Peške',
  Bicycle: 'Bicikl',
  Car: 'Automobil',
};

const art = (name: string): string => `/images/ilustracije/${name}.svg`;

export const tours: ProtoTour[] = [
  {
    id: 'stari-grad',
    name: 'Stari grad Novog Sada',
    excerpt: 'Od Trga slobode do Dunavskog parka, uz priče o ljudima koji su gradili grad.',
    description:
      'Lagana šetnja kroz centar. Na svakoj tački otključavate deo priče o zgradama, trgovima i ' +
      'ljudima koji su ih gradili. Tura je pogodna za porodice i prvi susret sa gradom.',
    difficulty: 'Easy',
    lengthKm: 3.2,
    times: { Walking: 55, Bicycle: 20 },
    groupMin: 1,
    groupMax: 8,
    tags: ['istorija', 'grad'],
    price: 0,
    image: art('exploring'),
    relation: 'purchased',
    keyPoints: [
      {
        name: 'Trg slobode',
        description: 'Gradska kuća i Katedrala, srce starog grada.',
        latitude: 45.2551,
        longitude: 19.8452,
        image: art('destination'),
        isOpen: true,
      },
      {
        name: 'Zmaj Jovina',
        description: 'Najstarija pešačka ulica i Vladičanski dvor.',
        latitude: 45.2557,
        longitude: 19.8485,
        image: art('finding-the-way'),
        isOpen: false,
      },
      {
        name: 'Dunavska ulica',
        description: 'Kuće trgovaca i priča o poplavi 1876.',
        latitude: 45.2564,
        longitude: 19.8511,
        image: art('travel-destination'),
        isOpen: false,
      },
      {
        name: 'Dunavski park',
        description: 'Park sa jezerom i spomenikom Đorđu Natoševiću.',
        latitude: 45.2546,
        longitude: 19.8526,
        image: art('hiking'),
        isOpen: false,
      },
    ],
  },
  {
    id: 'tvrdjava',
    name: 'Tajne Petrovaradinske tvrđave',
    excerpt: 'Podzemni hodnici, sat koji kasni i legenda o zaljubljenom vojniku.',
    description:
      'Tura vodi preko mosta do tvrđave i kroz njene bedeme. Najlepše tajne čekaju na ' +
      'poslednjim tačkama, pa je preporučujemo u kasno popodne.',
    difficulty: 'Moderate',
    lengthKm: 5.8,
    times: { Walking: 110 },
    groupMin: 2,
    groupMax: 10,
    tags: ['istorija', 'tvrđava', 'pogled'],
    price: 40,
    image: art('adventure-map'),
    relation: 'none',
    keyPoints: [
      {
        name: 'Varadinski most',
        description: 'Ulaz u priču: pogled na grad sa Dunava.',
        latitude: 45.2531,
        longitude: 19.8589,
        image: art('destination'),
        isOpen: true,
      },
      {
        name: 'Sat sa obrnutim kazaljkama',
        description: 'Zašto mala kazaljka pokazuje minute.',
        latitude: 45.2522,
        longitude: 19.8626,
        image: art('finding-the-way'),
        isOpen: false,
      },
      {
        name: 'Podzemni hodnici',
        description: 'Šesnaest kilometara hodnika u četiri nivoa.',
        latitude: 45.2508,
        longitude: 19.8643,
        image: art('my-current-location'),
        isOpen: false,
      },
    ],
  },
  {
    id: 'fruska-gora',
    name: 'Fruškogorske staze',
    excerpt: 'Šumska tura do manastira sa pogledom na Dunav.',
    description:
      'Planinarska tura kroz šumu Fruške gore. Ponesite vodu i dobru obuću; deo staze je ' +
      'strm i kamenit.',
    difficulty: 'Hard',
    lengthKm: 12.4,
    times: { Walking: 240, Bicycle: 90 },
    groupMin: 2,
    groupMax: 6,
    tags: ['priroda', 'planinarenje'],
    price: 60,
    image: art('hiking'),
    relation: 'inCart',
    keyPoints: [
      {
        name: 'Stražilovo',
        description: 'Polazna tačka i grob Branka Radičevića.',
        latitude: 45.1695,
        longitude: 19.8458,
        image: art('hiking'),
        isOpen: true,
      },
      {
        name: 'Brankov čardak',
        description: 'Vidikovac nad Sremskim Karlovcima.',
        latitude: 45.1624,
        longitude: 19.8551,
        image: art('destination'),
        isOpen: false,
      },
    ],
  },
  {
    id: 'liman-kej',
    name: 'Kej i Liman biciklom',
    excerpt: 'Ravna ruta uz Dunav, od Štranda do Mosta slobode.',
    description: 'Ruta po biciklističkoj stazi uz reku. Pogodna za sve uzraste.',
    difficulty: 'Easy',
    lengthKm: 7.5,
    times: { Bicycle: 35, Walking: 95 },
    groupMin: 1,
    groupMax: 12,
    tags: ['bicikl', 'reka'],
    price: 15,
    image: art('travel-destination'),
    relation: 'inProgress',
    keyPoints: [
      {
        name: 'Štrand',
        description: 'Gradska plaža.',
        latitude: 45.2443,
        longitude: 19.8545,
        image: art('travel-destination'),
        isOpen: true,
      },
      {
        name: 'Kej žrtava racije',
        description: 'Spomenik i šetalište.',
        latitude: 45.2487,
        longitude: 19.8549,
        image: art('destination'),
        isOpen: false,
      },
      {
        name: 'Most slobode',
        description: 'Najduži most u gradu.',
        latitude: 45.2398,
        longitude: 19.8396,
        image: art('finding-the-way'),
        isOpen: false,
      },
    ],
  },
  {
    id: 'sremski-karlovci',
    name: 'Vino i priče Sremskih Karlovaca',
    excerpt: 'Česma četiri lava, gimnazija i podrumi sa starim bermetom.',
    description: 'Tura po malom baroknom gradu, sa degustacijom na poslednjoj tački.',
    difficulty: 'Moderate',
    lengthKm: 4.1,
    times: { Walking: 80, Car: 25 },
    groupMin: 2,
    groupMax: 8,
    tags: ['vino', 'istorija'],
    price: 50,
    image: art('destination'),
    relation: 'author',
    keyPoints: [
      {
        name: 'Česma četiri lava',
        description: 'Centralni trg.',
        latitude: 45.2021,
        longitude: 19.9343,
        image: art('destination'),
        isOpen: true,
      },
      {
        name: 'Karlovačka gimnazija',
        description: 'Najstarija gimnazija u Srbiji.',
        latitude: 45.2015,
        longitude: 19.9356,
        image: art('finding-the-way'),
        isOpen: false,
      },
    ],
  },
  {
    id: 'detelinara',
    name: 'Grafiti Detelinare',
    excerpt: 'Ulična umetnost i priče njenih autora.',
    description: 'Gradska tura kroz kvart poznat po muralima.',
    difficulty: 'Easy',
    lengthKm: 2.6,
    times: { Walking: 45 },
    groupMin: 1,
    groupMax: 15,
    tags: ['umetnost', 'grad'],
    price: 20,
    image: art('my-current-location'),
    relation: 'none',
    keyPoints: [
      {
        name: 'Mural na Rumenačkoj',
        description: 'Najveći mural u gradu.',
        latitude: 45.2633,
        longitude: 19.8188,
        image: art('my-current-location'),
        isOpen: true,
      },
      {
        name: 'Park Detelinara',
        description: 'Galerija na otvorenom.',
        latitude: 45.2668,
        longitude: 19.8133,
        image: art('exploring'),
        isOpen: false,
      },
    ],
  },
];

export type ChallengeKind = 'quiz' | 'social' | 'location';
export type ChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface ProtoChallenge {
  id: string;
  kind: ChallengeKind;
  name: string;
  status: ChallengeStatus;
  xp: number;
  latitude: number;
  longitude: number;
  // One line the admin list shows under the name, different per kind.
  summary: string;
  updatedAt: string;
}

export const challengeKindLabels: Record<ChallengeKind, string> = {
  quiz: 'Kviz',
  social: 'Društveni',
  location: 'Lokacijski',
};

export const challenges: ProtoChallenge[] = [
  {
    id: 'q1',
    kind: 'quiz',
    name: 'Ko je podigao Gradsku kuću?',
    status: 'Published',
    xp: 120,
    latitude: 45.2553,
    longitude: 19.8449,
    summary: '5 pitanja · opseg 50 m',
    updatedAt: '18.11.2026.',
  },
  {
    id: 'q2',
    kind: 'quiz',
    name: 'Kviz o tvrđavi',
    status: 'Draft',
    xp: 80,
    latitude: 45.2517,
    longitude: 19.8618,
    summary: '3 pitanja · opseg 80 m',
    updatedAt: '20.11.2026.',
  },
  {
    id: 'q3',
    kind: 'quiz',
    name: 'Mostovi Novog Sada',
    status: 'Retired',
    xp: 60,
    latitude: 45.2405,
    longitude: 19.8401,
    summary: '4 pitanja · opseg 60 m',
    updatedAt: '02.11.2026.',
  },
  {
    id: 's1',
    kind: 'social',
    name: 'Okupljanje na Trgu',
    status: 'Published',
    xp: 200,
    latitude: 45.2549,
    longitude: 19.8458,
    summary: '22.–24.11. · 4 učesnika · opseg 30 m',
    updatedAt: '19.11.2026.',
  },
  {
    id: 's2',
    kind: 'social',
    name: 'Zalazak sa bedema',
    status: 'Draft',
    xp: 150,
    latitude: 45.2512,
    longitude: 19.8635,
    summary: '28.11. · 6 učesnika · opseg 40 m',
    updatedAt: '20.11.2026.',
  },
  {
    id: 's3',
    kind: 'social',
    name: 'Jesenji maraton',
    status: 'Archived',
    xp: 300,
    latitude: 45.2441,
    longitude: 19.8539,
    summary: '01.10. · 10 učesnika · opseg 100 m',
    updatedAt: '05.10.2026.',
  },
  {
    id: 'l1',
    kind: 'location',
    name: 'Skriveni detalji Dunavske',
    status: 'Published',
    xp: 90,
    latitude: 45.2562,
    longitude: 19.8508,
    summary: '3 skrivene tačke',
    updatedAt: '18.11.2026.',
  },
  {
    id: 'l2',
    kind: 'location',
    name: 'Fotografije sa Limana',
    status: 'Published',
    xp: 70,
    latitude: 45.2457,
    longitude: 19.8452,
    summary: '2 skrivene tačke',
    updatedAt: '17.11.2026.',
  },
];

// ---- Scenario S2: cart and payment ----

export const walletBalance = 120;

// The tours in the cart: their ids in `tours`.
export const cartTourIds = ['fruska-gora', 'tvrdjava'];

// The one coupon the prototype accepts; any other code is rejected.
export const validCoupon = {
  code: 'JESEN20',
  discountPercent: 20,
  tourId: 'tvrdjava',
  author: 'Ana Petrović',
};

export const availableGiftCard = {
  amount: 30,
  validUntil: '31.12.2026.',
  giver: 'jelena@explorer.dev',
};

// ---- Scenario S3: journeys and the active tour ----

// Tours the tourist owns and has not started yet.
export const readyTourIds = ['fruska-gora', 'tvrdjava', 'stari-grad'];

// The tour in progress and how far the tourist got.
export const activeTourId = 'liman-kej';
export const activeReachedCount = 1;

// A secret per key point, revealed when the tourist reaches it; same order as keyPoints.
export const secrets: Record<string, string[]> = {
  'liman-kej': [
    'Pesak na Štrandu je dovezen 1911. godine sa Dunava, a plaža je prvo bila samo za žene.',
    'Na keju svake godine u januaru gori 1.300 sveća, po jedna za svaku žrtvu racije.',
    'Most slobode je srušen 1999. i ponovo otvoren 2005; noću ga osvetljava 200 reflektora.',
  ],
  'stari-grad': [
    'Katedrala zapravo nije katedrala, već župna crkva; „katedrala“ je samo nadimak.',
    'Vladičanski dvor je obložen keramikom sa motivima iz srpske srednjovekovne umetnosti.',
    'Posle poplave 1876. ulica je podignuta skoro za metar; stari pragovi su ispod pločnika.',
    'Jezero u parku je ostatak rukavca Dunava, a u njemu žive kornjače koje niko nije uneo.',
  ],
  detelinara: ['Prvi mural je naslikan 2012. preko noći, bez dozvole, a danas je zaštićen.'],
};

export type ExecutionOutcome = 'Completed' | 'Abandoned';

export interface ProtoExecution {
  tourId: string;
  outcome: ExecutionOutcome;
  date: string;
  reached: number;
}

export const history: ProtoExecution[] = [
  { tourId: 'stari-grad', outcome: 'Completed', date: '15.11.2026.', reached: 4 },
  { tourId: 'detelinara', outcome: 'Abandoned', date: '09.11.2026.', reached: 1 },
];

// ---- Scenario S4: challenges as the player sees them ----

// Where the simulated tourist stands when the challenges screen opens: Trg slobode, a little off.
export const playerPosition = { latitude: 45.2541, longitude: 19.8432 };
export const playerXp = 1240;

export type PlayStatus = 'new' | 'started' | 'solved';

export interface PlayChallenge {
  id: string;
  kind: ChallengeKind;
  name: string;
  description: string;
  xp: number;
  latitude: number;
  longitude: number;
  rangeMeters: number;
  status: PlayStatus;
  earnedXp: number | null;
  // Quiz: how many questions and how many were answered.
  questions?: number;
  answered?: number;
  // Social: how many players must be there at once, and the hours of the day it is active.
  participants?: number;
  activeHours?: string;
  completionMessage?: string;
  // Location: one photo per hidden point, and the search circle drawn after activation
  // (the farthest point plus a random 0–100 m). rangeMeters is the 50 m around the start.
  photos?: string[];
  searchRadius?: number;
}

export const playChallenges: PlayChallenge[] = [
  {
    id: 'q1',
    kind: 'quiz',
    name: 'Ko je podigao Gradsku kuću?',
    description: 'Pet pitanja o zgradama oko trga. Odgovore tražite na fasadama.',
    xp: 120,
    latitude: 45.2553,
    longitude: 19.8449,
    rangeMeters: 50,
    status: 'new',
    earnedXp: null,
    questions: 5,
    answered: 0,
  },
  {
    id: 'q4',
    kind: 'quiz',
    name: 'Tajne Katedrale',
    description: 'Četiri pitanja o crkvi imena Marije.',
    xp: 100,
    latitude: 45.2556,
    longitude: 19.8459,
    rangeMeters: 50,
    status: 'started',
    earnedXp: null,
    questions: 4,
    answered: 2,
  },
  {
    id: 's1',
    kind: 'social',
    name: 'Okupljanje na Trgu',
    description: 'Četiri igrača moraju biti na trgu u isto vreme.',
    xp: 200,
    latitude: 45.2549,
    longitude: 19.8458,
    rangeMeters: 30,
    status: 'new',
    earnedXp: null,
    participants: 4,
    activeHours: '18:00–21:00',
    completionMessage: 'Uspeli ste! Trg je vaš — ovde se 1848. okupilo deset hiljada ljudi.',
  },
  {
    id: 'l1',
    kind: 'location',
    name: 'Skriveni detalji Dunavske',
    description: 'Tri ugla Dunavske ulice snimljena izbliza. Krenite od početka ulice.',
    xp: 90,
    latitude: 45.2562,
    longitude: 19.8508,
    rangeMeters: 50,
    searchRadius: 180,
    status: 'new',
    earnedXp: null,
    photos: [
      '/images/ilustracije/destination.svg',
      '/images/ilustracije/finding-the-way.svg',
      '/images/ilustracije/travel-destination.svg',
    ],
  },
  {
    id: 'l2',
    kind: 'location',
    name: 'Fotografije sa Limana',
    description: 'Dva mesta uz kej.',
    xp: 70,
    latitude: 45.2457,
    longitude: 19.8452,
    rangeMeters: 50,
    searchRadius: 140,
    status: 'solved',
    earnedXp: 70,
    photos: ['/images/ilustracije/hiking.svg', '/images/ilustracije/exploring.svg'],
  },
];

// ---- Scenario S6: the author's studio ----

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export interface AuthorTour {
  id: string;
  name: string;
  description: string;
  difficulty: Difficulty;
  tags: string[];
  groupMin: number;
  groupMax: number;
  status: TourStatus;
  price: number | null;
  times: Partial<Record<Transport, number>>;
  keyPoints: ProtoKeyPoint[];
  image: string;
  sold: number;
  changedAt: string;
}

const karlovci = tours.find((tour) => tour.id === 'sremski-karlovci')!;

export const authorTours: AuthorTour[] = [
  {
    id: 'dunavski-mostovi',
    name: 'Mostovi preko Dunava',
    description: 'Šetnja od Varadinskog mosta do Mosta slobode, uz priče o rušenju i obnovi.',
    difficulty: 'Easy',
    tags: ['istorija', 'reka'],
    groupMin: 1,
    groupMax: 10,
    status: 'Draft',
    price: null,
    times: {},
    keyPoints: [
      {
        name: 'Varadinski most',
        description: 'Najnoviji most u gradu, otvoren 2018.',
        latitude: 45.2531,
        longitude: 19.8589,
        image: '/images/ilustracije/destination.svg',
        isOpen: true,
      },
    ],
    image: '/images/ilustracije/finding-the-way.svg',
    sold: 0,
    changedAt: '20.11.2026.',
  },
  {
    id: karlovci.id,
    name: karlovci.name,
    description: karlovci.description,
    difficulty: karlovci.difficulty,
    tags: karlovci.tags,
    groupMin: karlovci.groupMin,
    groupMax: karlovci.groupMax,
    status: 'Published',
    price: karlovci.price,
    times: karlovci.times,
    keyPoints: karlovci.keyPoints,
    image: karlovci.image,
    sold: 14,
    changedAt: '12.11.2026.',
  },
  {
    id: 'zimska-tara',
    name: 'Zimska Tara',
    description:
      'Sezonska tura kroz Nacionalni park Tara, sa vidikovcima Banjska stena i Crnjeskovo.',
    difficulty: 'Hard',
    tags: ['priroda', 'zima'],
    groupMin: 2,
    groupMax: 6,
    status: 'Archived',
    price: 80,
    times: { Car: 180, Walking: 360 },
    keyPoints: [],
    image: '/images/ilustracije/adventure-map.svg',
    sold: 6,
    changedAt: '01.04.2026.',
  },
];

// "1 tačka", "3 tačke", "5 tačaka": Serbian picks the noun form by the number.
export function pointsText(count: number): string {
  const lastTwo = count % 100;
  const last = count % 10;
  if (last === 1 && lastTwo !== 11) {
    return `${count} tačka`;
  }
  if (last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)) {
    return `${count} tačke`;
  }
  return `${count} tačaka`;
}

// ---- Scenario S5: wallet and gift cards ----

export type TransactionKind = 'Credit' | 'Purchase' | 'GiftCard';

export interface ProtoTransaction {
  id: string;
  kind: TransactionKind;
  amount: number;
  date: string;
  time: string;
  // What the line is about: the tours bought, or who got the gift card.
  detail: string;
  cancelled: boolean;
}

export const transactions: ProtoTransaction[] = [
  {
    id: 't6',
    kind: 'Purchase',
    amount: 15,
    date: '20.11.2026.',
    time: '17:42',
    detail: 'Kej i Liman biciklom',
    cancelled: false,
  },
  {
    id: 't5',
    kind: 'GiftCard',
    amount: 50,
    date: '18.11.2026.',
    time: '10:05',
    detail: 'za ana@explorer.dev',
    cancelled: false,
  },
  {
    id: 't4',
    kind: 'Credit',
    amount: 100,
    date: '18.11.2026.',
    time: '09:30',
    detail: 'od administratora',
    cancelled: false,
  },
  {
    id: 't3',
    kind: 'Credit',
    amount: 500,
    date: '17.11.2026.',
    time: '14:11',
    detail: 'od administratora — greškom, stornirano',
    cancelled: true,
  },
  {
    id: 't2',
    kind: 'Purchase',
    amount: 0,
    date: '15.11.2026.',
    time: '11:20',
    detail: 'Stari grad Novog Sada',
    cancelled: false,
  },
  {
    id: 't1',
    kind: 'Credit',
    amount: 85,
    date: '14.11.2026.',
    time: '08:00',
    detail: 'od administratora',
    cancelled: false,
  },
];

export type GiftCardStatus = 'Valid' | 'Redeemed' | 'Expired';

export interface ProtoGiftCard {
  id: string;
  person: string;
  amount: number;
  message: string;
  createdAt: string;
  validUntil: string;
  status: GiftCardStatus;
}

// Gift card rules from Payment.Domain: 10–1000 EC, a message of at most 200 characters,
// valid three years, never to yourself, paid from the wallet, used whole at checkout.
export const giftCardRules = { min: 10, max: 1000, maxMessage: 200 };

export const receivedCards: ProtoGiftCard[] = [
  {
    id: 'g1',
    person: 'jelena@explorer.dev',
    amount: 30,
    message: 'Srećan rođendan! Prošetaj tvrđavom umesto mene :)',
    createdAt: '10.11.2026.',
    validUntil: '10.11.2029.',
    status: 'Valid',
  },
  {
    id: 'g0',
    person: 'mama@explorer.dev',
    amount: 20,
    message: 'Za prvu turu.',
    createdAt: '01.09.2026.',
    validUntil: '01.09.2029.',
    status: 'Redeemed',
  },
];

export const sentCards: ProtoGiftCard[] = [
  {
    id: 'g2',
    person: 'ana@explorer.dev',
    amount: 50,
    message: 'Hvala za pomoć oko selidbe!',
    createdAt: '18.11.2026.',
    validUntil: '18.11.2029.',
    status: 'Valid',
  },
];

// ---- Scenario S8: the administrator's wallets ----

export interface ProtoExplorer {
  id: string;
  email: string;
  balance: number;
  // The prototype only: when the account was made and when it last got EC (not in the API yet).
  registered: string;
  lastCredit: string | null;
}

export const explorers: ProtoExplorer[] = [
  {
    id: 'u1',
    email: 'marko@explorer.dev',
    balance: 120,
    registered: '01.11.2026.',
    lastCredit: '18.11.2026.',
  },
  {
    id: 'u2',
    email: 'ana@explorer.dev',
    balance: 340,
    registered: '02.11.2026.',
    lastCredit: '15.11.2026.',
  },
  {
    id: 'u3',
    email: 'jelena@explorer.dev',
    balance: 15,
    registered: '05.11.2026.',
    lastCredit: '06.11.2026.',
  },
  { id: 'u4', email: 'tester1@gmail.com', balance: 0, registered: '21.11.2026.', lastCredit: null },
  { id: 'u5', email: 'tester2@gmail.com', balance: 0, registered: '21.11.2026.', lastCredit: null },
  {
    id: 'u6',
    email: 'nikola.p@uns.ac.rs',
    balance: 0,
    registered: '20.11.2026.',
    lastCredit: null,
  },
  {
    id: 'u7',
    email: 'explorer1@explorer.dev',
    balance: 1000,
    registered: '25.08.2026.',
    lastCredit: '10.11.2026.',
  },
  {
    id: 'u8',
    email: 'explorer2@explorer.dev',
    balance: 55,
    registered: '25.08.2026.',
    lastCredit: '01.11.2026.',
  },
];

// ---- S4: solving a quiz (challenge q1) ----

export interface ProtoQuestion {
  text: string;
  options: string[];
  // Indexes of the correct options; a question may have more than one.
  correct: number[];
  xp: number;
}

export const quizQuestions: ProtoQuestion[] = [
  {
    text: 'Koje godine je završena Gradska kuća?',
    options: ['1848.', '1895.', '1918.', '1931.'],
    correct: [1],
    xp: 20,
  },
  {
    text: 'Koje figure se nalaze na fasadi Gradske kuće?',
    options: ['Alegorijske ženske figure', 'Konjanici', 'Lavovi', 'Grbovi gradova'],
    correct: [0, 3],
    xp: 30,
  },
  {
    text: 'Šta se nalazi u kuli Gradske kuće?',
    options: ['Sat', 'Zvono', 'Vidikovac za posetioce', 'Gradska biblioteka'],
    correct: [0, 1],
    xp: 25,
  },
  {
    text: 'Ko je projektovao zgradu?',
    options: ['Đorđe Tabaković', 'Đorđe Molnar', 'Nikola Tesla'],
    correct: [1],
    xp: 20,
  },
  {
    text: 'Koje zgrade gledaju na Trg slobode?',
    options: ['Katedrala', 'Gradska kuća', 'Hotel Vojvodina', 'Petrovaradinska tvrđava'],
    correct: [0, 1, 2],
    xp: 25,
  },
];

// The server's rule (Question.Score): each ticked correct option gives xp / correct count,
// each ticked wrong one takes xp / 4, never below zero.
export function questionScore(question: ProtoQuestion, selected: number[]): number {
  const share = question.xp / question.correct.length;
  const score = selected.reduce(
    (sum, option) => sum + (question.correct.includes(option) ? share : -question.xp / 4),
    0,
  );
  return Math.max(0, score);
}

// "37,5" rather than "37.50".
export function xpText(xp: number): string {
  return String(Math.round(xp * 10) / 10).replace('.', ',');
}
