import { useGameStore } from '@/stores/useGameStore';
import { useRef, useEffect, useState } from 'react';
import { signalRService } from '@/services/signalRService';

const MessageSidebar = () => {
  const messages = useGameStore((s) => s.statusMessages);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  return (
    <div className="w-full lg:w-80 border-t lg:border-t-0 lg:border-l border-border bg-card/30 backdrop-blur-sm flex flex-col">
      <div className="p-4 border-b border-border">
        <h3 className="font-display text-xs uppercase tracking-[0.2em] text-primary">
          System Log
        </h3>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-2 max-h-48 lg:max-h-none">
        {messages.length === 0 ? (
          <p className="text-xs text-muted-foreground/50 italic">Awaiting transmissions...</p>
        ) : (
          messages.map((msg, i) => (
            <div
              key={i}
              className="text-xs text-muted-foreground py-1.5 px-3 rounded bg-muted/60 border border-border/50 animate-fade-in"
            >
              {msg}
            </div>
          ))
        )}
        <div ref={bottomRef} />
      </div>

      {/* chat input */}
      <ChatInput />
    </div>
  );
};



// extract chat form into its own inner component to keep sidebar tidy
const ChatInput = () => {
  const matchId = useGameStore((s) => s.matchId);
  const [chatInput, setChatInput] = useState('');

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!chatInput.trim() || !matchId) return;

    try {
      await signalRService.sendChatMessage(matchId, chatInput);
      setChatInput('');
    } catch (err) {
      console.error('Chat failed:', err);
    }
  };

  return (
    <div className="p-4 border-t border-border mt-auto">
      <form onSubmit={handleSendMessage} className="flex gap-2">
        <input
          type="text"
          value={chatInput}
          onChange={(e) => setChatInput(e.target.value)}
          placeholder="Send a message..."
          disabled={!matchId}
          className="flex-1 bg-background border border-border rounded px-3 py-2 text-xs text-foreground focus:outline-none focus:border-primary disabled:opacity-50"
        />
        <button
          type="submit"
          disabled={!matchId || !chatInput.trim()}
          className="bg-primary/20 text-primary border border-primary/50 hover:bg-primary/30 px-3 py-2 rounded text-xs font-bold transition-colors disabled:opacity-50"
        >
          SEND
        </button>
      </form>
    </div>
  );
};

export default MessageSidebar;
