﻿﻿using System.Text;
using System.Text.RegularExpressions;

namespace MemoryWords
{
    public class Program
    {
        private static readonly Encoding _isoLatin1Encoding = Encoding.GetEncoding("ISO-8859-1");
        private static readonly Encoding _utf8Encoding = Encoding.UTF8;

        private const string _vowels = "[aeiouyæøåhjwc]"; // vowels and non-used characters (hjwc)

        private const string _zeroOrMoreVowels = _vowels + "*";
        private const string _oneOrMoreVowels = _vowels + "+";

        private static readonly string DictPath = Path.Combine("dict");

        /// <summary>
        /// Clears the current line in the console.
        /// </summary>
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        #region Methods to convert string and int to byte array
        /// <summary>
        /// Converts a string of digits to a byte array.
        /// </summary>
        static byte[] Digits(string str)
        {
            byte[] digits = Array.ConvertAll(str.ToArray(), c => (byte)Char.GetNumericValue(c));
            return digits;
        }

        /// <summary>
        /// Converts an integer to a byte array of digits.
        /// </summary>
        static byte[] Digits(int num)
        {
            int nDigits = 1 + Convert.ToInt32(Math.Floor(Math.Log10(num)));
            var digits = new byte[nDigits];
            int index = nDigits - 1;
            while (num > 0)
            {
                byte digit = (byte)(num % 10);
                digits[index] = digit;
                num = num / 10;
                index = index - 1;
            }
            return digits;
        }
        #endregion

