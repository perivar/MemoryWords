using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace MemoryWords
{
	class Program
	{
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
			
			//const string vocals = "[aeiouæøåh]"; // vocals and non-used characters
			const string vocals = "[aeiouæøå]"; // vocals and non-used characters
			var pattern = new StringBuilder();
			
			
			pattern.Append("^("); // start regexp and define first group
			pattern.Append(vocals).Append("*"); // zero or more vocals
			
			// the first digits
			int backReferenceCount = 2;
			string numPattern = null;
			for (int i = 0; i < digits.Length - 1; i++) {
				if ((numPattern = GetDigitRegexp(digits[i])) != null) {
					// one or two consecutive consonants using backreference
					// e.g. "(k)\2"
					pattern.Append("(").Append(numPattern).Append(")");
					pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number
					
					pattern.Append(vocals).Append("*"); // zero or more vocals
					backReferenceCount++;
				}
			}
			
			// the last digit
			if ((numPattern = GetDigitRegexp(digits[digits.Length - 1])) != null) {
				// one or two consecutive consonants using backreference
				// e.g. "(k)\3"
				pattern.Append("(").Append(numPattern).Append(")");
				pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number
				
				pattern.Append(vocals).Append("*"); // zero or more vocals
			}

			// end regexp and end group
			pattern.Append(")\r?$");
			
			var regExpression = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline);
			
			return regExpression;
		}
		
		private static int SearchBytes( byte[] haystack, byte[] needle ) {
			var len = needle.Length;
			var limit = haystack.Length - len;
			for( var i = 0;  i <= limit;  i++ ) {
				var k = 0;
				for( ;  k < len;  k++ ) {
					if( needle[k] != haystack[i+k] ) break;
				}
				if( k == len ) return i;
			}
			return -1;
		}
		
		private static void FindMatchingWords(string everything, byte[] allDigits, byte[] searchDigits, int wordCount) {
			
			Console.WriteLine("Searching for {0}", string.Join(",", searchDigits));
			
			var regExpression = GetRegexp(searchDigits);
			var matches = regExpression.Matches(everything);
			if (matches.Count > 0) {
				foreach(Match match in matches)
				{
					//Console.WriteLine("{0} found at position {1}", match.Groups[1], match.Index);
					for (int i = 0; i < wordCount - 1; i++) {
						Console.Write("\t");
					}
					Console.Write("{0}\n", match.Groups[1]);
				}
				
				// get remaining bytes and search again
				int startPos = SearchBytes(allDigits, searchDigits);
				
				// extract the remainding bytes
				int nextPos = startPos + searchDigits.Length;
				int length = allDigits.Length - nextPos;
				if (nextPos <  allDigits.Length) {
					var restDigits = new byte[length];
					Array.Copy(allDigits, nextPos, restDigits, 0, length);
					FindMatchingWords(everything, allDigits, restDigits, wordCount + 1);
				}
			} else {
				//Console.WriteLine("No matches, trying again...");
				
				// reduce the number of digits and try again
				searchDigits = searchDigits.Take(searchDigits.Count() - 1).ToArray();
				FindMatchingWords(everything, allDigits, searchDigits, wordCount);
			}
		}

		public static void Main(string[] args)
		{
			// test code: 53138552
			//byte[] digits = Digits(53138552);
			//byte[] digits = {7,1}; // akutt
			//byte[] digits = {3,1,4}; // amatør
			//byte[] digits = {8,1,4}; // føtter
			//byte[] digits = {4,6,2,2}; // regionen
			//byte[] digits = {7,4,8,1,8,4,7,1}; // kraftverket
			
			byte[] digits = Digits("231578963120");
			//byte[] digits = Digits("314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651");
			
			var Latin1 = Encoding.GetEncoding("iso-8859-1");
			const string inputFile = @"dict\norwegian.txt";
			string everything = File.ReadAllText(inputFile, Latin1);
			
			// divide into sections of x digits
			const int chunkSize = 10;
			byte[] buffer;
			for(int i = 0; i < digits.Length; i+=chunkSize)
			{
				if (chunkSize + i < digits.Length) {
					// use full chunk size
					buffer = new byte[chunkSize];
					Array.Copy(digits, i, buffer, 0, chunkSize);
				} else {
					// use the remaining number of digits
					int length = digits.Length - i;
					buffer = new byte[length];
					Array.Copy(digits, i, buffer, 0, length);
				}
				FindMatchingWords(everything, buffer, buffer, 1);
			}
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}