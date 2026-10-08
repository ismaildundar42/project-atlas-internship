export interface ProjectAiSummaryRequest {
  language?: 'tr' | 'en' | string;
}

export interface ProjectAiSummaryResponse {
  projectId: number;
  projectName: string;
  summary: string;
  generatedAtUtc: string;
  provider: string;
  model: string;
  durationMs: number;
  providerAvailable: boolean;
  finishReason?: string;
  isComplete?: boolean;
  promptTokens?: number;
  completionTokens?: number;
}
