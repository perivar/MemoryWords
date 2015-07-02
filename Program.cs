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
		private static void LookupWords(string dictionary, byte[] searchDigits, ref int noDigitsProcessed, ref List<Word> wordList, bool outputProgess = false) {
			
			if (outputProgess) Console.WriteLine("Searching for chunk {0}", string.Join(",", searchDigits));
			
			var regExpression = GetRegexp(searchDigits);
			var matches = regExpression.Matches(dictionary);
			if (matches.Count > 0) {
				noDigitsProcessed = searchDigits.Length; // store the number of digits already processed
				
				if (outputProgess) Console.WriteLine("Found {0}", string.Join(",", searchDigits));
				
				var word = new Word(searchDigits);
				wordList.Add(word);
				foreach(Match match in matches)
				{
					//Console.WriteLine("{0} found at position {1}", match.Groups[1], match.Index);
					if (outputProgess) Console.Write("{0}\n", match.Groups[1].Value);
					word.WordCandidates.Add(match.Groups[1].Value);
				}
				Console.WriteLine();
			} else {
				// reduce the number of digits and try again
				int newLength = searchDigits.Count() - 1;
				if (newLength <= 0) return;
				
				searchDigits = searchDigits.Take(newLength).ToArray();
				LookupWords(dictionary, searchDigits, ref noDigitsProcessed, ref wordList, outputProgess);
			}
		}

		public static List<Word> FindWords(string dictionary, byte[] digits, bool outputProgess = false) {
			
			if (outputProgess) Console.WriteLine("Searching for {0}", string.Join(",", digits));
			
			// divide into sections of x digits
			const int chunkSize = 8;
			
			// variables
			var wordList = new List<Word>();
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
		
		private static void WriteCSV(string filePath, byte[] digits, List<Word> wordList) {
			
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
			
			const string inputFile = @"dict\norwegian.txt";
			string dictionary = File.ReadAllText(inputFile, _isoLatin1Encoding);
			var wordList = FindWords(dictionary, digits, true);

			//var nsfWords = new NSFOrdliste(@"dict\NSF-ordlisten\NSF-ordlisten.txt");
			//var wordList = FindWords(nsfWords.Nouns, digits, true);
			
			WriteCSV(@"pi.csv", digits, wordList);
			
			//string sentence = File.ReadAllText("pi.txt", _isoLatin1Encoding);
			//byte[] foundDigits = ParseDigits(sentence);
			//WriteCSV(@"pi2.csv", foundDigits, null);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
	
	#region Word class
	class Word {
		byte[] digits;
		readonly List<string> wordCandidates = new List<string>();
		
		public Word(byte[] digits) {
			this.digits = digits;
		}
		
		public byte[] Digits {
			get {
				return digits;
			}
		}

		public string DigitsAsString() {
			return string.Join(",", digits);
		}
		
		public List<string> WordCandidates {
			get {
				return wordCandidates;
			}
		}
		
		public override string ToString()
		{
			return string.Format("[{0}] Candidates={1}", string.Join(",", digits), wordCandidates.Count);
		}
	}
	#endregion
	
	class NSFOrdliste {
		
		string nouns; // Substantiv - an abstract or concrete entity; a person, place, thing, idea or quality
		string adjectives; // Adjektiv - a modifier of a noun or pronoun (big, brave)
		string pronouns; // Pronomen - a substitute for a noun or noun phrase (them, he)
		string verbs; // Verb - an action (walk), occurrence (happen), or state of being (be)
		string adverbs; // Adverb - a modifier of an adjective, verb, or other adverb (very, quite)
		string prepositions; // Preposisjon - aids in syntactic contex (in, of)
		string conjunctions; // Konjunksjon - links words, phrases, or clauses (and, but)
		string interjections; // Interjeksjon - greeting or exclamation (Hurrah, Alas)
		string subjunctions; // Subjunksjon – innleder bisetninger
		string articles; // Artikkel - definiteness (the) or indefiniteness (a, an)
		string determinatives; // Determinativ – bestemmer substantivet nærmere
		
		#region Getters
		public string Nouns {
			get {
				return nouns;
			}
		}

		public string Adjectives {
			get {
				return adjectives;
			}
		}

		public string Pronouns {
			get {
				return pronouns;
			}
		}

		public string Verbs {
			get {
				return verbs;
			}
		}

		public string Adverbs {
			get {
				return adverbs;
			}
		}

		public string Prepositions {
			get {
				return prepositions;
			}
		}

		public string Conjunctions {
			get {
				return conjunctions;
			}
		}

		public string Interjections {
			get {
				return interjections;
			}
		}

		public string Subjunctions {
			get {
				return subjunctions;
			}
		}

		public string Articles {
			get {
				return articles;
			}
		}

		public string Determinatives {
			get {
				return determinatives;
			}
		}
		#endregion
		
		public NSFOrdliste(string filePath) {
			
			// initialise HashSets
			var mynouns = new HashSet<string>();
			var myadjectives = new HashSet<string>();
			var mypronouns = new HashSet<string>();
			var myverbs = new HashSet<string>();
			var myadverbs = new HashSet<string>();
			var myprepositions = new HashSet<string>();
			var myconjunctions = new HashSet<string>();
			var myinterjections = new HashSet<string>();
			var mysubjunctions = new HashSet<string>();
			var myarticles = new HashSet<string>();
			var mydeterminatives = new HashSet<string>();
			
			// read file
			foreach (var line in File.ReadLines(filePath))
			{
				if (string.IsNullOrEmpty(line)) continue;

				var elements = line.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				string word = elements.First().ToLower();
				string wordClass = elements.Last();
				
				switch (wordClass) {
					case "subst":
						mynouns.Add(word);
						break;
					case "prep":
						myprepositions.Add(word);
						break;
					case "verb":
						myverbs.Add(word);
						break;
					case "adj":
					case "adjektiv":
						myadjectives.Add(word);
						break;
					case "adv":
						myadverbs.Add(word);
						break;
					case "konj":
						myconjunctions.Add(word);
						break;
					case "interj":
						myinterjections.Add(word);
						break;
					case "sbu":
						mysubjunctions.Add(word);
						break;
					case "det":
						mydeterminatives.Add(word);
						break;
					case "pron":
						mypronouns.Add(word);
						break;
					case "verbalsubst":
					case "numeral":
					case "nominal":
					case "musikkuttr":
					case "CLB":
						break;
					default:
						Console.WriteLine("woops {0}", wordClass);
						break;
				}
			}
			
			// load into strings
			nouns = string.Join("\r\n", mynouns.ToArray());
			adjectives = string.Join("\r\n", myadjectives.ToArray());
			pronouns = string.Join("\r\n", mypronouns.ToArray());
			verbs = string.Join("\r\n", myverbs.ToArray());
			adverbs = string.Join("\r\n", myadverbs.ToArray());
			prepositions = string.Join("\r\n", myprepositions.ToArray());
			conjunctions = string.Join("\r\n", myconjunctions.ToArray());
			interjections = string.Join("\r\n", myinterjections.ToArray());
			subjunctions = string.Join("\r\n", mysubjunctions.ToArray());
			articles = string.Join("\r\n", myarticles.ToArray());
			determinatives = string.Join("\r\n", mydeterminatives.ToArray());
		}
	}
}