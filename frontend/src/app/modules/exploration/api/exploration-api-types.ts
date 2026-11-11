export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published';

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
  transportTimes: TransportTimeDto[];
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
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
  difficulty: TourDifficulty | null;
  tags: string[] | null;
  transport: TransportMode | null;
  sortByDifficulty: DifficultySort | null;
}
