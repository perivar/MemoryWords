﻿using System.Text;
namespace MemoryWords
{
	public class FrequencyLists
	{
		static readonly Encoding _isoLatin1Encoding = Encoding.GetEncoding("ISO-8859-1");
		List<WordElement> wordElements = new List<WordElement>();
		string words = "";

		public string Words
		{
			get
			{
				return words;
			}
		}

		/// <summary>
		/// Reads a frequency list from a file in the ord10k.csv format.
		/// The file is a CSV file with the following columns:
		/// Place, Frequency, Percentage, Cumulated, Word
		/// The first line is a header and is skipped.
		/// </summary>
		public void ReadOrd10KFormat(string filePath, bool ignoreSingleLetterWords = false)
		{

			var sb = new StringBuilder();
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
				var elements = line.Split(new String[] {
											  ","
										  }, StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[4].Length < 2) continue;

				var word = new WordElement();
				wordElements.Add(word);

				word.Place = int.Parse(elements[0]);
				word.Frequency = int.Parse(elements[1]);
				word.Word = elements[4];

				// also add to the string
				sb.Append(word.Word);
				sb.Append("\r\n");
			}

			words = sb.ToString();
		}

		/// <summary>
		/// Reads a frequency list from a file in the wiktionary_frequency_list.txt format.
		/// The file is a space-separated file with the following columns:
		/// Word, Frequency
		/// </summary>
		public void ReadWiktionary5KFormat(string filePath, bool ignoreSingleLetterWords = false)
		{

			var sb = new StringBuilder();
			int lineCounter = 0;

			// read in the dictionary file in the wiktionary_frequency_list.txt format
			foreach (var line in File.ReadLines(filePath))
			{
				lineCounter++;

				// ignore blank lines
				if (string.IsNullOrEmpty(line))
					continue;

				// parse
				var elements = line.Split(new String[] {
											  " "
										  }, StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[0].Length < 2) continue;

				var word = new WordElement();
				wordElements.Add(word);

				word.Place = lineCounter;
				word.Frequency = int.Parse(elements[1]);
				word.Word = elements[0];

				// also add to the string
				sb.Append(word.Word);
				sb.Append("\r\n");
			}

			words = sb.ToString();
		}

		/// <summary>
		/// Reads a frequency list from a file in the ord10000.txt format.
		/// The file is a space-separated file with the following columns:
		/// Frequency, Word
		/// </summary>
		public void ReadOrd10000Format(string filePath, bool ignoreSingleLetterWords = false)
		{

			var sb = new StringBuilder();
			int lineCounter = 0;

			// read in the dictionary file in the ord10000.txt format
			foreach (var line in File.ReadLines(filePath, _isoLatin1Encoding))
			{
				lineCounter++;

				// ignore blank lines
				if (string.IsNullOrEmpty(line))
					continue;

				// parse
				var elements = line.Split(new String[] {
											  " "
										  }, StringSplitOptions.RemoveEmptyEntries);

				// ignore words with only one letter or digit
				if (ignoreSingleLetterWords && elements[1].Length < 2) continue;

				var word = new WordElement();
				wordElements.Add(word);

				word.Place = lineCounter;
				word.Frequency = int.Parse(elements[0]);
				word.Word = elements[1];

				// also add to the string
				sb.Append(word.Word);
				sb.Append("\r\n");
			}

			words = sb.ToString();
		}
	}

	public class WordElement
	{
		public int Frequency
		{
			get;
			set;
		}

		public string? Word
		{
			get;
			set;
		}

		public int Place
		{
			get;
			set;
		}

		public override string ToString()
		{
			return string.Format("Place {0}, Freq. {1} = '{2}'", Place, Frequency, Word);
		}

	}
}

