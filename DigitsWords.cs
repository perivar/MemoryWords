namespace MemoryWords
{
	public class DigitsWords
	{
		byte[] digits;

		readonly List<string> wordCandidates = new List<string>();

		public DigitsWords(byte[] digits)
		{
			this.digits = digits;
		}

		public byte[] Digits {
			get {
				return digits;
			}
		}

		public string DigitsAsString()
		{
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
}

