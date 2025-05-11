// src/__tests__/testMocks.ts
import axios from 'axios';
import {Mocked, vi} from 'vitest';

// Export shared mocks
export const mockNavigate = vi.fn();
export const mockedAxios = axios as Mocked<typeof axios>;
export const mockGetItem = vi.fn();