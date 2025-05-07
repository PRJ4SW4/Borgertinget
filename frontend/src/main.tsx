import React from "react";
import ReactDOM from "react-dom/client";
// BrowserRouter is needed for React Router to work, handles URL changes
import { BrowserRouter } from "react-router-dom";
// The main App component where our routes and layout live
import App from "./App";
// Global styles for the entire application
import './App.css';

// This is the main entry point where React attaches to the HTML.
// We grab the 'root' div from index.html.
ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  // React.StrictMode helps catch potential problems in the app during development.
  // It doesn't affect the production build.
  <React.StrictMode>
    {/* BrowserRouter wraps the App to enable routing capabilities */}
    <BrowserRouter>
      {/* App component contains all our pages and routing logic */}
      <App />
    </BrowserRouter>
  </React.StrictMode>
);