        #region Utility methods to create the regular expression for a digit or several digits
        /// <summary>
        /// Returns a regular expression string for a single digit.
        /// </summary>
        static string? GetDigitRegexp(byte singleDigit)
        {
            switch (singleDigit)
            {
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
                    return "((s(k)?j)|(kj)|(tj))";
                case 7:
                    return "[kg]";
                case 8:
                    return "[fv]";
                case 9:
                    return "[pb]";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a regular expression for a sequence of digits.
        /// The regular expression matches words that can be formed from the digits,
        /// where each digit corresponds to a set of consonants.
        /// </summary>
        private static readonly Dictionary<string, Regex> _regexCache = new();

        public static Regex GetRegexp(byte[] digits)
        {
            // Use the byte array as string for cache key
            string key = string.Join(",", digits);
            if (_regexCache.TryGetValue(key, out var cachedRegex))
            {
                return cachedRegex;
            }

            var pattern = new StringBuilder();

            pattern.Append("^("); // start regexp and define first group
            pattern.Append(_zeroOrMoreVowels); // zero or more vowels

            // the first digits
            int backReferenceCount = 2;
            string? numPattern = null;
            for (int i = 0; i < digits.Length - 1; i++)
            {
                if ((numPattern = GetDigitRegexp(digits[i])) != null)
                {
                    // one or two consecutive consonants using backreference
                    // e.g. "(k)\2"
                    pattern.Append("(").Append(numPattern).Append(")");
                    pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number
                    backReferenceCount++;
                }
                if (i + 1 < digits.Length)
                {
                    // check if the next number is the same consonant,
                    // if that is the case a vowel is mandatory, otherwise vowels are optional
                    if (digits[i] == digits[i + 1])
                    {
                        pattern.Append(_oneOrMoreVowels); // one or more vowels
                    }
                    else
                    {
                        pattern.Append(_zeroOrMoreVowels); // zero or more vowels
                    }
                }
                else
                {
                    pattern.Append(_zeroOrMoreVowels); // zero or more vowels
                }
            }

            // the last digit
            if ((numPattern = GetDigitRegexp(digits[digits.Length - 1])) != null)
            {
                // one or two consecutive consonants using backreference
                // e.g. "(k)\3"
                pattern.Append("(").Append(numPattern).Append(")");
                pattern.Append("\\").Append(backReferenceCount).Append("?"); // use backreference to group number

                pattern.Append(_zeroOrMoreVowels); // zero or more vowels
            }

            // end regexp and end group
            pattern.Append(")\\r?$");

            var regExpression = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline);

            // Cache the compiled regex
            _regexCache[key] = regExpression;

            return regExpression;
        }
        #endregion

        #region from digits into words
        /// <summary>
        /// Looks up words in the dictionary that match the given digits.
        /// This method recursively reduces the number of digits if no match is found.
        /// </summary>
        private static void LookupWords(string[] dictionary, byte[] searchDigits, ref int noDigitsProcessed, ref List<DigitsWords> wordList, bool outputProgess = false)
        {
            if (outputProgess)
            {
                ClearCurrentConsoleLine();
                Console.Write("Searching for {0}\r", string.Join(",", searchDigits));
            }

            var regExpression = GetRegexp(searchDigits);
            var matches = new List<string>();

            // Build string from dictionary array for regex matching
            var dictionaryString = string.Join("\r\n", dictionary);
            var regexMatches = regExpression.Matches(dictionaryString);

            if (regexMatches.Count > 0)
            {
                noDigitsProcessed = searchDigits.Length; // store the number of digits already processed

                if (outputProgess)
                {
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Found digits {0}:", string.Join(",", searchDigits));
                }

                var word = new DigitsWords(searchDigits);
                wordList.Add(word);

                // Calculate the maximum word length for alignment
                int maxLength = regexMatches.Cast<Match>()
                    .Max(m => m.Groups[1].Value.Length);

                // Number of words per line (adjust based on console width)
                const int wordsPerLine = 4;
                int currentWordCount = 0;

                foreach (Match match in regexMatches)
                {
                    if (outputProgess)
                    {
                        if (currentWordCount == 0)
                        {
                            Console.Write("    "); // Indent for visual hierarchy
                        }

                        string formattedWord = match.Groups[1].Value.PadRight(maxLength);
                        Console.Write(formattedWord);
                        Console.Write("  "); // Space between words

                        currentWordCount++;
                        if (currentWordCount >= wordsPerLine)
                        {
                            Console.WriteLine(); // New line after wordsPerLine words
                            currentWordCount = 0;
                        }
                    }
                    word.WordCandidates.Add(match.Groups[1].Value);
                }

                if (outputProgess)
                {
                    if (currentWordCount > 0)
                    {
                        Console.WriteLine(); // Ensure we end with a newline
                    }
                    Console.WriteLine(); // Add extra line for spacing between different digit groups
                }
            }
            else
            {
                // reduce the number of digits and try again
                int newLength = searchDigits.Count() - 1;
                if (newLength <= 0) return;

                searchDigits = searchDigits.Take(newLength).ToArray();
                LookupWords(dictionary, searchDigits, ref noDigitsProcessed, ref wordList, outputProgess);
            }
        }

        /// <summary>
        /// Extracts the next chunk of bytes from the source array, starting at the specified index.
        /// If there are fewer bytes remaining than maxChunkSize, returns all remaining bytes.
        /// </summary>
        /// <param name="source">The source byte array to extract from</param>
        /// <param name="startIndex">The starting index in the source array</param>
        /// <param name="maxChunkSize">The maximum size of the chunk to extract</param>
        /// <returns>A new byte array containing the extracted chunk</returns>
        private static byte[] GetNextChunk(byte[] source, int startIndex, int maxChunkSize)
        {
            int remainingLength = source.Length - startIndex;
            int chunkSize = Math.Min(maxChunkSize, remainingLength);
            var chunk = new byte[chunkSize];
            Array.Copy(source, startIndex, chunk, 0, chunkSize);
            return chunk;
        }

        /// <summary>
        /// Finds words in the dictionary that can be formed from the given digits.
        /// The digits are divided into chunks, and the LookupWords method is used to find matching words.
        /// </summary>
        public static List<DigitsWords> FindWords(string[] dictionary, byte[] digits, bool outputProgress = false)
        {
            // if (outputProgress)
            //     Console.WriteLine("Converting into words: {0}", string.Join(",", digits));

            const int maxChunkSize = 16;
            var wordList = new List<DigitsWords>();
            int digitsProcessed = 0;
            int lastProcessedCount = 0;

            while (digitsProcessed < digits.Length)
            {
                var currentChunk = GetNextChunk(digits, digitsProcessed, maxChunkSize);
                LookupWords(dictionary, currentChunk, ref lastProcessedCount, ref wordList, outputProgress);

                if (lastProcessedCount == 0)
                    break; // No more words found

                digitsProcessed += lastProcessedCount;
            }

            if (outputProgress)
                Console.WriteLine();

            return wordList;
        }
        #endregion

        #region from words into digits

        private static readonly Dictionary<string, byte> PhonemeToDigit = new Dictionary<string, byte>
        {
            { "s", 0 }, { "z", 0 },
            { "t", 1 }, { "d", 1 },
            { "n", 2 },
            { "m", 3 },
            { "r", 4 },
            { "l", 5 },
            { "sj", 6 }, { "skj", 6 }, { "tj", 6 }, { "kj", 6 },
            { "k", 7 }, { "g", 7 },
            { "f", 8 }, { "v", 8 },
            { "p", 9 }, { "b", 9 }
        };

        /// <summary>
        /// Parses a sentence into a byte array of digits.
        /// </summary>
        public static byte[] ParseDigits(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                return Array.Empty<byte>();

            // Konverter til små bokstaver for enklere matching
            sentence = sentence.ToLowerInvariant();
            List<byte> digits = new List<byte>();

            // Match phonemes and ignored characters (vowels and spaces)
            string pattern = @"(sj|skj|tj|kj|[sztdnmrlkgfvpb])|(" + _oneOrMoreVowels + @"|\s+)";
            MatchCollection matches = Regex.Matches(sentence, pattern);

            byte? lastDigit = null;
            bool hasVowelOrSpaceSinceLastDigit = false;

            foreach (Match match in matches)
            {
                // Check if this is a vowel or space
                if (match.Groups[2].Success)
                {
                    hasVowelOrSpaceSinceLastDigit = true;
                    continue;
                }

                string phoneme = match.Groups[1].Value;
                if (PhonemeToDigit.ContainsKey(phoneme))
                {
                    byte currentDigit = PhonemeToDigit[phoneme];
                    // Add the digit if it's different from the last one OR if we've seen vowels/spaces since the last digit
                    if (!lastDigit.HasValue || lastDigit.Value != currentDigit || hasVowelOrSpaceSinceLastDigit)
                    {
                        digits.Add(currentDigit);
                        lastDigit = currentDigit;
                        hasVowelOrSpaceSinceLastDigit = false;
                    }
                }
            }

            return digits.ToArray();
        }

        // Converts the sentence to a mnemonic string for explanation
        public static string ToMnemonic(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                return string.Empty;

            sentence = sentence.ToLowerInvariant();
            List<string> mnemonicParts = new List<string>();
            string pattern = @"(sj|skj|tj|kj|[sztdnmrlkgfvpb])|(" + _oneOrMoreVowels + @"|\s+)";
            MatchCollection matches = Regex.Matches(sentence, pattern);

            byte? lastDigit = null;
            string? lastPhoneme = null;
            bool hasVowelOrSpaceSinceLastDigit = false;

            foreach (Match match in matches)
            {
                // Check if this is a vowel or space
                if (match.Groups[2].Success)
                {
                    hasVowelOrSpaceSinceLastDigit = true;
                    // Show spaces as [space], other characters in brackets
                    if (string.IsNullOrWhiteSpace(match.Value))
                        mnemonicParts.Add("[space]");
                    else
                        mnemonicParts.Add($"[{match.Value}]"); // Show vowels in brackets
                    continue;
                }

                string phoneme = match.Groups[1].Value;
                if (PhonemeToDigit.ContainsKey(phoneme))
                {
                    byte currentDigit = PhonemeToDigit[phoneme];
                    // Add the phoneme if it's different from the last one OR if we've seen vowels since the last digit
                    if (!lastDigit.HasValue || lastDigit.Value != currentDigit || hasVowelOrSpaceSinceLastDigit)
                    {
                        mnemonicParts.Add($"{phoneme}({currentDigit})");
                        lastDigit = currentDigit;
                        lastPhoneme = phoneme;
                        hasVowelOrSpaceSinceLastDigit = false;
                    }
                    else
                    {
                        // Replace the last part to show combined consonants
                        mnemonicParts[mnemonicParts.Count - 1] = $"{lastPhoneme}{phoneme}({currentDigit})";
                    }
                }
            }

            return string.Join(" ", mnemonicParts);
        }
        #endregion

        /// <summary>
        /// Converts a list of DigitsWords to a string, by extracting the first word candidate from each DigitsWords object and joining them with a newline character.
        /// </summary>
        private static string WordListAsString(List<DigitsWords> wordList)
        {
            var sentenceList = from list in wordList
                               select list.WordCandidates.First()
                             ;
            return string.Join(" ", sentenceList);
        }

        /// <summary>
        /// Verifies that the word list correctly represents the expected digits by converting
        /// the words back to digits and comparing with the original.
        /// </summary>
        /// <param name="expectedDigits">The original digits that should be represented</param>
        /// <param name="wordList">The list of words to verify</param>
        public static bool VerifyResults(byte[] expectedDigits, List<DigitsWords> wordList)
        {
            string digitsSentence = WordListAsString(wordList);
            byte[] foundDigits = ParseDigits(digitsSentence);
            bool isMatch = expectedDigits.SequenceEqual(foundDigits);

            if (!isMatch)
            {
                Console.WriteLine("Failed - Sequences not identical");

                // Numbers are always single digits, use fixed width of 1
                // Create position indicators
                // Pad each number to occupy exactly 3 spaces
                string positions = string.Join(" ", Enumerable.Range(0, Math.Max(expectedDigits.Length, foundDigits.Length))
                    .Select(i => i.ToString().PadLeft(3)));
                Console.WriteLine($"Position: {positions}");

                // Format the sequences with 3-space padding
                string expected = string.Join(" ", expectedDigits.Select(d => d.ToString().PadLeft(3)));
                string found = string.Join(" ", foundDigits.Select(d => d.ToString().PadLeft(3)));
                Console.WriteLine($"Expected: {expected}");
                Console.WriteLine($"Found   : {found}");

                // Generate difference markers
                var diffMarkers = new List<string>();
                int minLength = Math.Min(expectedDigits.Length, foundDigits.Length);
                for (int i = 0; i < Math.Max(expectedDigits.Length, foundDigits.Length); i++)
                {
                    if (i >= minLength)
                        diffMarkers.Add("^"); // Different length
                    else if (expectedDigits[i] != foundDigits[i])
                        diffMarkers.Add("^"); // Different values
                    else
                        diffMarkers.Add(" "); // Same
                }
                // Join markers with same 3-space width
                Console.WriteLine($"Diff    : {string.Join(" ", diffMarkers.Select(m => m.PadLeft(3)))}");

                Console.WriteLine($"Word    : {digitsSentence}");
                Console.WriteLine($"Mnemonic: {ToMnemonic(digitsSentence)}");
            }

            return isMatch;
        }

        /// <summary>
        /// Writes the digits and word list to a CSV file.
        /// </summary>
        private static void WriteCSV(string outputDir, string filename, byte[] digits, List<DigitsWords>? wordList)
        {
            const string columnSeparator = ",";

            // Create the output directory if it doesn't exist
            Directory.CreateDirectory(outputDir);

            // Combine the output directory and filename
            string fullPath = Path.Combine(outputDir, filename);

            TextWriter pw = new StreamWriter(fullPath, false, _isoLatin1Encoding);

            // header
            pw.Write("\"{0}\"", string.Join(",", digits));
            pw.Write("\r");

            // rows and columns
            if (wordList != null)
            {
                foreach (var word in wordList)
                {
                    pw.Write("\"{0}\"{1}", word.DigitsAsString(), columnSeparator);
                    foreach (var wordCandidate in word.WordCandidates)
                    {
                        pw.Write("\"{0}\"{1}", wordCandidate, columnSeparator);
                    }
                    pw.Write("\r");
                }
            }
            pw.Close();
        }

        /// <summary>
        /// The main entry point of the application.
        /// This method performs tests using different dictionaries and digits based on command line arguments.
        /// E.g. use dotnet run -- -a 314159265358
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine("Usage: MemoryWords [options] [digits]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help                  Show help message and exit");
                Console.WriteLine("  -r, --rules                 Show the mnemonic major system rules");
                Console.WriteLine("  -s, --string <text>         Parse the given text into digits using ParseDigits");
                Console.WriteLine("  -d, --dictionary <name>     Use the specified dictionary (wiktionary, ord10000, ord10k, norwegian, norwegian_large, nsf2012, nsf2023) [default: nsf2023]");
                Console.WriteLine("  -a, --all                   Use all dictionaries");
                Console.WriteLine("  -c, --csv                   Enable CSV output");
                Console.WriteLine("Digits: A string of digits to convert into words (default: pi digits)");
                return;
            }

