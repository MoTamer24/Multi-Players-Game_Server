import { useGameStore } from '@/stores/useGameStore';
import { useRef, useEffect } from 'react';

const MessageSidebar = () => {
  const messages = useGameStore((s) => s.statusMessages);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  return (
    <div className="w-full lg:w-80 border-t lg:border-t-0 lg:border-l border-border bg-card/30 backdrop-blur-sm flex flex-col">
      <div className="p-4 border-b border-border">
        <h3 className="font-display text-xs uppercase tracking-[0.2em] text-muted-foreground">
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
              className="text-xs text-muted-foreground py-1.5 px-3 rounded bg-muted/30 border border-border/50 animate-fade-in"
            >
              {msg}
            </div>
          ))
        )}
        <div ref={bottomRef} />
      </div>
    </div>
  );
};

export default MessageSidebar;
