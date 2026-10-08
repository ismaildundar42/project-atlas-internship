export interface ProjectAssistantHistoryItem {
  role: 'user' | 'assistant';
  content: string;
  referencedProjectIds?: number[];
}

export interface ProjectAssistantRequest {
  question: string;
  language?: string;
  history?: ProjectAssistantHistoryItem[];
}

export interface ProjectAssistantCitation {
  projectId: number;
  slug: string;
  name: string;
  shortDescription: string;
  coverImageUrl?: string | null;
  statusName: string;
  categoryName: string;
  locations: string[];
  technologies: string[];
  matchedChunkKeys: string[];
}

export interface ProjectAssistantMetadata {
  retrievedChunksCount: number;
  citationCount: number;
  retrievalDurationMs: number;
  generationDurationMs: number;
  totalDurationMs: number;
  groundedFromContext: boolean;
  responseLanguage: string;
  finishReason?: string;
  isComplete?: boolean;
  intent?: string;
  executionPath?: string;
  fallbackReason?: string;
  structuredResolution?: string;
}

export interface ProjectAssistantResponse {
  answer: string;
  citations: ProjectAssistantCitation[];
  metadata: ProjectAssistantMetadata;
}

export interface AssistantMessage {
  id: string;
  type: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  citations?: ProjectAssistantCitation[];
  metadata?: ProjectAssistantMetadata;
  isError?: boolean;
}

