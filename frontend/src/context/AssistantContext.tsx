import React, { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import assistantService from '../services/assistantService';
import type { AssistantMessage, ProjectAssistantHistoryItem } from '../types/assistant';

export type AssistantDisplayState = 'collapsed' | 'compact' | 'expanded';

interface AssistantContextValue {
  displayState: AssistantDisplayState;
  isOpen: boolean;
  setDisplayState: (state: AssistantDisplayState) => void;
  openCompact: () => void;
  openExpanded: () => void;
  collapseAssistant: () => void;
  toggleAssistant: () => void;
  messages: AssistantMessage[];
  isLoading: boolean;
  errorMessage: string | null;
  sendMessage: (queryText: string) => Promise<void>;
  clearConversation: () => void;
  suggestedPrompts: string[];
}

const AssistantContext = createContext<AssistantContextValue | undefined>(undefined);

export const AssistantProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const { t, i18n } = useTranslation(['assistant', 'common']);
  const [displayState, setDisplayState] = useState<AssistantDisplayState>('collapsed');
  const [messages, setMessages] = useState<AssistantMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Suggested prompts
  const suggestedPrompts = [
    t('assistant.examples.predictiveMaintenance', 'Kestirimci bakım alanında hangi projelerimiz var?'),
    t('assistant.examples.sapIntegration', 'SAP ile entegre çalışan sistemlerimiz hangileri?'),
    t('assistant.examples.kangalAi', 'Kangal sahasında kullanılan yapay zeka projeleri neler?'),
    t('assistant.examples.capabilities', 'Sen ne yapabiliyorsun?'),
  ];

  const openCompact = useCallback(() => setDisplayState('compact'), []);
  const openExpanded = useCallback(() => setDisplayState('expanded'), []);
  const collapseAssistant = useCallback(() => setDisplayState('collapsed'), []);

  const toggleAssistant = useCallback(() => {
    setDisplayState((prev) => (prev === 'collapsed' ? 'compact' : 'collapsed'));
  }, []);

  const clearConversation = useCallback(() => {
    setMessages([]);
    setErrorMessage(null);
  }, []);

  const sendMessage = useCallback(
    async (queryText: string) => {
      const query = queryText.trim();
      if (!query) return;

      setErrorMessage(null);

      const userMessage: AssistantMessage = {
        id: `user-${Date.now()}`,
        type: 'user',
        content: query,
        timestamp: new Date(),
      };

      // Prepare bounded history for conversational RAG (last 6 turns, excluding errors)
      const validHistory: ProjectAssistantHistoryItem[] = messages
        .filter((m) => !m.isError)
        .slice(-6)
        .map((m) => ({
          role: m.type,
          content: m.content,
          referencedProjectIds: m.citations ? m.citations.map((c) => c.projectId) : undefined,
        }));

      setMessages((prev) => [...prev, userMessage]);
      setIsLoading(true);

      try {
        const response = await assistantService.ask({
          question: query,
          language: i18n.language || 'tr',
          history: validHistory.length > 0 ? validHistory : undefined,
        });

        const assistantMessage: AssistantMessage = {
          id: `assistant-${Date.now()}`,
          type: 'assistant',
          content: response.answer,
          timestamp: new Date(),
          citations: response.citations,
          metadata: response.metadata,
        };

        setMessages((prev) => [...prev, assistantMessage]);
      } catch (err: unknown) {
        const is503 = (err as { response?: { status?: number } })?.response?.status === 503;
        const errorContent = is503
          ? t('assistant.unavailable', 'Proje Asistanı şu anda kullanılamıyor. Lütfen biraz sonra tekrar deneyin.')
          : t('assistant.unavailable', 'Proje Asistanı şu anda kullanılamıyor. Lütfen biraz sonra tekrar deneyin.');

        const failureMessage: AssistantMessage = {
          id: `assistant-error-${Date.now()}`,
          type: 'assistant',
          content: errorContent,
          timestamp: new Date(),
          isError: true,
        };

        setMessages((prev) => [...prev, failureMessage]);
      } finally {
        setIsLoading(false);
      }
    },
    [messages, i18n.language, t]
  );

  return (
    <AssistantContext.Provider
      value={{
        displayState,
        isOpen: displayState !== 'collapsed',
        setDisplayState,
        openCompact,
        openExpanded,
        collapseAssistant,
        toggleAssistant,
        messages,
        isLoading,
        errorMessage,
        sendMessage,
        clearConversation,
        suggestedPrompts,
      }}
    >
      {children}
    </AssistantContext.Provider>
  );
};

export function useAssistant(): AssistantContextValue {
  const context = useContext(AssistantContext);
  if (!context) {
    throw new Error('useAssistant must be used within an AssistantProvider');
  }
  return context;
}
