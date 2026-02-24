import { useGameStore } from '@/stores/useGameStore';
import { useAuthStore } from '@/stores/useAuthStore';
import { signalRService } from '@/services/signalRService';
import MessageSidebar from './MessageSidebar';
import WinnerOverlay from './WinnerOverlay';

const GameView = () => {
  const board = useGameStore((s) => s.board);
  const currentTurnId = useGameStore((s) => s.currentTurnId);
  const isGameOver = useGameStore((s) => s.isGameOver);
  const matchId = useGameStore((s) => s.matchId);
  const userId = useAuthStore((s) => s.user?.id || '');
  const isMyTurn = currentTurnId === userId && !isGameOver;

  const handleCellClick = async (index: number) => {
    if (!isMyTurn || board[index] !== null || !matchId) return;
    try {
      await signalRService.makeMove({ cellIndex: index }, matchId);
    } catch (err) {
      console.error('Move failed:', err);
    }
  };

  const renderCell = (value: string | null, index: number) => {
    const isClickable = isMyTurn && value === null;
    return (
      <button
        key={index}
        onClick={() => handleCellClick(index)}
        disabled={!isClickable}
        className={`
          aspect-square flex items-center justify-center text-4xl sm:text-5xl font-display font-bold
          border border-border/50 rounded-lg transition-all duration-300
          ${isClickable ? 'cell-hover cursor-pointer' : 'cursor-default'}
          ${value === 'X' ? 'text-primary glow-text-cyan' : ''}
          ${value === 'O' ? 'text-secondary glow-text-emerald' : ''}
          ${!value ? 'text-transparent' : ''}
          bg-card/30
        `}
      >
        {value || '·'}
      </button>
    );
  };
console.log('Server Turn ID:', currentTurnId, '| My Frontend ID:', userId);
  return (
    <div className="min-h-screen flex flex-col lg:flex-row">
      <WinnerOverlay />

      {/* Main game area */}
      <div className="flex-1 flex flex-col items-center justify-center p-4 sm:p-8">
        {/* Turn indicator */}
        <div className="mb-6 text-center animate-fade-in">
          <p className="font-display text-xs uppercase tracking-[0.3em] text-muted-foreground mb-1">
            {isGameOver ? 'Game Over' : isMyTurn ? 'Your Turn' : "Opponent's Turn"}
          </p>
          <div className={`w-16 h-0.5 mx-auto transition-all duration-500 ${
            isMyTurn ? 'bg-primary' : 'bg-muted-foreground/30'
          }`} />
        </div>

        {/* Board */}
        <div
          className={`grid grid-cols-3 gap-2 w-full max-w-xs sm:max-w-sm p-4 rounded-xl transition-all duration-700 ${
            isMyTurn ? 'board-glow-active' : 'board-glow-inactive'
          }`}
        >
          {board.map((cell, i) => renderCell(cell, i))}
        </div>

        {/* Match ID */}
        <p className="mt-6 text-xs text-muted-foreground/50 font-mono">
          Match: {matchId?.slice(0, 8) || '—'}
        </p>
      </div>

      {/* Sidebar */}
      <MessageSidebar />
    </div>
  );
};

export default GameView;
