export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface ChallengeDto {
  id: string;
  name: string;
  status: QuizStatus;
  totalXp: number;
}

export interface AnswerOptionDto {
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  id: string;
  text: string;
  xpReward: number;
  options: AnswerOptionDto[];
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string;
  status: QuizStatus;
  questions: QuestionDto[];
  latitude: number;
  longitude: number;
  rangeInMeters: number;
}

export interface QuestionContentDto {
  text: string;
  xpReward: number;
  options: AnswerOptionDto[];
}

export interface QuizContentDto {
  name: string;
  instructions: string;
  questions: QuestionContentDto[];
  latitude: number;
  longitude: number;
  rangeInMeters: number;
}

export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface SocialChallengeDetailsDto {
  name: string;
  start: string;
  end: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xpReward: number;
  completionMessage: string;
  latitude: number | null;
  longitude: number | null;
}

export interface SocialChallengeDto extends SocialChallengeDetailsDto {
  id: string;
  status: SocialChallengeStatus;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  startLatitude: number;
  startLongitude: number;
  xpReward: number;
  pointCount: number;
}

export interface LocationChallengeDetailsDto {
  challenge: LocationChallengeDto;
  points: HiddenLocationDto[];
}

export interface HiddenLocationDto {
  id: string;
  latitude: number;
  longitude: number;
  imageAddress: string;
}

export interface LocationChallengeContentDto {
  name: string;
  startLatitude: number;
  startLongitude: number;
  xpReward: number;
  points: HiddenLocationContentDto[];
}

export interface HiddenLocationContentDto {
  latitude: number | null;
  longitude: number | null;
  image: File;
}

export interface CheckLocationDto {
  latitude: number | null;
  longitude: number | null;
}

export interface LocationChallengePlayDto {
  challenge: LocationChallengeDto;
  attempt: LocationChallengeProgressDto | null;
}

export interface LocationChallengeProgressDto {
  radiusInMeters: number;
  completedAt: string | null;
  awardedXp: number;
  points: LocationChallengePointProgressDto[];
}

export interface LocationChallengePointProgressDto {
  id: string;
  discovered: boolean;
  imageAddress: string;
}

export type QuizProgress = 'NotStarted' | 'Started' | 'Solved';

export interface QuizMapItemDto {
  id: string;
  name: string;
  instructions: string;
  latitude: number;
  longitude: number;
  progress: QuizProgress;
  earnedXp: number | null;
}

export interface QuizAttemptDto {
  quizId: string;
  name: string;
  instructions: string;
  answeredCount: number;
  questionCount: number;
  currentQuestion: CurrentQuestionDto | null;
  result: QuizResultDto | null;
}

export interface CurrentQuestionDto {
  id: string;
  text: string;
  options: string[];
}

export interface QuizResultDto {
  earnedXp: number;
  totalXp: number;
  percentage: number;
}

export interface QuizPositionDto {
  latitude: number;
  longitude: number;
}

export interface AnswerDto {
  selectedOptions: number[];
}

export interface SocialChallengeMapItemDto {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  xpReward: number;
  solved: boolean;
  awardedXp: number | null;
}

export interface SocialChallengeRunDto {
  isParticipant: boolean;
  activeParticipantCount: number;
  requiredParticipants: number;
  completionMessage: string | null;
  awardedXp: number | null;
}

export interface SocialChallengePositionDto {
  latitude: number;
  longitude: number;
}
