// src/__tests__/testMocks.ts
import axios from "axios";
import { Mocked, vi } from "vitest";
import { AxiosError } from "axios";

// Export shared mocks
export const mockNavigate = vi.fn();
export const mockedAxios = axios as Mocked<typeof axios>;
export const mockGetItem = vi.fn();

(mockedAxios.isAxiosError as unknown as (val: unknown) => val is AxiosError) = (
  val
): val is AxiosError =>
  typeof val === "object" &&
  val !== null &&
  "isAxiosError" in val &&
  (val as { isAxiosError?: unknown }).isAxiosError === true;

// Mock local storage
export function mockLocalStorage() {
  const localStorageMock = (() => {
    let store: Record<string, string> = {};

    return {
      getItem(key: string) {
        return store[key] || null;
      },
      setItem(key: string, value: string) {
        store[key] = value.toString();
      },
      removeItem(key: string) {
        delete store[key];
      },
      clear() {
        store = {};
      },
    };
  })();

  Object.defineProperty(window, "localStorage", {
    value: localStorageMock,
    writable: true,
  });
}

// Mock alert
export function mockAlert() {
  vi.spyOn(window, "alert").mockImplementation(() => {});
}

// Mock Axios Error
// Simulating real HTTP error responses
export function createAxiosError(
  status: number,
  message = "Request failed"
): AxiosError {
  return {
    isAxiosError: true, // sets this to true for all its errors
    message, // Custom error message
    name: "AxiosError",
    config: {}, // Mocked Axios config
    toJSON: () => ({}),

    // Simulated HTTP response object
    response: {
      status,
      statusText: message,
      headers: {},
      config: {},
      data: {},
    },
  } as AxiosError;
}
