import { useState } from 'react';
import { useAuthStore } from '@/stores/useAuthStore';
import { authApi } from '@/services/api';
import { signalRService } from '@/services/signalRService';
import { toast } from 'sonner';
import {GoogleLogin} from '@react-oauth/google';

const LoginView = () => {
  const [guestName, setGuestName] = useState('');
  const [loading, setLoading] = useState(false);
  const setAuth = useAuthStore((s) => s.setAuth);

  const handleGuestLogin = async () => {
    if (!guestName.trim()) {
      toast.error('Enter a name, pilot.');
      return;
    }
    setLoading(true);
    try {
      const { data } = await authApi.guestLogin(guestName.trim());
      setAuth(
        { id: data.userId || 'guest', name: guestName.trim(), token: data.accessToken },
        data.accessToken,
        data.refreshToken
      );
      await signalRService.start(data.accessToken);
      toast.success('Connected to the grid.');
    } catch {
      toast.error('Connection failed. Server offline?');
    } finally {
      setLoading(false);
    }
  };

 const handleGoogleSuccess = async (credentialResponse: any) => {
    setLoading(true);
    try {
      // credentialResponse.credential contains the raw JWT id_token from Google
      const idToken = credentialResponse.credential; 
      
      const { data } = await authApi.googleLogin(idToken);
      
      setAuth(
        { id: data.userId, name: 'Pilot', token: data.accessToken }, // Adjust name extraction if you want it from Google
        data.accessToken,
        data.refreshToken
      );
      
      await signalRService.start(data.accessToken);
      toast.success('Connected via Google.');
    } catch (error) {
      console.error(error);
      toast.error('Google authentication rejected by server.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen p-4">
      {/* Background grid effect */}
      <div className="fixed inset-0 opacity-5 pointer-events-none"
        style={{
          backgroundImage: `linear-gradient(hsl(var(--cyan) / 0.3) 1px, transparent 1px),
                            linear-gradient(90deg, hsl(var(--cyan) / 0.3) 1px, transparent 1px)`,
          backgroundSize: '60px 60px',
        }}
      />

      <div className="glass-card p-8 sm:p-12 w-full max-w-md animate-scale-in relative z-10">
        {/* Logo / Title */}
        <div className="text-center mb-10">
          <h1 className="font-display text-3xl sm:text-4xl font-bold text-primary glow-text-cyan tracking-wider">
            NEXUS
          </h1>
          <p className="text-muted-foreground mt-2 text-sm tracking-widest uppercase">
            Multiplayer Arena
          </p>
          <div className="w-20 h-0.5 bg-primary/40 mx-auto mt-4" />
        </div>

        {/* Guest Login */}
        <div className="space-y-4">
          <label className="block text-xs font-display uppercase tracking-wider text-muted-foreground">
            Callsign
          </label>
          <input
            type="text"
            value={guestName}
            onChange={(e) => setGuestName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleGuestLogin()}
            placeholder="Enter your name..."
            className="w-full bg-muted/50 border border-border text-foreground rounded-lg px-4 py-3 
                       focus:outline-none focus:border-primary/50 focus:ring-1 focus:ring-primary/30
                       placeholder:text-muted-foreground/50 font-body transition-all duration-200"
            disabled={loading}
          />

          <button
            onClick={handleGuestLogin}
            disabled={loading}
            className="neon-button w-full text-sm disabled:opacity-50"
          >
            {loading ? (
              <span className="flex items-center justify-center gap-2">
                <span className="w-4 h-4 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
                Connecting...
              </span>
            ) : (
              'Enter as Guest'
            )}
          </button>
        </div>

        {/* Divider */}
        <div className="flex items-center gap-4 my-8">
          <div className="flex-1 h-px bg-border" />
          <span className="text-xs text-muted-foreground uppercase tracking-wider">or</span>
          <div className="flex-1 h-px bg-border" />
        </div>

        {/* Google Login */}



        {/* Google Login Component */}
        <div className="flex justify-center w-full">
          <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={() => {
              toast.error('Google Login Failed locally.');
            }}
            theme="filled_black" // Try to match your dark UI
            shape="rectangular"
            text="signin_with"
          />
          </div>

        
      </div>
    </div>
  );
};

export default LoginView;
