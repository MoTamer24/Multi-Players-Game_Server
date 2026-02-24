export interface User {
  id: string;
  name: string;
  token: string;
}

export interface GameState {
  board: (string | null)[];
  currentTurnId: string;
  isGameOver: boolean;
  winnerId: string | null;
}

export interface MoveData {
  cellIndex: number;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export type GameView = 'login' | 'lobby' | 'game';
