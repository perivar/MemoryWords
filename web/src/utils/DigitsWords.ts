export class DigitsWords {
  digits: number[];
  wordCandidates: string[];

  constructor(digits: number[]) {
    this.digits = digits;
    this.wordCandidates = [];
  }

  get Digits(): number[] {
    return this.digits;
  }

  DigitsAsString(): string {
    return this.digits.join(',');
  }

  get WordCandidates(): string[] {
    return this.wordCandidates;
  }

  toString(): string {
    return `[${this.digits.join(',')}] Candidates=${this.WordCandidates.length}`;
  }
}