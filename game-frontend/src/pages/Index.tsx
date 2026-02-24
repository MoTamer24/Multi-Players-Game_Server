import { useAuthStore } from '@/stores/useAuthStore';
import { useGameStore } from '@/stores/useGameStore';
import LoginView from '@/components/LoginView';
import LobbyView from '@/components/LobbyView';
import GameView from '@/components/GameView';
import type { GameView as GameViewType } from '@/types';

const Index = () => {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const matchId = useGameStore((s) => s.matchId);

  const currentView: GameViewType = !isAuthenticated
    ? 'login'
    : matchId
    ? 'game'
    : 'lobby';

  return (
    <>
      {currentView === 'login' && <LoginView />}
      {currentView === 'lobby' && <LobbyView />}
      {currentView === 'game' && <GameView />}
    </>
  );
};

export default Index;
