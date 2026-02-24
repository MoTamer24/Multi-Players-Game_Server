import { useGameStore } from '@/stores/useGameStore';
import { signalRService } from '@/services/signalRService';
import { useAuthStore } from '@/stores/useAuthStore';

const LobbyView = () => {
  const isSearching = useGameStore((s) => s.isSearching);
  const setSearching = useGameStore((s) => s.setSearching);
  const userName = useAuthStore((s) => s.user?.name || 'Pilot');
  const logout = useAuthStore((s) => s.logout);

  const handleFindMatch = async () => {
    setSearching(true);
    try {
      await signalRService.findMatch(0);
    } catch {
      setSearching(false);
    }
  };

  const handleCancel = () => {
    setSearching(false);
  };

  return (
    <div className="flex items-center justify-center min-h-screen p-4">
      {/* Background grid */}
      <div className="fixed inset-0 opacity-5 pointer-events-none"
        style={{
          backgroundImage: `linear-gradient(hsl(var(--cyan) / 0.3) 1px, transparent 1px),
                            linear-gradient(90deg, hsl(var(--cyan) / 0.3) 1px, transparent 1px)`,
          backgroundSize: '60px 60px',
        }}
      />

      <div className="glass-card p-8 sm:p-12 w-full max-w-lg animate-fade-in relative z-10 text-center">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="text-left">
            <p className="text-xs font-display uppercase tracking-wider text-muted-foreground">Pilot</p>
            <p className="text-primary font-display font-semibold glow-text-cyan">{userName}</p>
          </div>
          <button
            onClick={logout}
            className="text-xs text-muted-foreground hover:text-destructive transition-colors uppercase tracking-wider"
          >
            Disconnect
          </button>
        </div>

        <div className="w-full h-px bg-border mb-8" />

        {isSearching ? (
          <div className="space-y-8 animate-fade-in">
            {/* Radar Animation */}
            <div className="flex justify-center">
              <div className="radar-container">
                <div className="radar-ring" />
                <div className="radar-ring" />
                <div className="radar-ring" />
                <div className="radar-sweep" />
                <div className="absolute inset-0 flex items-center justify-center">
                  <div className="w-3 h-3 rounded-full bg-primary animate-pulse-glow" />
                </div>
              </div>
            </div>

            <div>
              <h2 className="font-display text-xl text-primary glow-text-cyan tracking-wider">
                SCANNING
              </h2>
              <p className="text-muted-foreground text-sm mt-2">
                Searching for an opponent...
              </p>
            </div>

            <button onClick={handleCancel} className="neon-button text-sm">
              Cancel Search
            </button>
          </div>
        ) : (
          <div className="space-y-8 animate-fade-in">
            <div>
              <h2 className="font-display text-2xl font-bold text-foreground tracking-wider mb-2">
                MISSION SELECT
              </h2>
              <p className="text-muted-foreground text-sm">
                Ready to enter the arena?
              </p>
            </div>

            <button onClick={handleFindMatch} className="neon-button w-full text-base">
              Find Match
            </button>

            <div className="grid grid-cols-3 gap-4 text-center">
              <div>
                <p className="font-display text-lg text-primary">0</p>
                <p className="text-xs text-muted-foreground uppercase">Wins</p>
              </div>
              <div>
                <p className="font-display text-lg text-secondary">0</p>
                <p className="text-xs text-muted-foreground uppercase">Games</p>
              </div>
              <div>
                <p className="font-display text-lg text-foreground">—</p>
                <p className="text-xs text-muted-foreground uppercase">Rank</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default LobbyView;
