import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { GoogleOAuthProvider } from '@react-oauth/google';

const googleclientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

createRoot(document.getElementById("root")!).render(
    <GoogleOAuthProvider clientId={googleclientId}>
      <App />
    </GoogleOAuthProvider>
);
