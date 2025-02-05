import { render, screen } from "@testing-library/react";
import App from "../src/App";
import { test, expect } from "vitest"; // Import Vitest functions

test("renders frontend to backend test header", () => {
  render(<App />);
  expect(screen.getByText(/Frontend to Backend Test/i)).toBeInTheDocument();
});
