// src/types/pageTypes.ts

export interface PageSummaryDto {
    id: number;
    title: string;
    parentPageId: number | null;
    displayOrder: number;
    hasChildren: boolean; // Added this in the backend example, useful here
  }
  
  export interface PageDetailDto {
    id: number;
    title: string;
    content: string; // The Markdown content
    parentPageId: number | null;
    // --- Next/Previous Requirement ---
    previousSiblingId: number | null;
    nextSiblingId: number | null;
    // --- End Next/Previous Requirement ---
    // --- Questions Requirement ---
    associatedQuestions: QuestionDto[]; // Now an array
    // --- End Questions Requirement ---
  }

  export interface AnswerOptionDto {
    id: number;
    optionText: string;
  }
  
  export interface QuestionDto {
    id: number;
    questionText: string;
    options: AnswerOptionDto[];
  }
  
  // Type for the hierarchical structure used in the frontend navigation
  export interface PageNode extends PageSummaryDto {
    children: PageNode[];
  }