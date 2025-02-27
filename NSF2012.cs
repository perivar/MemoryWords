﻿namespace MemoryWords
{
	class NSF2012
	{
		// Substantiv - an abstract or concrete entity; a person, place, thing, idea or quality
		private string[] nouns;

		// Adjektiv - a modifier of a noun or pronoun (big, brave)
		private string[] adjectives;

		// Pronomen - a substitute for a noun or noun phrase (them, he)
		private string[] pronouns;

		// Verb - an action (walk), occurrence (happen), or state of being (be)
		private string[] verbs;

		// Adverb - a modifier of an adjective, verb, or other adverb (very, quite)
		private string[] adverbs;

		// Preposisjon - aids in syntactic contex (in, of)
		private string[] prepositions;

		// Konjunksjon - links words, phrases, or clauses (and, but)
		private string[] conjunctions;

		// Interjeksjon - greeting or exclamation (Hurrah, Alas)
		private string[] interjections;

		// Subjunksjon – innleder bisetninger
		private string[] subjunctions;

		// Artikkel - definiteness (the) or indefiniteness (a, an)
		private string[] articles;

		// Determinativ – bestemmer substantivet nærmere
		private string[] determinatives;

		#region Getters
		public string[] Nouns => nouns;
		public string[] Adjectives => adjectives;
		public string[] Pronouns => pronouns;
		public string[] Verbs => verbs;
		public string[] Adverbs => adverbs;
		public string[] Prepositions => prepositions;
		public string[] Conjunctions => conjunctions;
		public string[] Interjections => interjections;
		public string[] Subjunctions => subjunctions;
		public string[] Articles => articles;
		public string[] Determinatives => determinatives;

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
			foreach (var line in File.ReadLines(filePath))
			{
				if (string.IsNullOrEmpty(line))
					continue;
				var elements = line.Split([" "], StringSplitOptions.RemoveEmptyEntries);
				string word = elements.First().ToLower();
				string wordClass = elements.Last();
				switch (wordClass)
				{
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

			// store arrays directly
			nouns = mynouns.ToArray();
			adjectives = myadjectives.ToArray();
			pronouns = mypronouns.ToArray();
			verbs = myverbs.ToArray();
			adverbs = myadverbs.ToArray();
			prepositions = myprepositions.ToArray();
			conjunctions = myconjunctions.ToArray();
			interjections = myinterjections.ToArray();
			subjunctions = mysubjunctions.ToArray();
			articles = myarticles.ToArray();
			determinatives = mydeterminatives.ToArray();
		}
	}
}

