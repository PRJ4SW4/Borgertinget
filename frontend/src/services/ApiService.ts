// src/services/apiService.ts
import type { PageSummaryDto, PageDetailDto } from '../types/pageTypes'; // Import types

const API_BASE_URL = '/api'; // Adjust if needed

export const fetchPagesStructure = async (): Promise<PageSummaryDto[]> => {
  const response = await fetch(`${API_BASE_URL}/pages`);
  if (!response.ok) {
    // Consider more specific error handling based on status code
    throw new Error(`Failed to fetch page structure: ${response.statusText}`);
  }
  // Explicitly type the response parsing
  return await response.json() as PageSummaryDto[];
};

export const fetchPageDetails = async (id: string | number): Promise<PageDetailDto | null> => {
  // Ensure id is valid before fetching
  if (!id) return null;
  const response = await fetch(`${API_BASE_URL}/pages/${id}`);
  if (response.status === 404) {
     return null; // Page not found
  }
  if (!response.ok) {
    throw new Error(`Failed to fetch page details for ID ${id}: ${response.statusText}`);
  }
  return await response.json() as PageDetailDto;
};