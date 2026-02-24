import { useGameStore } from '@/stores/useGameStore';
import { useAuthStore } from '@/stores/useAuthStore';

const WinnerOverlay = () => {
  const showWinnerOverlay = useGameStore((s) => s.showWinnerOverlay);
  const winnerId = useGameStore((s) => s.winnerId);
  const userId = useAuthStore((s) => s.user?.id || '');
  const resetGame = useGameStore((s) => s.resetGame);
  const setShowWinnerOverlay = useGameStore((s) => s.setShowWinnerOverlay);

  if (!showWinnerOverlay) return null;

  const isWinner = winnerId === userId;
  const isDraw = winnerId === null || winnerId === '';

  const handleBackToLobby = () => {
    setShowWinnerOverlay(false);
    resetGame();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-md animate-fade-in">
      <div className="glass-card p-8 sm:p-12 text-center max-w-sm mx-4 animate-scale-in">
        <div className={`font-display text-5xl sm:text-6xl font-bold mb-4 ${
          isDraw ? 'text-muted-foreground' : isWinner ? 'text-primary glow-text-cyan' : 'text-destructive'
        }`}>
          {isDraw ? 'DRAW' : isWinner ? 'VICTORY' : 'DEFEAT'}
        </div>

        <p className="text-muted-foreground text-sm mb-8">
          {isDraw
            ? 'No victor this round.'
            : isWinner
            ? 'You dominated the grid.'
            : 'Better luck next time, pilot.'}
        </p>

        <button onClick={handleBackToLobby} className="neon-button text-sm">
          Return to Lobby
        </button>
      </div>
    </div>
  );
};

export default WinnerOverlay;
