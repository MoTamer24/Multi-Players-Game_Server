import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/useAuthStore';
import { useGameStore } from '@/stores/useGameStore';
import { toast } from 'sonner';

const HUB_URL = 'http://localhost:5024/Hub';

class SignalRService {
  private static instance: SignalRService;
  private connection: signalR.HubConnection | null = null;

  private constructor() {}

  static getInstance(): SignalRService {
    if (!SignalRService.instance) {
      SignalRService.instance = new SignalRService();
    }
    return SignalRService.instance;
  }

async start(directToken?: string): Promise<void> {
  if (this.connection?.state === signalR.HubConnectionState.Connected) return;

  this.connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => {
        // 1. Use the directly passed token if available
        // 2. Fallback to Zustand state (for reconnects later)
        // 3. Fallback to empty string
        return directToken || useAuthStore.getState().accessToken || '';
      }
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  this.registerListeners();

  try {
    await this.connection.start();
    console.log('SignalR connected');
  } catch (err) {
    console.error('SignalR connection failed:', err);
  }
}

  private registerListeners(): void {
    if (!this.connection) return;

    this.connection.on('GameStarted', (matchId: string, board: any, turnId: string) => {
      useGameStore.getState().setMatch(matchId, board, turnId);
      useGameStore.getState().addMessage('🎮 Game started!');
    });

    this.connection.on('BoardUpdate', (board: any, turnId: string) => {
      useGameStore.getState().updateBoard(board, turnId);
    });

    this.connection.on('ReceiveMsg', (message: string) => {
      useGameStore.getState().addMessage(message);
    });

    this.connection.on('GameOver', (winnerId: string) => {
      useGameStore.getState().setGameOver(winnerId);
    });

    this.connection.on('OpponentDisconnected', (userId: string) => {
      toast.error('Opponent disconnected');
      useGameStore.getState().addMessage(`⚠️ Opponent (${userId}) disconnected`);
    });
    this.connection.on("ReceiveChat", (playerId, message) => {
    // Push this string to your Zustand store's statusMessages array
    useGameStore.getState().addMessage(`${playerId.slice(0, 4)}: ${message}`); 
});

this.connection.on("OpponentDisconnected", (userId) => {
    // Trigger game over in your store and show the win
    useGameStore.getState().handleOpponentDisconnect();
});
  }

  async findMatch(gameType: number): Promise<void> {
    await this.connection?.invoke('FindMatch', gameType);
  }

  async makeMove(moveData: object, matchId: string): Promise<void> {
    await this.connection?.invoke('MakeMove', moveData, matchId);
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
  }
  async sendChatMessage(matchId: string, chatInput: string): Promise<void> {
    // call hub method for chat rather than MakeMove
    await this.connection?.invoke('SendChatMessage', matchId, chatInput);
  }
}

export const signalRService = SignalRService.getInstance();
