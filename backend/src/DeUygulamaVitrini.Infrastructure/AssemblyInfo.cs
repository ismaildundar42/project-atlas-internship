using System.Runtime.CompilerServices;

// Grants the AI test console app access to internal members of this assembly,
// specifically ProjectAssistantService.ClassifyIntent and AssistantIntent
// for deterministic unit testing without reflection.
[assembly: InternalsVisibleTo("DeUygulamaVitrini.AiTests")]