            // Check for -r/--rules flag first
            if (args.Contains("-r") || args.Contains("--rules"))
            {
                Console.WriteLine("The Mnemonic major system is a memory technique that converts numbers into consonant sounds,");
                Console.WriteLine("which can then be combined with vowels to create words. This makes it easier to remember long");
                Console.WriteLine("sequences of numbers by converting them into more memorable words or phrases.\n");

                Console.WriteLine("Original System:\n");
                Console.WriteLine("0 = S, Z        Zero begins with Z. Other letters sound similar when spoken.");
                Console.WriteLine("1 = T, D        t and d each have one downstroke, and sound similar when voiced.");
                Console.WriteLine("2 = N           n has two downstrokes when written.");
                Console.WriteLine("3 = M           m has three downstrokes when written.");
                Console.WriteLine("4 = R           Last letter of 'fouR'");
                Console.WriteLine("5 = L           Roman numeral 'L' = 50");
                Console.WriteLine("6 = CH, J, SH   j has a curve near the bottom, like 6 does");
                Console.WriteLine("7 = K, G        'K' contains two '7's");
                Console.WriteLine("8 = F, V        I associate V with V8. F sounds like V when spoken.'F' looks like '8'");
                Console.WriteLine("9 = P, B        9 rotated 180 degrees looks like b. 9 flipped horizontally looks like P.\n");

                Console.WriteLine("Norwegian Adaptation:\n");
                Console.WriteLine("0 = S, Z        S som i 'Sirkel' eller '0' på engelsk 'ZERO'");
                Console.WriteLine("1 = T, D        t og d har én nedstrek");
                Console.WriteLine("2 = N           n har to nedstreker");
                Console.WriteLine("3 = M           m har to nedstreker");
                Console.WriteLine("4 = R           tenk på 'fiRe', eller 'R som i rein, fire bein'");
                Console.WriteLine("5 = L           Romertallet 'L' er 50");
                Console.WriteLine("6 = SJ, KJ, TJ  'Sjø', 'Skje', 'Kjede', 'Tjue'. J har en kurve nederst slik som 6 har");
                Console.WriteLine("7 = K, G        'K' inneholder to '7'-tall");
                Console.WriteLine("8 = F, V        Jeg assosierer V med V8. F lyder som V når den uttales. 'F' ligner på '8'");
                Console.WriteLine("9 = P, B        9 rotert 180 grader ser ut som b. 9 speilvendt ser ut som P.\n");

                Console.WriteLine("Note: Vowels (A, E, I, O, U, Y, Æ, Ø, Å) and some consonants (H, W, C) are ignored,");
                return;
            }

