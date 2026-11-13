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
}

export interface SocialChallengeDto extends SocialChallengeDetailsDto {
  id: string;
  status: SocialChallengeStatus;
}
