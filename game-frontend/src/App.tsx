import React from "react";
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound";
import { signalRService } from "./services/signalRService";
import { useAuthStore } from "./stores/useAuthStore";


const queryClient = new QueryClient();

const App = () => {
  // when the component mounts we may already have tokens in storage (persisted by zustand)
  // if so make sure signalR is started and auth state is marked.
  React.useEffect(() => {
    const { accessToken, isAuthenticated } = useAuthStore.getState();
    if (accessToken && !signalRService['connection']) {
      // start with the stored token; we don't await because App render isn't blocked
      signalRService.start(accessToken).catch((err) => {
        console.error('SignalR auto‑start failed', err);
      });
    }
    // if we have a token but isn’t marked authenticated, set the flag
    if (accessToken && !isAuthenticated) {
      useAuthStore.getState().updateTokens(accessToken, useAuthStore.getState().refreshToken || '');
      // we don't know user details; leave as-is –  login screen will still render if user null
    }
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Index />} />
            {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  );
};

export default App;
