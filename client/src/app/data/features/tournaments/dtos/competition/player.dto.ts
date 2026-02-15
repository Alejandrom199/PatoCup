export interface PlayerSelectDto {
  id: number;
  nickname: string; 
}

export interface PlayerResponseDto {
  id: number;
  nickname: string;
  gameId: string;
  registrationIp: string;
  createdAt: Date;

  playerStateId: number;
  playerStateName: string;

  stateId: number;
  stateName: string;
}

export interface PublicSubmitPlayerDto {
  Nickname: string;
  gameId?: string;
}

export interface UpdatePlayerDto {
  id: number;
  nickname: string;
  gameId?: string;
  stateId: number;
}