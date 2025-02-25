﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Text;
using System.Text.RegularExpressions;

namespace MemoryWords
{
    class Program
    {
        static readonly Encoding _isoLatin1Encoding = Encoding.GetEncoding("ISO-8859-1");
        const string _vowels = "[aeiouyæøå]"; // vocals and non-used characters

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

        /// <summary>
        /// Creates a regular expression for a sequence of digits.
        /// The regular expression matches words that can be formed from the digits,
        /// where each digit corresponds to a set of consonants.
        /// </summary>
        static Regex GetRegexp(byte[] digits)
        {
            var pattern = new StringBuilder();

            pattern.Append("^("); // start regexp and define first group
            pattern.Append(_vowels).Append("*"); // zero or more vocals

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
                    // if that is the case a vocal is mandatory, otherwise vocals are optional
                    if (digits[i] == digits[i + 1])
                    {
                        pattern.Append(_vowels).Append("+"); // one or more vocals
                    }
                    else
                    {
                        pattern.Append(_vowels).Append("*"); // zero or more vocals
                    }
                }
                else
                {
                    pattern.Append(_vowels).Append("*"); // zero or more vocals
                }
            }

            // the last digit
            if ((numPattern = GetDigitRegexp(digits[digits.Length - 1])) != null)
            {
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
        /// <summary>
        /// Looks up words in the dictionary that match the given digits.
        /// This method recursively reduces the number of digits if no match is found.
        /// </summary>
        private static void LookupWords(string dictionary, byte[] searchDigits, ref int noDigitsProcessed, ref List<DigitsWords> wordList, bool outputProgess = false)
        {
            if (outputProgess)
            {
                ClearCurrentConsoleLine();
                Console.Write("Searching for {0}\r", string.Join(",", searchDigits));
            }

            var regExpression = GetRegexp(searchDigits);
            var matches = regExpression.Matches(dictionary);
            if (matches.Count > 0)
            {
                noDigitsProcessed = searchDigits.Length; // store the number of digits already processed

                if (outputProgess)
                {
                    ClearCurrentConsoleLine();
                    Console.Write("Found {0}: ", string.Join(",", searchDigits));
                }

                var word = new DigitsWords(searchDigits);
                wordList.Add(word);

                // Parallel.ForEach did not seem to be faster
                foreach (Match match in matches)
                {
                    //Console.WriteLine("{0} found at position {1}", match.Groups[1], match.Index);
                    if (outputProgess) Console.Write("{0}  ", match.Groups[1].Value);
                    word.WordCandidates.Add(match.Groups[1].Value);
                }

                if (outputProgess) Console.Write("\n");
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
        /// Finds words in the dictionary that can be formed from the given digits.
        /// The digits are divided into chunks, and the LookupWords method is used to find matching words.
        /// </summary>
        public static List<DigitsWords> FindWords(string dictionary, byte[] digits, bool outputProgess = false)
        {
            if (outputProgess) Console.WriteLine("Converting into words: {0}", string.Join(",", digits));

            // divide into sections of x digits
            const int chunkSize = 8;

            // variables
            var wordList = new List<DigitsWords>();
            byte[] chunk;
            int processedNew = 0;
            int processedAlready = 0;
            int curIndex;
            int remainingDigits;

            while (true)
            {
                curIndex = processedAlready + processedNew; // what index are we at
                processedAlready += processedNew; // store how many digits we have processed so far
                remainingDigits = digits.Length - curIndex;
                if (remainingDigits <= 0) break; // we have exceeded the end

                if (chunkSize + curIndex <= digits.Length)
                {
                    // use chunk size
                    chunk = new byte[chunkSize];
                    Array.Copy(digits, curIndex, chunk, 0, chunkSize);
                }
                else
                {
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
        /// <summary>
        /// Parses a sentence into a byte array of digits.
        /// </summary>
        private static byte[] ParseDigits(string sentence)
        {
            var digits = new List<byte>();

            foreach (string word in sentence.Split(new string[] { "\r\n", "\n", " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                //Console.WriteLine(word);

                char lastCharacter = '.';
                foreach (char c in word)
                {
                    char charLetter = char.ToLower(c);
                    if (_vowels.Contains(charLetter))
                    {
                        lastCharacter = charLetter;
                    }
                    else
                    {
                        if (lastCharacter == charLetter) continue;

                        // consonant
                        byte value = 255;
                        switch (charLetter)
                        {
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

        /// <summary>
        /// Converts a list of DigitsWords to a string, by extracting the first word candidate from each DigitsWords object and joining them with a newline character.
        /// </summary>
        private static string WordListAsString(List<DigitsWords> wordList)
        {
            var sentenceList = from list in wordList
                               select list.WordCandidates.First()
                             ;
            return string.Join("\r\n", sentenceList);
        }

        /// <summary>
        /// Verifies that the word list correctly represents the expected digits by converting
        /// the words back to digits and comparing with the original.
        /// </summary>
        /// <param name="expectedDigits">The original digits that should be represented</param>
        /// <param name="wordList">The list of words to verify</param>
        private static void VerifyResults(byte[] expectedDigits, List<DigitsWords> wordList)
        {
            string digitsSentence = WordListAsString(wordList);
            byte[] foundDigits = ParseDigits(digitsSentence);
            if (!expectedDigits.SequenceEqual(foundDigits)) Console.WriteLine("Failed - Not identical");
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
        /// E.g. use dotnet run -- -a 314159265358979
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine("Usage: MemoryWords [options] [digits]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help                  Show help message and exit");
                Console.WriteLine("  -s, --string <text>         Parse the given text into digits using ParseDigits");
                Console.WriteLine("  -d, --dictionary <name>     Use the specified dictionary (wiktionary, ord10000, ord10k, norwegian, norwegian_large, nsf2012, nsf2023)");
                Console.WriteLine("  -a, --all                   Use all dictionaries (default)");
                Console.WriteLine("  -c, --csv                   Enable CSV output");
                Console.WriteLine("Digits: A string of digits to convert into words (default: pi digits)");
                return;
            }

            // Check for -s/--string flag first
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "-s" || args[i] == "--string") && i + 1 < args.Length)
                {
                    string input = args[i + 1];
                    byte[] parsedDigits = ParseDigits(input);
                    Console.WriteLine("Input: {0}", input);
                    Console.WriteLine("Parsed digits: {0}", string.Join(",", parsedDigits));
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
            bool useAllDictionaries = true;

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
                // string defaultDigits = "03918284226"; // småbåthavnforening
                string defaultDigits = "314159265358979";
                // string defaultDigits = "314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651";
                // string defaultDigits = "05721184084105791485091641940564691959501972917284624120958121584129557914181849214842841226184084612846419491230290748494107908502039150121418543492858012042158641202054157914203849850032319410790149284059511014718531484972";
                Console.WriteLine("Could not find any digits to use. Using default digits {0}\n", defaultDigits);
                digits = Digits(defaultDigits);
            }

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
                    string dictionary = File.ReadAllText(Path.Combine(DictPath, dictionaryName), _isoLatin1Encoding);
                    wordList = FindWords(dictionary, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                // read the nsf dictionary in 2012 format
                case "nsf2012.txt":
                    var nsf2012Words = new NSF2012(Path.Combine(Path.Combine(DictPath, "nsf2012"), dictionaryName));
                    wordList = FindWords(nsf2012Words.Nouns, digits, true);
                    if (outputCSV) WriteCSV(outputCSVDir, outFileName, digits, wordList);
                    VerifyResults(digits, wordList);
                    break;

                // read the nsf dictionary in 2023 format (as list of words)
                case "nsf2023.txt":
                    string dictionaryNSF = File.ReadAllText(Path.Combine(Path.Combine(DictPath, "nsf2023"), dictionaryName), _isoLatin1Encoding);
                    wordList = FindWords(dictionaryNSF, digits, true);
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