import { DigitsWords } from './DigitsWords';

const vowels = '[aeiouyæøåhjwc]'; // vowels and non-used characters (hjwc)
const zeroOrMoreVowels = `${vowels}*`;
const oneOrMoreVowels = `${vowels}+`;

function getDigitRegexp(singleDigit: number): string | null {
  switch (singleDigit) {
    case 0:
      return '[sz]';
    case 1:
      return '[td]';
    case 2:
      return 'n';
    case 3:
      return 'm';
    case 4:
      return 'r';
    case 5:
      return 'l';
    case 6:
      return '((s(k)?j)|(kj)|(tj))';
    case 7:
      return '[kg]';
    case 8:
      return '[fv]';
    case 9:
      return '[pb]';
    default:
      return null;
  }
}

// Cache for compiled regular expressions
const regexCache: Map<string, RegExp> = new Map();

function getRegexp(digits: number[]): RegExp {
  // Use the array as string for cache key
  const key = digits.join(',');
  if (regexCache.has(key)) {
    return regexCache.get(key)!;
  }

  let pattern = '^('; // start regexp and define first group
  pattern += zeroOrMoreVowels; // zero or more vowels

  // the first digits
  let backReferenceCount = 2;
  let numPattern: string | null = null;

  for (let i = 0; i < digits.length - 1; i++) {
    if ((numPattern = getDigitRegexp(digits[i])) !== null) {
      // one or two consecutive consonants using backreference
      pattern += `(${numPattern})`;
      pattern += `\\${backReferenceCount}?`; // use backreference to group number
      backReferenceCount++;
    }

    if (i + 1 < digits.length) {
      // check if the next number is the same consonant,
      // if that is the case a vowel is mandatory, otherwise vowels are optional
      if (digits[i] === digits[i + 1]) {
        pattern += oneOrMoreVowels; // one or more vowels
      } else {
        pattern += zeroOrMoreVowels; // zero or more vowels
      }
    } else {
      pattern += zeroOrMoreVowels;
    }
  }

  // the last digit
  if ((numPattern = getDigitRegexp(digits[digits.length - 1])) !== null) {
    // one or two consecutive consonants using backreference
    pattern += `(${numPattern})`;
    pattern += `\\${backReferenceCount}?`; // use backreference to group number
    pattern += zeroOrMoreVowels; // zero or more vowels
  }

  // end regexp and end group
  pattern += ')$';

  const regExpression = new RegExp(pattern, 'i'); // i for case-insensitive
  regexCache.set(key, regExpression);

  return regExpression;
}

function getNextChunk(source: number[], startIndex: number, maxChunkSize: number): number[] {
  const remainingLength = source.length - startIndex;
  const chunkSize = Math.min(maxChunkSize, remainingLength);
  return source.slice(startIndex, startIndex + chunkSize);
}

type ProgressCallback = (words: DigitsWords) => void;

function lookupWords(
  dictionary: string[],
  searchDigits: number[],
  noDigitsProcessed: { value: number },
  onProgress?: ProgressCallback
): DigitsWords | null {
  const regExpression = getRegexp(searchDigits);
  const matches: string[] = [];

  // Test each word individually like the C# version
  for (const word of dictionary) {
    if (regExpression.test(word)) {
      matches.push(word);
    }
  }

  if (matches.length > 0) {
    noDigitsProcessed.value = searchDigits.length; // store the number of digits already processed
    const word = new DigitsWords(searchDigits);
    word.wordCandidates = matches;

    if (onProgress) {
      onProgress(word);
    }

    return word;
  } else {
    // reduce the number of digits and try again
    const newLength = searchDigits.length - 1;
    if (newLength <= 0) return null;

    const reducedDigits = searchDigits.slice(0, newLength);
    return lookupWords(dictionary, reducedDigits, noDigitsProcessed, onProgress);
  }
}

export async function FindWords(
  dictionary: string[],
  digits: number[],
  onProgress?: ProgressCallback
): Promise<DigitsWords[]> {
  const maxChunkSize = 16;
  const wordList: DigitsWords[] = [];
  let digitsProcessed = 0;
  const lastProcessedCount = { value: 0 };

  while (digitsProcessed < digits.length) {
    const currentChunk = getNextChunk(digits, digitsProcessed, maxChunkSize);
    // Add small delay to allow UI updates
    await new Promise(resolve => setTimeout(resolve, 0));
    const word = lookupWords(dictionary, currentChunk, lastProcessedCount, onProgress);

    if (lastProcessedCount.value === 0) {
      break; // No more words found
    }

    if (word) {
      wordList.push(word);
    }

    digitsProcessed += lastProcessedCount.value;
  }

  return wordList;
}