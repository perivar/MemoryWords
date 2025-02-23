# MemoryWords

A C# utility that helps you remember long numbers (like π) by converting them into memorable Norwegian words using the Mnemonic major system.

## What is the Mnemonic Major System?

The Mnemonic major system is a memory technique that converts numbers into consonant sounds, which can then be combined with vowels to create words. This makes it easier to remember long sequences of numbers by converting them into more memorable words or phrases.

### How it Works

The system maps numbers to consonant sounds as follows:

```cs
0 = s, z        (think "zero" starts with 'z')
1 = t, d        (think 't' has one downstroke)
2 = n           (think 'n' has two downstrokes)
3 = m           (think 'm' has three downstrokes)
4 = r           (last letter of 'four')
5 = l           (roman numeral 'L' = 50)
6 = j, g        (reversed 'j' looks like '6')
7 = k           (think 'k' contains two '7's)
8 = f, v        ('f' looks like '8')
9 = p, b        ('p' is a mirror of '9')
```

Vowels (a, e, i, o, u, y, æ, ø, å) and some consonants (h, w, c) are ignored, which allows for flexibility in creating words while maintaining the number sequence.

### Example

The number "53138552" could be broken down into memorable Norwegian words:

- "53" = "lam" (l=5, m=3)
- "13" = "dum" (d=1, m=3)
- "85" = "fly" (f=8, l=5)
- "52" = "lyn" (l=5, n=2)

## Features

- Processes numbers of any length
- Uses multiple Norwegian word lists for maximum coverage
- Supports various dictionary formats (5K, 10K, 62K words)
- Outputs results in CSV format for easy analysis
- Cross-platform support (.NET 9.0)

## Usage

Run the program with a test number (default is 53138552):

```bash
dotnet run
```

The program will search through different Norwegian dictionaries and find words that match the number sequence according to the Mnemonic major system rules.

## Dictionary Sources

- wiktionary_frequency_list.txt: Common Norwegian words from Wiktionary
- ord10k.csv: 10,000 most frequent Norwegian words
- ord10000.txt: Alternative 10K word list
- norwegian.txt: Comprehensive Norwegian word list (62K words)
- NSF-ordlisten: Norwegian Scrabble Federation word list
