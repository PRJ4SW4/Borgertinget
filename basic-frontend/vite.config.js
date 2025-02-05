import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true, // Enables global `test`, `expect` functions
    environment: "jsdom", // Simulates browser-like environment
    setupFiles: "./src/test/setup.js", // Optional: Jest DOM setup
  },
});
