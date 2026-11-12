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

export type DifficultySort = 'Ascending' | 'Descending';

export interface PublishedTourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transport: TransportMode | null;
  sortByDifficulty: DifficultySort | null;
}
