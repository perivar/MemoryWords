namespace MemoryWords
{
	class NSF2012
	{
		// Substantiv - an abstract or concrete entity; a person, place, thing, idea or quality
		string nouns;

		// Adjektiv - a modifier of a noun or pronoun (big, brave)
		string adjectives;

		// Pronomen - a substitute for a noun or noun phrase (them, he)
		string pronouns;

		string verbs;
		// Verb - an action (walk), occurrence (happen), or state of being (be)

		string adverbs;
		// Adverb - a modifier of an adjective, verb, or other adverb (very, quite)

		// Preposisjon - aids in syntactic contex (in, of)
		string prepositions;

		// Konjunksjon - links words, phrases, or clauses (and, but)
		string conjunctions;

		// Interjeksjon - greeting or exclamation (Hurrah, Alas)
		string interjections;

		// Subjunksjon – innleder bisetninger
		string subjunctions;

		// Artikkel - definiteness (the) or indefiniteness (a, an)
		string articles;

		// Determinativ – bestemmer substantivet nærmere
		string determinatives;

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
		
		public NSF2012(string filePath)
		{
			// initialise temporary HashSets
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
			
			// read in the dictionary file
			foreach (var line in File.ReadLines(filePath)) {
				if (string.IsNullOrEmpty(line))
					continue;
				var elements = line.Split(new String[] {
				                          	" "
				                          }, StringSplitOptions.RemoveEmptyEntries);
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

