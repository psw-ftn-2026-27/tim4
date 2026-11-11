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
