// src/setupTests.ts
import * as matchers from '@testing-library/jest-dom/matchers';
import {cleanup} from '@testing-library/react';
import {afterEach, expect, vi} from 'vitest';

import {mockGetItem, mockNavigate} from './__tests__/testMocks';

// Extend jest-dom matchers
expect.extend(matchers);

// Cleanup
afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

// Mock useNavigate
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock axios (axios mock itself stays here because it’s used globally)
vi.mock('axios');

// Setup global mocks
Object.defineProperty(window, 'localStorage', {
  value: {getItem: mockGetItem},
  writable: true,
});

global.fetch = vi.fn();
window.alert = vi.fn();
window.confirm = vi.fn();
