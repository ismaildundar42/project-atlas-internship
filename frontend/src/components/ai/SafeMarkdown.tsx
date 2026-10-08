import React from 'react';

interface SafeMarkdownProps {
  content: string;
  className?: string;
}

/**
 * Parses inline formatting: ***bold-italic***, **bold**, *italic*, _italic_, `code` safely without dangerouslySetInnerHTML.
 */
function renderInline(text: string): React.ReactNode[] {
  const parts: React.ReactNode[] = [];
  // Tokenize regex for inline markdown patterns:
  // 1: ***bold italic***
  // 2: **bold**
  // 3: *italic*
  // 4: _italic_
  // 5: `code`
  const inlineRegex = /(\*\*\*(.+?)\*\*\*|\*\*(.+?)\*\*|\*(.+?)\*|_(.+?)_|`([^`]+)`)/g;
  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = inlineRegex.exec(text)) !== null) {
    if (match.index > lastIndex) {
      parts.push(text.substring(lastIndex, match.index));
    }

    const fullMatch = match[0];
    if (match[2]) {
      // ***bold italic***
      parts.push(
        <strong key={`bi-${match.index}`} style={{ fontWeight: 600 }}>
          <em>{match[2]}</em>
        </strong>
      );
    } else if (match[3]) {
      // **bold**
      parts.push(
        <strong key={`b-${match.index}`} style={{ fontWeight: 600 }}>
          {match[3]}
        </strong>
      );
    } else if (match[4]) {
      // *italic*
      parts.push(
        <em key={`i-${match.index}`}>
          {match[4]}
        </em>
      );
    } else if (match[5]) {
      // _italic_
      parts.push(
        <em key={`u-${match.index}`}>
          {match[5]}
        </em>
      );
    } else if (match[6]) {
      // `code`
      parts.push(
        <code
          key={`c-${match.index}`}
          style={{
            background: 'var(--color-surface-sunken, rgba(0,0,0,0.06))',
            padding: '2px 5px',
            borderRadius: '4px',
            fontSize: '0.88em',
            fontFamily: 'monospace',
          }}
        >
          {match[6]}
        </code>
      );
    }

    lastIndex = match.index + fullMatch.length;
  }

  if (lastIndex < text.length) {
    parts.push(text.substring(lastIndex));
  }

  return parts;
}

/**
 * Sanitized, lightweight markdown renderer supporting paragraphs, bold, lists, and linebreaks.
 * Zero external dependencies and 100% XSS safe.
 */
export const SafeMarkdown: React.FC<SafeMarkdownProps> = ({ content, className }) => {
  if (!content) return null;

  const lines = content.split('\n');
  const elements: React.ReactNode[] = [];
  let currentList: { type: 'ul' | 'ol'; items: string[] } | null = null;

  const flushList = (keyPrefix: string) => {
    if (!currentList) return;
    if (currentList.type === 'ul') {
      elements.push(
        <ul
          key={`${keyPrefix}-ul`}
          style={{
            margin: 'var(--space-2) 0',
            paddingLeft: 'var(--space-5)',
            display: 'flex',
            flexDirection: 'column',
            gap: 'var(--space-1)',
          }}
        >
          {currentList.items.map((item, idx) => (
            <li key={idx} style={{ lineHeight: 1.55 }}>
              {renderInline(item)}
            </li>
          ))}
        </ul>
      );
    } else {
      elements.push(
        <ol
          key={`${keyPrefix}-ol`}
          style={{
            margin: 'var(--space-2) 0',
            paddingLeft: 'var(--space-5)',
            display: 'flex',
            flexDirection: 'column',
            gap: 'var(--space-1)',
          }}
        >
          {currentList.items.map((item, idx) => (
            <li key={idx} style={{ lineHeight: 1.55 }}>
              {renderInline(item)}
            </li>
          ))}
        </ol>
      );
    }
    currentList = null;
  };

  lines.forEach((line, index) => {
    const trimmed = line.trim();

    if (!trimmed) {
      flushList(`line-${index}`);
      return;
    }

    // Unordered list: • or - or *
    const ulMatch = trimmed.match(/^[-*•]\s+(.*)$/);
    if (ulMatch) {
      if (currentList && currentList.type !== 'ul') {
        flushList(`line-${index}`);
      }
      if (!currentList) {
        currentList = { type: 'ul', items: [] };
      }
      currentList.items.push(ulMatch[1]);
      return;
    }

    // Ordered list: 1. or 2) etc
    const olMatch = trimmed.match(/^(\d+)[.)]\s+(.*)$/);
    if (olMatch) {
      if (currentList && currentList.type !== 'ol') {
        flushList(`line-${index}`);
      }
      if (!currentList) {
        currentList = { type: 'ol', items: [] };
      }
      currentList.items.push(olMatch[2]);
      return;
    }

    // Normal paragraph line
    flushList(`line-${index}`);
    elements.push(
      <p
        key={`p-${index}`}
        style={{
          margin: '0 0 var(--space-2) 0',
          lineHeight: 1.6,
        }}
      >
        {renderInline(trimmed)}
      </p>
    );
  });

  flushList('end');

  return (
    <div className={className} style={{ wordBreak: 'break-word' }}>
      {elements}
    </div>
  );
};

export default SafeMarkdown;
