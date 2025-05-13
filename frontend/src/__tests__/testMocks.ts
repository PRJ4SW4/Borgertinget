// src/__tests__/testMocks.ts
import axios from 'axios';
import {Mocked, vi} from 'vitest';
import { AxiosError, AxiosHeaders, InternalAxiosRequestConfig, AxiosResponse } from "axios";

// Export shared mocks
export const mockNavigate = vi.fn();
export const mockedAxios = axios as Mocked<typeof axios>;
export const mockGetItem = vi.fn();

(
  mockedAxios.isAxiosError as unknown as (val: unknown) => val is AxiosError
) = (val): val is AxiosError =>
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

  Object.defineProperty(window, 'localStorage', {
    value: localStorageMock,
    writable: true,
  });
}

// Mock alert
export function mockAlert() {
  vi.spyOn(window, "alert").mockImplementation(() => {});
}

// Mock Axios Error
export function createAxiosError(status: number, message: string): AxiosError {
  const headers = new AxiosHeaders();

  const dummyConfig: InternalAxiosRequestConfig = {
    headers,
    method: "GET",
    url: "/mock-url",
    transformRequest: [],
    transformResponse: [],
    timeout: 0,
    adapter: async () =>
      ({
        data: {},
        status: 200,
        statusText: "OK",
        headers,
        config: {} as InternalAxiosRequestConfig,
      } as AxiosResponse),
    data: undefined,
    params: undefined,
    responseType: "json",
    withCredentials: false,
    transitional: {},
    signal: undefined,
  };

  const error = Object.create(axios.AxiosError.prototype) as AxiosError;
  axios.AxiosError.call(error, message, "ERR_BAD_REQUEST", dummyConfig);

  error.response = {
    status,
    data: {},
    statusText: message,
    headers,
    config: dummyConfig,
  };

  error.isAxiosError = true;

  return error;
}