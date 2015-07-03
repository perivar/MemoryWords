using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace MemoryWords
{
	class Program
	{
		static readonly Encoding _isoLatin1Encoding = Encoding.GetEncoding("ISO-8859-1");
		const string _vowels = "[aeiouyæøå]"; // vocals and non-used characters
		
		public static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}
		
		#region Methods to convert string and int to byte array
		static byte[] Digits(string str)
		{
			byte[] digits = Array.ConvertAll(str.ToArray(), c => (byte)Char.GetNumericValue(c));
			return digits;
		}
		
		static byte[] Digits(int num)
		{
			int nDigits = 1 + Convert.ToInt32(Math.Floor(Math.Log10(num)));
			var digits = new byte[nDigits];
			int index = nDigits - 1;
			while (num > 0) {
				byte digit = (byte) (num % 10);
				digits[index] = digit;
				num = num / 10;
				index = index - 1;
			}
			return digits;
		}
		#endregion
		
		#region Utility methods to create the regular expression for a digit or several digits
		static string GetDigitRegexp(byte singleDigit) {
			switch (singleDigit) {
				case 0:
					return "[sz]";
				case 1:
					return "[td]";
				case 2:
					return "n";
				case 3:
					return "m";
				case 4:
					return "r";
				case 5:
					return "l";
				case 6:
					return "[gj]";
				case 7:
					return "k";
				case 8:
					return "[fv]";
				case 9:
					return "[pb]";
				default:
					return null;
			}
		}
		
		static Regex GetRegexp(byte[] digits) {
			
			var pattern = new StringBuilder();
			
			pattern.Append("^("); // start regexp and define first group
			pattern.Append(_vowels).Append("*"); // zero or more vocals
			
			// the first digits
			int backReferenceCount = 2;
			string numPattern = null;
			for (int i = 0; i < digits.Length - 1; i++) {
				if ((numPattern = GetDigitRegexp(digits[i])) != null) {
					// one or two consecutive consonants using backreference
					// e.g. "(k)\2"
					pattern.Append("(").Append(numPattern).Append(")");
					pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number
					backReferenceCount++;
				}
				if (i + 1 < digits.Length) {
					// check if the next number is the same consonant,
					// if that is the case a vocal is mandatory, otherwise vocals are optional
					if (digits[i] == digits[i+1]) {
						pattern.Append(_vowels).Append("+"); // one or more vocals
					} else {
						pattern.Append(_vowels).Append("*"); // zero or more vocals
					}
				} else {
					pattern.Append(_vowels).Append("*"); // zero or more vocals
				}
			}
			
			// the last digit
			if ((numPattern = GetDigitRegexp(digits[digits.Length - 1])) != null) {
				// one or two consecutive consonants using backreference
				// e.g. "(k)\3"
				pattern.Append("(").Append(numPattern).Append(")");
				pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number
				
				pattern.Append(_vowels).Append("*"); // zero or more vocals
			}

			// end regexp and end group
			pattern.Append(")\r?$");
			
			var regExpression = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline);
			
			return regExpression;
		}
		#endregion
		
		#region from digits into words
		private static void LookupWords(string dictionary, byte[] searchDigits, ref int noDigitsProcessed, ref List<DigitsWords> wordList, bool outputProgess = false) {
			
			if (outputProgess) {
				ClearCurrentConsoleLine();
				Console.Write("Searching for {0}\r", string.Join(",", searchDigits));
			}
			
			var regExpression = GetRegexp(searchDigits);
			var matches = regExpression.Matches(dictionary);
			if (matches.Count > 0) {
				noDigitsProcessed = searchDigits.Length; // store the number of digits already processed
				
				if (outputProgess) {
					ClearCurrentConsoleLine();
					Console.Write("Found {0}: ", string.Join(",", searchDigits));
				}
				
				var word = new DigitsWords(searchDigits);
				wordList.Add(word);
				
				// Parallel.ForEach did not seem to be faster
				foreach(Match match in matches)
				{
					//Console.WriteLine("{0} found at position {1}", match.Groups[1], match.Index);
					if (outputProgess) Console.Write("{0}  ", match.Groups[1].Value);
					word.WordCandidates.Add(match.Groups[1].Value);
				}
				
				if (outputProgess) Console.Write("\n");
			} else {
				// reduce the number of digits and try again
				int newLength = searchDigits.Count() - 1;
				if (newLength <= 0) return;
				
				searchDigits = searchDigits.Take(newLength).ToArray();
				LookupWords(dictionary, searchDigits, ref noDigitsProcessed, ref wordList, outputProgess);
			}
		}

		public static List<DigitsWords> FindWords(string dictionary, byte[] digits, bool outputProgess = false) {
			
			if (outputProgess) Console.WriteLine("Converting into words: {0}", string.Join(",", digits));
			
			// divide into sections of x digits
			const int chunkSize = 8;
			
			// variables
			var wordList = new List<DigitsWords>();
			byte[] chunk;
			int processedNew = 0;
			int processedAlready = 0;
			int curIndex = 0;
			int remainingDigits = digits.Length;
			
			while (true) {
				curIndex = processedAlready + processedNew; // what index are we at
				processedAlready += processedNew; // store how many digits we have processed so far
				remainingDigits = digits.Length - curIndex;
				if (remainingDigits <= 0) break; // we have exceeded the end
				
				if (chunkSize + curIndex <= digits.Length) {
					// use chunk size
					chunk = new byte[chunkSize];
					Array.Copy(digits, curIndex, chunk, 0, chunkSize);
				} else {
					// use the remaining number of digits
					chunk = new byte[remainingDigits];
					Array.Copy(digits, curIndex, chunk, 0, remainingDigits);
				}
				LookupWords(dictionary, chunk, ref processedNew, ref wordList, outputProgess);
				if (processedNew == 0) break; // we found the last word
			}
			
			if (outputProgess) Console.WriteLine();
			
			return wordList;
		}
		#endregion

		#region from words into digits
		private static byte[] ParseDigits(string sentence) {
			var digits = new List<byte>();
			
			foreach (string word in sentence.Split(new string[] { "\r\n", "\n", " " }, StringSplitOptions.RemoveEmptyEntries)) {
				//Console.WriteLine(word);
				
				char lastCharacter = '.';
				foreach(char c in word) {
					char charLetter = char.ToLower(c);
					if (_vowels.Contains(charLetter)) {
						lastCharacter = charLetter;
					} else {
						if (lastCharacter == charLetter) continue;

						// consonant
						byte value = 255;
						switch (charLetter) {
							case 'h':
							case 'c':
							case 'w':
								// ignore
								break;
							case 'z':
							case 's':
								value = 0;
								break;
							case 'd':
							case 't':
								value = 1;
								break;
							case 'n':
								value = 2;
								break;
							case 'm':
								value = 3;
								break;
							case 'r':
								value = 4;
								break;
							case 'l':
								value = 5;
								break;
							case 'j':
							case 'g':
								value = 6;
								break;
							case 'k':
								value = 7;
								break;
							case 'f':
							case 'v':
								value = 8;
								break;
							case 'p':
							case 'b':
								value = 9;
								break;
							default:
								//Console.WriteLine("Error, found {0} letter!", charLetter);
								break;
						}
						if (value != 255) digits.Add(value);
						lastCharacter = charLetter;
					}
				}
			}
			
			return digits.ToArray();
		}
		#endregion
		
		private static string WordListAsString(List<DigitsWords> wordList) {
			var sentenceList = (from list in wordList
			                    select list.WordCandidates.First()
			                   );
			return string.Join("\r\n", sentenceList);
		}
		
		private static void WriteCSV(string filePath, byte[] digits, List<DigitsWords> wordList) {
			
			const string columnSeparator = ",";
			
			TextWriter pw = new StreamWriter(filePath, false, _isoLatin1Encoding);

			// header
			pw.Write("\"{0}\"", string.Join(",", digits));
			pw.Write("\r");
			
			// rows and columns
			if (wordList != null) {
				foreach(var word in wordList) {
					pw.Write("\"{0}\"{1}", word.DigitsAsString(), columnSeparator);
					foreach(var wordCandidate in word.WordCandidates) {
						pw.Write("\"{0}\"{1}", wordCandidate, columnSeparator);
					}
					pw.Write("\r");
				}
			}
			pw.Close();
		}
		
		public static void Main(string[] args)
		{
			// test code: 53138552
			byte[] digits = Digits(53138552);
			//byte[] digits = Digits(5511);
			//byte[] digits = {7,1}; // akutt
			//byte[] digits = {3,1,4}; // amatør
			//byte[] digits = {8,1,4}; // føtter
			//byte[] digits = {4,6,2,2}; // regionen
			//byte[] digits = {7,4,8,1,8,4,7,1}; // kraftverket
			
			//byte[] digits = Digits("231578963120");
			//byte[] digits = Digits("314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651");
			//byte[] digits = Digits("314159265358979");
			//byte[] digits = Digits("05721184084105791485091641940564691959501972917284624120958121584129557914181849214842841226184084612846419491230290748494107908502039150121418543492858012042158641202054157914203849850032319410790149284059511014718531484972");
			
			#region Run Tests
			const string inputFile5k = @"dict\wiktionary_frequency_list.txt";
			var freqList5k = new FrequencyLists();
			freqList5k.Read5KFormat(inputFile5k);
			var wordList5k = FindWords(freqList5k.Words, digits, true);
			WriteCSV(@"pi_list5k.csv", digits, wordList5k);
			string digitsSentence5k = WordListAsString(wordList5k);
			byte[] foundDigits5k = ParseDigits(digitsSentence5k);
			if (!digits.SequenceEqual(foundDigits5k)) Console.WriteLine("FAILED - Not identical");
			
			const string inputFile10000 = @"dict\ord10000.txt";
			var freqList10000 = new FrequencyLists();
			freqList10000.Read10000Format(inputFile10000);
			var wordList10000 = FindWords(freqList10000.Words, digits, true);
			WriteCSV(@"pi_list10000.csv", digits, wordList10000);
			string digitsSentence10000 = WordListAsString(wordList10000);
			byte[] foundDigits10000 = ParseDigits(digitsSentence10000);
			if (!digits.SequenceEqual(foundDigits10000)) Console.WriteLine("FAILED - Not identical");
			
			const string inputFile10k = @"dict\ord10k.csv";
			var freqList10k = new FrequencyLists();
			freqList10k.Read10KFormat(inputFile10k);
			var wordList10k = FindWords(freqList10k.Words, digits, true);
			WriteCSV(@"pi_list10k.csv", digits, wordList10k);
			string digitsSentence10K = WordListAsString(wordList10k);
			byte[] foundDigits10K = ParseDigits(digitsSentence10K);
			if (!digits.SequenceEqual(foundDigits10K)) Console.WriteLine("FAILED - Not identical");
			
			const string inputFile62K = @"dict\norwegian.txt";
			string dictionary62K = File.ReadAllText(inputFile62K, _isoLatin1Encoding);
			var wordList62K = FindWords(dictionary62K, digits, true);
			WriteCSV(@"pi_list62K.csv", digits, wordList62K);
			string digitsSentence62K = WordListAsString(wordList62K);
			byte[] foundDigits62K = ParseDigits(digitsSentence62K);
			if (!digits.SequenceEqual(foundDigits62K)) Console.WriteLine("FAILED - Not identical");
			
			// THESE ARE VERY SLOW
			/*
			var nsfWords = new NSFOrdliste(@"dict\NSF-ordlisten\NSF-ordlisten.txt");
			var wordListNSF = FindWords(nsfWords.Nouns, digits, true);
			WriteCSV(@"pi_nsf_list.csv", digits, wordListNSF);

			const string inputFileLarge = @"dict\norwegian_large.txt";
			string dictionaryLarge = File.ReadAllText(inputFileLarge, _isoLatin1Encoding);
			var wordListLarge = FindWords(dictionaryLarge, digits, true);
			WriteCSV(@"pi_large_list.csv", digits, wordListLarge);
			 */
			#endregion
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
	
	
}