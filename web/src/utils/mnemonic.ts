// Mapping of digits to phonemes (consonant sounds)
export const digitToPhonemes: { [key: string]: string[] } = {
  '0': ['s', 'z'],
  '1': ['t', 'd'],
  '2': ['n'],
  '3': ['m'],
  '4': ['r'],
  '5': ['l'],
  '6': ['sj', 'skj', 'tj', 'kj'],
  '7': ['k', 'g'],
  '8': ['f', 'v'],
  '9': ['p', 'b']
};

// Reverse mapping for word to digit conversion
export const phonemeToDigit: { [key: string]: string } = Object.entries(digitToPhonemes).reduce(
  (acc, [digit, phonemes]) => {
    phonemes.forEach(phoneme => {
      acc[phoneme] = digit;
    });
    return acc;
  },
  {} as { [key: string]: string }
);

// Vowels and ignored characters
export const vowels = ['a', 'e', 'i', 'o', 'u', 'y', 'æ', 'ø', 'å', 'h', 'j', 'w', 'c'];

// Pattern for matching phonemes and vowels/spaces
const pattern = `(${[...new Set(Object.keys(phonemeToDigit))].sort((a, b) => b.length - a.length).join('|')})|(([${vowels.join('')}]+)|\\s+)`;
const phonemeRegex = new RegExp(pattern, 'gi');

export function parseDigits(word: string): string {
  if (!word) return '';

  const normalizedWord = word.toLowerCase();
  const digits: string[] = [];
  let lastDigit: string | null = null;
  let hasVowelOrSpaceSinceLastDigit = false;

  let match: RegExpExecArray | null;
  phonemeRegex.lastIndex = 0; // Reset regex index
  while ((match = phonemeRegex.exec(normalizedWord)) !== null) {
    // Check if this is a vowel or space
    if (match[2]) {
      hasVowelOrSpaceSinceLastDigit = true;
      continue;
    }

    const phoneme = match[1];
    if (phoneme && phonemeToDigit[phoneme]) {
      const currentDigit = phonemeToDigit[phoneme];
      // Add the digit if it's different from the last one OR if we've seen vowels/spaces
      if (!lastDigit || lastDigit !== currentDigit || hasVowelOrSpaceSinceLastDigit) {
        digits.push(currentDigit);
        lastDigit = currentDigit;
        hasVowelOrSpaceSinceLastDigit = false;
      }
    }
  }

  return digits.join('');
}

export function toMnemonic(word: string): string {
  if (!word) return '';

  const normalizedWord = word.toLowerCase();
  const parts: string[] = [];
  let lastDigit: string | null = null;
  let lastPhoneme: string | null = null;
  let hasVowelOrSpaceSinceLastDigit = false;

  let match: RegExpExecArray | null;
  phonemeRegex.lastIndex = 0; // Reset regex index
  while ((match = phonemeRegex.exec(normalizedWord)) !== null) {
    // Check if this is a vowel or space
    if (match[2]) {
      hasVowelOrSpaceSinceLastDigit = true;
      const vowelGroup = match[2];
      if (vowelGroup.trim()) {
        parts.push(`[${vowelGroup}]`);
      }
      continue;
    }

    const phoneme = match[1];
    if (phoneme && phonemeToDigit[phoneme]) {
      const currentDigit = phonemeToDigit[phoneme];
      // Add the phoneme if it's different from the last one OR if we've seen vowels
      if (!lastDigit || lastDigit !== currentDigit || hasVowelOrSpaceSinceLastDigit) {
        parts.push(`${phoneme}(${currentDigit})`);
        lastDigit = currentDigit;
        lastPhoneme = phoneme;
        hasVowelOrSpaceSinceLastDigit = false;
      } else {
        // Replace the last part to show combined consonants
        parts[parts.length - 1] = `${lastPhoneme}${phoneme}(${currentDigit})`;
      }
    }
  }

  return parts.join(' ');
}