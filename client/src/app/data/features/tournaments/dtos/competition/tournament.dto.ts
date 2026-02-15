import { paginationFilter } from "../common/pagination-filter.dto";

export interface CreateTournament {
  name: string;
  description: string;
  startDate: Date;
  endDate: Date;
}
export interface UpdateTournamentDto extends CreateTournament {
  id: number;
  tournamentStateId: number;
  stateId: number;
  isPublic?: boolean;
}

export interface TournamentQuery extends paginationFilter {
  name?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  tournamentStateId?: number;
}

export interface TournamentResponseDto {
  id: number;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  tournamentStateId: number;
  tournamentStateName: string;
  stateId: number;
  stateName: string;
  isPublic?: boolean;
}

export interface TournamentBracketDto {
    tournamentId: number;
    tournamentName: string;
    phases: PhaseBracketDto[];
}

export interface PhaseBracketDto {
    phaseId: number;
    phaseName: string;
    phaseOrder: number;
    matches: MatchBracketDto[];
}

export interface MatchBracketDto {
    matchId: number;
    player1Name: string;
    player2Name: string;
    scorePlayer1: number | null;
    scorePlayer2: number | null;
    isFinal: boolean;
}

