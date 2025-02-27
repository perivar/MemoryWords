﻿using System.Text;
namespace MemoryWords
{
	public class FrequencyLists
	{
		private static readonly Encoding _isoLatin1Encoding = Encoding.GetEncoding("ISO-8859-1");
		private readonly List<WordElement> _wordElements = new();
		public List<WordElement> WordElements => _wordElements;
		public string[] Words => _wordElements.Where(w => w.Word != null).Select(w => w.Word!).ToArray();

		/// <summary>
		/// Reads a frequency list from a file in the ord10k.csv format.
		/// The file is a CSV file with the following columns:
		/// Place, Frequency, Percentage, Cumulated, Word
		/// The first line is a header and is skipped.
		/// </summary>
		public void ReadOrd10KFormat(string filePath, bool ignoreSingleLetterWords = false)
		{
			int lineCounter = 0;

			// read in the dictionary file in the ord10k.csv format
			foreach (var line in File.ReadLines(filePath, _isoLatin1Encoding))
			{
				lineCounter++;

				// skip header
				if (lineCounter == 1) continue;

				// ignore blank lines
				if (string.IsNullOrEmpty(line))
					continue;

				// parse
				var elements = line.Split([","], StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[4].Length < 2) continue;

				var word = new WordElement();
				_wordElements.Add(word);

				word.Place = int.Parse(elements[0]);
				word.Frequency = int.Parse(elements[1]);
				word.Word = elements[4];
			}
		}

		/// <summary>
		/// Reads a frequency list from a file in the wiktionary_frequency_list.txt format.
		/// The file is a space-separated file with the following columns:
		/// Word, Frequency
		/// </summary>
		public void ReadWiktionary5KFormat(string filePath, bool ignoreSingleLetterWords = false)
		{
			int lineCounter = 0;

			// read in the dictionary file in the wiktionary_frequency_list.txt format
			foreach (var line in File.ReadLines(filePath))
			{
				lineCounter++;

				// ignore blank lines
				if (string.IsNullOrEmpty(line))
					continue;

				// parse
				var elements = line.Split([" "], StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[0].Length < 2) continue;

				var word = new WordElement();
				_wordElements.Add(word);

				word.Place = lineCounter;
				word.Frequency = int.Parse(elements[1]);
				word.Word = elements[0];
			}
		}

		/// <summary>
		/// Reads a frequency list from a file in the ord10000.txt format.
		/// The file is a space-separated file with the following columns:
		/// Frequency, Word
		/// </summary>
		public void ReadOrd10000Format(string filePath, bool ignoreSingleLetterWords = false)
		{
			int lineCounter = 0;

			// read in the dictionary file in the ord10000.txt format
			foreach (var line in File.ReadLines(filePath, _isoLatin1Encoding))
			{
				lineCounter++;

				// ignore blank lines
				if (string.IsNullOrEmpty(line))
					continue;

				// parse
				var elements = line.Split([" "], StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[1].Length < 2) continue;

				var word = new WordElement();
				_wordElements.Add(word);

				word.Place = lineCounter;
				word.Frequency = int.Parse(elements[0]);
				word.Word = elements[1];
			}
		}
	}

	public class WordElement
	{
		public int Frequency { get; set; }
		public string? Word { get; set; }
		public int Place { get; set; }

		public override string ToString() => $"Place {Place}, Freq. {Frequency} = '{Word}'";

	}
}

