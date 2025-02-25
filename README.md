# MemoryWords

A C# utility that helps you remember long numbers (like π) by converting them into memorable Norwegian words using the Mnemonic major system.

## What is the Mnemonic Major System?

The Mnemonic major system is a memory technique that converts numbers into consonant sounds, which can then be combined with vowels to create words. This makes it easier to remember long sequences of numbers by converting them into more memorable words or phrases.

### How it Works

The system maps numbers to consonant sounds as follows:

```cs
0 = S, Z        (think "ZERO" starts with 'Z')
1 = T, D        (think 'T' has one downstroke)
2 = N           (think 'N' has two downstrokes)
3 = M           (think 'M' has three downstrokes)
4 = R           (last letter of 'FOUR')
5 = L           (roman numeral 'L' = 50)
6 = J, G        (reversed 'J' looks like '6')
7 = K           (think 'K' contains two '7's)
8 = F, V        ('F' looks like '8')
9 = P, B        ('P' is a mirror of '9')
```

Or in Norwegian:

```cs
0 = S, Z        (tenk på "SIRKEL" eller "0" på engelsk "ZERO")
1 = T, D        (tenk på at 'T' har én nedstrek)
2 = N           (tenk på at 'N' har to nedstreker)
3 = M           (tenk på at 'M' har tre nedstreker)
4 = R           (tenk på at "FIRE" inneholder 'R', eller "R som i rein, fire bein")
5 = L           (tenk på romertallet 'L' som er 50)
6 = J, G        (tenk på at speilvendt 'J' ligner på '6')
7 = K           (tenk på at 'K' inneholder to '7'-tall)
8 = F, V        (tenk på at 'F' ligner på '8')
9 = P, B        (tenk på at 'P' er speilvendt '9')
```

Vowels (A, E, I, O, U, Y, Æ, Ø, Å) and some consonants (H, W, C) are ignored, which allows for flexibility in creating words while maintaining the number sequence.

### Example

The number "53138552" could be broken down into memorable Norwegian words:

- "53" = "LAM" (L=5, M=3)
- "13" = "DUM" (D=1, M=3)
- "85" = "FLY" (F=8, L=5)
- "52" = "LYN" (L=5, N=2)

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

The program uses several Norwegian word lists from different sources:

### Aksis: Norsk tekstarkiv

- Source: <http://korpus.uib.no/humfak/nta/>
- Files:
  - `ord10k.csv`: 10,000 most frequent Norwegian words
  - `ord10000.txt`: Alternative 10K word list

### Wiktionary Frequency Lists

- Source: <https://en.wiktionary.org/wiki/Wiktionary:Frequency_lists/Norwegian_Bokmål_wordlist>
- File: `wiktionary_frequency_list.txt`
- Contains: Common Norwegian words with frequency data

### Norwegian Scrabble Federation (NSF)

- Source: <http://www2.scrabbleforbundet.no/?attachment_id=1620>
- File:
  - `nsf2012.txt`: Historical NSF word list from 2012
- Source: <https://www2.scrabbleforbundet.no/?p=4939>
- File:
  - `nsf2023.txt`: Latest NSF word list from 2023
- Description: Official word list used in Norwegian Scrabble tournaments

### Scandinavian Keyboard Project

- Source: <https://code.google.com/p/scandinavian-keyboard/downloads/detail?name=norwegian.txt>
- File: `norwegian.txt`
- Contains: Comprehensive Norwegian word list (62K words)

### OpenOffice Norwegian Dictionary

- Source: <http://extensions.openoffice.org/en/projectrelease/norwegian-dictionaries-spell-checker-thesaurus-and-hyphenation-22>
- File: `norwegian_large.txt`
- Description: Comprehensive word list generated using unmunch to build all word forms
- Note: This is a very large dictionary that includes all possible word forms
- Steps to generate:
  - Unzip the dictionary-no-no-2.2.oxt file (Oxt-files are just zip files with a different extension. Unzip it and you'll find the dic and aff files inside)
  - `unmunch nb_NO.dic nb_NO.aff > norwegian_large.txt`
  - Remember to remove duplicate lines
  