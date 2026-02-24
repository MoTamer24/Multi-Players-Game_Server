import { create } from 'zustand';

interface GameStoreState {
  matchId: string | null;
  board: (string | null)[];
  currentTurnId: string;
  isGameOver: boolean;
  winnerId: string | null;
  isSearching: boolean;
  statusMessages: string[];
  showWinnerOverlay: boolean;

  setMatch: (matchId: string, board: (string | null)[], turnId: string) => void;
  updateBoard: (board: (string | null)[], turnId: string) => void;
  setGameOver: (winnerId: string | null) => void;
  addMessage: (message: string) => void;
  setSearching: (searching: boolean) => void;
  setShowWinnerOverlay: (show: boolean) => void;
  resetGame: () => void;
}

export const useGameStore = create<GameStoreState>((set) => ({
  matchId: null,
  board: Array(9).fill(null),
  currentTurnId: '',
  isGameOver: false,
  winnerId: null,
  isSearching: false,
  statusMessages: [],
  showWinnerOverlay: false,

  setMatch: (matchId, board, turnId) =>
    set({ matchId, board, currentTurnId: turnId, isGameOver: false, winnerId: null, isSearching: false, statusMessages: [] }),

  updateBoard: (board, turnId) =>
    set({ board, currentTurnId: turnId }),

  setGameOver: (winnerId) =>
    set({ isGameOver: true, winnerId, showWinnerOverlay: true }),

  addMessage: (message) =>
    set((state) => ({ statusMessages: [...state.statusMessages, message] })),

  setSearching: (searching) =>
    set({ isSearching: searching }),

  setShowWinnerOverlay: (show) =>
    set({ showWinnerOverlay: show }),

  resetGame: () =>
    set({
      matchId: null,
      board: Array(9).fill(null),
      currentTurnId: '',
      isGameOver: false,
      winnerId: null,
      isSearching: false,
      statusMessages: [],
      showWinnerOverlay: false,
    }),
}));
