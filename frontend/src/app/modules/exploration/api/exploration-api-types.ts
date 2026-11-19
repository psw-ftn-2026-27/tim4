export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  archivedAt: string | null;
  recommendedGroupSizeMin: number;
  recommendedGroupSizeMax: number;
  transportTimes: TransportTimeDto[];
  lengthInKm: number;
}

export interface KeyPointDto {
  id: string;
  name: string;
  description: string;
  imageAddress: string;
  secret: string | null;
  latitude: number;
  longitude: number;
  isVisibleWithoutPurchase: boolean;
}

export interface TourKeyPointDto {
  id: string;
  name: string;
  description: string;
  imageAddress: string;
  secret: string | null;
  latitude: number;
  longitude: number;
  isVisibleWithoutPurchase: boolean;
}

export interface TourDetailsDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  lengthInKm: number;
  transportTimes: TransportTimeDto[];
  keyPoints: TourKeyPointDto[];
  isPurchased: boolean;
}

export interface KeyPointDetailsDto {
  name: string;
  description: string;
  secret: string | null;
  latitude: number;
  longitude: number;
  isVisibleWithoutPurchase: boolean;
  tourLengthInKm: number;
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSizeMin: number;
  recommendedGroupSizeMax: number;
}

export type TourPricingType = 'Free' | 'Paid';

export interface PublishTourDto {
  pricingType: TourPricingType | null;
  pricingAmount: number | null;
}

export interface TourPreferenceDto {
  difficulties: TourDifficulty[];
  transport: TransportMode | null;
  tags: string[];
  openSearchWithPreferences: boolean;
}

export type TourSort =
  | 'DifficultyAscending'
  | 'DifficultyDescending'
  | 'PriceAscending'
  | 'PriceDescending';

export interface PublishedTourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transport: TransportMode | null;
  priceFrom: number | null;
  priceTo: number | null;
  lengthFrom: number | null;
  lengthTo: number | null;
  sort: TourSort | null;
  latitude: number | null;
  longitude: number | null;
  radiusInKm: number | null;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface PublishedTourDto {
  tour: TourDto;
  price: number | null;
  pinLocation: LocationDto | null;
}

export interface RecordLocationSearchDto {
  latitude: number;
  longitude: number;
  radiusInKm: number;
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transport: TransportMode | null;
  priceFrom: number | null;
  priceTo: number | null;
  lengthFrom: number | null;
  lengthTo: number | null;
}

export interface StartTourDto {
  tourId: string;
  latitude: number;
  longitude: number;
}

export interface ActiveKeyPointDto {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
}

export interface ActiveTourDto {
  executionId: string;
  tourId: string;
  tourName: string;
  keyPoints: ActiveKeyPointDto[];
  nextKeyPointId: string;
  reachedKeyPointCount: number;
}