            // Check for -s/--string flag next
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "-s" || args[i] == "--string") && i + 1 < args.Length)
                {
                    string input = args[i + 1];
                    byte[] parsedDigits = ParseDigits(input);
                    string mnemonic = ToMnemonic(input);
                    Console.WriteLine("Input: {0}", input);
                    Console.WriteLine("Digits: {0}", string.Join(", ", parsedDigits));
                    Console.WriteLine("Mnemonic: {0}", mnemonic);
                    return;
                }
            }

            // Dictionary name mappings
            var dictionaryMappings = new Dictionary<string, string>
            {
                ["wiktionary"] = "wiktionary_frequency_list.txt",
                ["ord10000"] = "ord10000.txt",
                ["ord10k"] = "ord10k.csv",
                ["norwegian"] = "norwegian.txt",
                ["norwegian_large"] = "norwegian_large.txt",
                ["nsf2012"] = "nsf2012.txt",
                ["nsf2023"] = "nsf2023.txt"
            };

            bool outputCSV = false;
            const string outputCSVDir = "output";

            byte[]? digits = null;
            string? dictionaryName = null;
            bool useAllDictionaries = false;
            dictionaryName = "nsf2023"; // Set default dictionary

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-d" || args[i] == "--dictionary")
                {
                    if (i + 1 < args.Length)
                    {
                        dictionaryName = args[i + 1];
                        useAllDictionaries = false;
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing dictionary name after --dictionary");
                        return;
                    }
                }
                else if (args[i] == "-c" || args[i] == "--csv")
                {
                    outputCSV = true;
                }
                else if (args[i] == "-a" || args[i] == "--all")
                {
                    useAllDictionaries = true;
                }
                else if (digits == null)
                {
                    try
                    {
                        digits = Digits(args[i]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error: Invalid digits format");
                        return;
                    }
                }
            }

            if (digits == null)
            {
                // string defaultDigits = "71"; // akutt
                // string defaultDigits = "314"; // amatør
                // string defaultDigits = "814"; // føtter
                // string defaultDigits = "4622"; // regionen
                // string defaultDigits = "74818471"; // kraftverket
                // string defaultDigits = "034101521"; // smertestillende
                // string defaultDigits = "03918284226"; // småbåtvenneforening
                string defaultDigits = "314159265358"; // PI 11 digits
                // string defaultDigits = "31415926535897932384"; // PI 20 digits
                // string defaultDigits = "314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651";
                // string defaultDigits = "05721184084105791485091641940564691959501972917284624120958121584129557914181849214842841226184084612846419491230290748494107908502039150121418543492858012042158641202054157914203849850032319410790149284059511014718531484972";
                Console.WriteLine("Could not find any digits to use. Using default digits {0}\n", defaultDigits);
                digits = Digits(defaultDigits);
            }

            // Start timer for execution time measurement
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Execute dictionary operations based on command line arguments
            if (!useAllDictionaries && dictionaryName != null)
            {
                string fullDictionaryName = dictionaryName;
                if (dictionaryMappings.ContainsKey(dictionaryName.ToLower()))
                {
                    fullDictionaryName = dictionaryMappings[dictionaryName.ToLower()];
                }

                if (dictionaryMappings.ContainsValue(fullDictionaryName))
                {
                    ExecuteDictionary(fullDictionaryName, digits, outputCSVDir, outputCSV);
                }
                else
                {
                    Console.WriteLine("Error: Unknown dictionary {0}", dictionaryName);
                    Console.WriteLine("Available dictionaries: wiktionary, ord10000, ord10k, norwegian, norwegian_large, nsf2012, nsf2023");
                }
            }
            else
            {
                // Execute all dictionaries
                foreach (string dictionary in dictionaryMappings.Values)
                {
                    ExecuteDictionary(dictionary, digits, outputCSVDir, outputCSV);
                }
            }

            // Stop timer and output execution time
            stopwatch.Stop();
            Console.WriteLine("\nTotal execution time: {0}ms", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Executes word lookup on a specified dictionary.
        /// Handles different dictionary formats (frequency lists, word lists, NSF dictionaries)
        /// and performs the word lookup, verification, and optional CSV output.
        /// </summary>
        /// <param name="dictionaryName">The name of the dictionary file to use</param>
        /// <param name="digits">The digits to convert into words</param>
        /// <param name="outputCSVDir">The directory where to write output files</param>
        /// <param name="outputCSV">Whether to write results to CSV files (default: false)</param>
        private static void ExecuteDictionary(string dictionaryName, byte[] digits, string outputCSVDir, bool outputCSV = false)
        {
            Console.WriteLine("Executing using: {0}", dictionaryName);
            string outFileName = $"pi_list_{Path.GetFileNameWithoutExtension(dictionaryName)}.csv";

            switch (dictionaryName)
            {
                // Read a dictionary stored as a frequency list
                case "wiktionary_frequency_list.txt":
                case "ord10000.txt":
                case "ord10k.csv":
                    var freqList = new FrequencyLists();
                    string inputFile = Path.Combine(DictPath, dictionaryName);
                    if (dictionaryName == "wiktionary_frequency_list.txt")
                        freqList.ReadWiktionary5KFormat(inputFile);
                    else if (dictionaryName == "ord10000.txt")
                        freqList.ReadOrd10000Format(inputFile);
                    else
                        freqList.ReadOrd10KFormat(inputFile);

                    var wordList = FindWords(freqList.Words, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                // read a norwegian dictionary that contain a list of words
                case "norwegian.txt":
                case "norwegian_large.txt":
                    string norwegianText = File.ReadAllText(Path.Combine(DictPath, dictionaryName), _isoLatin1Encoding);
                    var norwegianWords = norwegianText.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
                    wordList = FindWords(norwegianWords, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                // read the nsf dictionary in 2012 format
                case "nsf2012.txt":
                    var nsf2012 = new NSF2012(Path.Combine(Path.Combine(DictPath, "nsf2012"), dictionaryName));
                    wordList = FindWords(nsf2012.Nouns, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                // read the nsf dictionary in 2023 format (as list of words)
                case "nsf2023.txt":
                    string nsf2023Text = File.ReadAllText(Path.Combine(Path.Combine(DictPath, "nsf2023"), dictionaryName), _utf8Encoding);
                    var nsf2023Words = nsf2023Text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
                    wordList = FindWords(nsf2023Words, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                default:
                    Console.WriteLine("Error: Unknown dictionary {0}", dictionaryName);
                    break;
            }
        }
    }
}