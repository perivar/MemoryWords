# MemoryWords

A C# utility that helps you remember long numbers (like π) by converting them into memorable Norwegian words using the Mnemonic major system.

## What is the Mnemonic Major System?

The Mnemonic major system is a memory technique that converts numbers into consonant sounds, which can then be combined with vowels to create words. This makes it easier to remember long sequences of numbers by converting them into more memorable words or phrases.

### How it Works

You can read more about the original Mnemonic Major System here:

- [Major System Reference Guide](https://collegeinfogeek.com/major-mnemonic-system-reference/)
- [Major System Info](https://major-system.info/)
- [Wikipedia - Mnemonic Major System](https://en.wikipedia.org/wiki/Mnemonic_major_system)

The original Mnemonic Major System maps numbers to consonant sounds as follows:

| Digit | Speech Sounds (IPA)  | Associated Letters & Examples                              | Notes                                                                                                       |
| ----- | -------------------- | ---------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| 0     | /s/, /z/             | S as in See, Z as in Zero                                  | Zero begins with Z. Other letters sound similar when spoken.                                                |
| 1     | /t/, /d/, /θ/, /ð/   | T as in Take, D as in Day, TH as in THing, TH as in THis   | t and d each have one downstroke, and sound similar when voiced.                                            |
| 2     | /n/                  | N as in Nay                                                | n has two downstrokes when written.                                                                         |
| 3     | /m/                  | M as in May                                                | m has three downstrokes when written.                                                                       |
| 4     | /r/                  | R as in Ray                                                | Last letter of fouR                                                                                         |
| 5     | /l/                  | L as in Lay                                                | Roman numeral L = 50                                                                                        |
| 6     | /tʃ/, /dʒ/, /ʃ/, /ʒ/ | CH as in CHip, J as in Jump, SH as in SHip, S as in viSion | j has a curve near the bottom, like 6 does                                                                  |
| 7     | /k/, /g/             | K as in Key, G as in Gee                                   | K contains two 7s                                                                                           |
| 8     | /f/, /v/             | F as in Few, V as in View                                  | I associate V with "V8". F sounds like V when spoken. F looks like 8                                        |
| 9     | /p/, /b/             | P as in Pay, B as in Bay                                   | 9 rotated 180 degrees looks like b. 9 flipped horizontally looks like P. P and B sound similar when spoken. |

Here's a handy trick for remembering the sounds & digits in the right order, using uppercase letters (Created by Adam Beggs from Hertford, UK):
> houSToN May ReaLly CHecK iF Bowie is major tom.

For this program, the system has been modified to better suit Norwegian phonetics. In particular, digit 6 has been adapted to only match the /ʃ/ and /tʃ/ sounds, which in Norwegian are represented by combinations like sj, skj, kj, and tj.

The Norwegian adaptation:

| Digit | Speech Sounds (IPA) | Associated Letters & Examples                               | Notes                                                                                          |
| ----- | ------------------- | ----------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| 0     | /s/, /z/            | S som i Sirkel, Z som i Zalo                                | 0 er en Sirkel, 0 på engelsk er Zero                                                           |
| 1     | /t/, /d/            | T som i Tal, D som i Dal                                    | t og d har én nedstrek                                                                         |
| 2     | /n/                 | N som i Nei                                                 | n har to nedstreker                                                                            |
| 3     | /m/                 | M som i Meg                                                 | m har tre nedstreker                                                                           |
| 4     | /r/                 | R som i Rein                                                | tenk på fiRe, eller R som i rein, fire bein                                                    |
| 5     | /l/                 | L som i Liv                                                 | Romertallet L er 50                                                                            |
| 6     | /ʃ/, /tʃ/           | SJ som i Sjø, SKJ som i Skje, KJ som i Kjede, TJ som i Tjue | J har en kurve nederst slik som 6 har                                                          |
| 7     | /k/, /g/            | K som i Kul, G som i Gul                                    | K inneholder to 7-tall                                                                         |
| 8     | /f/, /v/            | F som i Fisk, V som i Visk                                  | Jeg assosierer V med "V8". F lyder som V når den uttales. F ligner på 8                        |
| 9     | /p/, /b/            | P som i Pil, B som i Bil                                    | 9 rotert 180 grader ser ut som b. 9 speilvendt ser ut som P. P og B lyder likt når de uttales. |

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
  