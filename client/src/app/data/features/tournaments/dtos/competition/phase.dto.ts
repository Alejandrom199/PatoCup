export interface PhaseResponseDto {
    id: number;
    name: string;
    tournamentId: number;
    tournamentName: string;
    phaseStateId: number;
    phaseStateName: string;
    stateId: number;
    stateName: string;
    sequence: number;
}

export interface CreatePhaseDto {
    tournamentId: number;
    name: string;
}

export interface UpdatePhaseDto {
    id: number;
    name: string;
    phaseStateId: number;
    stateId: number;
    sequence: number;
}
