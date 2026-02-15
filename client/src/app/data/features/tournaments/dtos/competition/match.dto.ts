export interface MatchResponseDto {
    id: number;
    phaseId: number;
    phaseName: string;
    player1Id: number;
    player1Name: string;
    player2Id: number;
    player2Name: string;
    scorePlayer1?: number;
    scorePlayer2?: number;
    winnerId?: string;
    winnerName?: string;
    matchStateId: number;
    matchStateName: string;
    stateId: number;
    stateName: string;
}

export interface CreateMatchDto {
    phaseId: number;
    player1Id: number;
    player2Id: number;
}

export interface UpdateMatchDto {
    id: number;
    player1Id: number;
    player2Id: number;
    matchStateId: number;
    stateId: number;
}

export interface RegisterResultDto {
    id: number;
    scorePlayer1: number;
    scorePlayer2: number;
    winnerId: number;
}