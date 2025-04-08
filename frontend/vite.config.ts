import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
const backendUrl = 'http://localhost:5218'; // Example: Use HTTP or HTTPS as appropriate

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: { // Add the 'server' configuration object
    proxy: {
      // String shorthand: forward '/api' requests to 'backendUrl/api'
      // '/api': backendUrl,

      // Or use the object syntax for more options:
      // This proxies any request starting with /api/...
      '/api': {
        target: backendUrl,  // The server where your ASP.NET Core app is running
        changeOrigin: true, // Recommended, needed for virtual hosted sites/correct host header
        secure: false,      // Set to false if your backend uses HTTPS with a self-signed dev certificate
        // Optional: If your backend API routes don't start with /api, you might need to rewrite
        // rewrite: (path) => path.replace(/^\/api/, '') // Removes /api prefix before forwarding
      },
      '/uploads': { // New rule for image uploads folder
            target: backendUrl, // Your ASP.NET Core backend URL
            changeOrigin: true,
            secure: false, // If using self-signed HTTPS cert
        }
    },
    // Optional: Define the port for the Vite dev server explicitly if needed
    // port: 5173
  }
})