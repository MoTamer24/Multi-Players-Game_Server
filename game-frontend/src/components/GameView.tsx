import { useGameStore } from '@/stores/useGameStore';
import { useAuthStore } from '@/stores/useAuthStore';
import { signalRService } from '@/services/signalRService';
import MessageSidebar from './MessageSidebar';
import WinnerOverlay from './WinnerOverlay';

const GameView = () => {
  const board = useGameStore((s) => s.board);
  const currentTurnId = useGameStore((s) => s.currentTurnId);
  const matchId = useGameStore((s) => s.matchId);
  const isGameOver = useGameStore((s) => s.isGameOver);
  const userId = useAuthStore((s) => s.user?.id || '');

  const handleCellClick = async (idx: number) => { 
    if (!matchId || isGameOver) return;
    if (currentTurnId !== userId) return; // not your turn

    try {
      await signalRService.makeMove({ cellIndex: idx }, matchId);
    } catch (err) {
      console.error('Move failed', err);
    }
  };

  return (
    <div className="flex flex-col min-h-screen h-full bg-gradient-to-br from-slate-900 to-slate-800 text-white">
      <header className="py-4 text-center font-display text-3xl tracking-wide text-primary">Tic‑Tac‑Toe</header>
      <div className={`text-center text-sm mb-4 ${isGameOver
          ? 'text-destructive'
          : currentTurnId === userId
            ? 'text-primary'
            : 'text-muted-foreground'
        }`}>
        {isGameOver
          ? 'Game over'
          : currentTurnId === userId
            ? 'Your move'
            : "Opponent's turn"}
      </div>

      <div className="flex flex-1 items-center justify-center gap-6 px-4">
        {/* board container */}
        <div className="flex-1 flex items-center justify-center">
          <div className="grid grid-cols-3 gap-3 bg-card/40 p-4 rounded-md shadow-xl">
            {board.map((cell, i) => (
              <div
                key={i}
                className={`w-20 h-20 flex items-center justify-center text-4xl font-bold border border-primary cursor-pointer select-none transition-colors \
                  ${cell === 'X' ? 'text-cyan-400' : cell === 'O' ? 'text-pink-400' : 'text-foreground'} \
                  ${cell ? (cell === 'X' ? 'bg-cyan-900/50' : 'bg-pink-900/50') : ''} \
                  ${isGameOver || currentTurnId !== userId ? 'pointer-events-none opacity-70' : 'hover:bg-muted/20'}`}
                onClick={() => handleCellClick(i)}
              >
                {cell || ''}
              </div>
            ))}
          </div>
        </div>

        {/* right sidebar */}
        <div className="w-full lg:w-80">
          <MessageSidebar />
        </div>
      </div>

      <WinnerOverlay />
    </div>
  );
};

export default GameView;