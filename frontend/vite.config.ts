import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

// https://vite.dev/config/

const backendUrl = "http://localhost:5218";

export default defineConfig({
  plugins: [react()],
  server: {
    // Add the 'server' configuration object
    proxy: {
      // This proxies any request starting with /api to the backend URL
      "/api": {
        target: backendUrl,
        changeOrigin: true, // Needed for virtual hosted sites/correct host header
        secure: false, // If using self-signed HTTPS cert on backend
      },
      "/uploads": {
        // New rule for image uploads folder
        target: backendUrl,
        changeOrigin: true,
        secure: false, // If using self-signed HTTPS cert
      },
    },
  },
  test: {
    environment: "jsdom",
    globals: true, // This line correctly enables Vitest globals
    setupFiles: "./src/setupTests.ts", // Path to your test setup file
    coverage: {
      provider: "v8", // Required for @vitest/coverage-v8
      reporter: ["text", "html", "lcov"], // Terminal + HTML + lcov
      reportsDirectory: "./coverage", // Optional: customize output dir
      exclude: ["**/node_modules/**", "**/tests/**"], // Optional: ignore files
    },
  },
});
