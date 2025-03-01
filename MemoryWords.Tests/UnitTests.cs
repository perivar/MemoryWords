﻿namespace MemoryWords.Tests;

public class NorwegianPhoneticsTests
{
    [Fact]
    public void ParseDigits_SjCombination_ShouldMapToDigit6()
    {
        // Arrange
        string input = "sjø";
        byte[] expected = new byte[] { 6 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_SkjCombination_ShouldMapToDigit6()
    {
        // Arrange
        string input = "skje";
        byte[] expected = new byte[] { 6 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_KjCombination_ShouldMapToDigit6And1()
    {
        // Arrange
        string input = "kjede";
        byte[] expected = new byte[] { 6, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_TjCombination_ShouldMapToDigit6()
    {
        // Arrange
        string input = "tjue";
        byte[] expected = new byte[] { 6 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_KjFollowedByT_ShouldMapToDigit6And1()
    {
        // Arrange
        string input = "kjøtt";
        byte[] expected = new byte[] { 6, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_SkjWithAdditionalConsonants_ShouldMapToDigit6And4And1()
    {
        // Arrange
        string input = "skjorte";
        byte[] expected = new byte[] { 6, 4, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_TjWithAdditionalConsonants_ShouldMapToDigit6And4And2()
    {
        // Arrange
        string input = "tjern";
        byte[] expected = new byte[] { 6, 4, 2 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_ConsecutiveConsonants_ShouldMapToSingleDigit()
    {
        // Arrange
        string input = "kjøtt";
        byte[] expected = new byte[] { 6, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_ConsecutiveAndSeparatedConsonants_ShouldHandleBothCases()
    {
        // Arrange
        string input = "kjøttet";  // 'tt' combines, but final 't' after 'e' stays separate
        byte[] expected = new byte[] { 6, 1, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_ConsonantsWithVowelsBetween_ShouldMapToMultipleDigits()
    {
        // Arrange
        string input = "toutet";
        byte[] expected = new byte[] { 1, 1, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_SkjemaVelde_ShouldMapCorrectly()
    {
        // Arrange
        string input = "skjemavelde";  // skj-m-v-l-d
        byte[] expected = new byte[] { 6, 3, 8, 5, 1 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_KjokkenHandkle_ShouldMapCorrectly()
    {
        // Arrange
        string input = "kjøkkenhåndkle";  // kj-kk-n-h-n-d-k-l
        byte[] expected = new byte[] { 6, 7, 2, 2, 1, 7, 5 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigits_SpacesBetweenWords_ShouldMapConsonantsAsSeparate()
    {
        // Arrange
        string input = "skjerm mus";  // space acts like a vowel, allowing 'm' and 'm' to be counted separately
        byte[] expected = new byte[] { 6, 4, 3, 3, 0 };

        // Act
        byte[] result = Program.ParseDigits(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void VerifyResults_ShouldShowDifferencesWhenSequencesDontMatch()
    {
        // Arrange
        var wordList = new List<DigitsWords> { new DigitsWords(new byte[] { 6, 7, 2, 1 }) }; // kjøkkenet
        wordList[0].WordCandidates.Add("kjøkkenet");
        byte[] expectedDigits = new byte[] { 6, 7, 2, 2, 1 }; // kjøkkenhånd
        var oldOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            bool result = Program.VerifyResults(expectedDigits, wordList);

            // Assert
            Assert.False(result);
            string output = stringWriter.ToString();
            Assert.Contains("Position:   0   1   2   3   4", output);
            Assert.Contains("Expected:   6   7   2   2   1", output);
            Assert.Contains("Found   :   6   7   2   1", output);
            Assert.Contains("Diff    :               ^   ^", output);
            Assert.Contains("Word    : kjøkkenet", output);
        }
        finally
        {
            Console.SetOut(oldOut);
        }
    }

    [Fact]
    public void VerifyResults_ShouldReturnTrueWhenSequencesMatch()
    {
        // Arrange
        var wordList = new List<DigitsWords> { new DigitsWords(new byte[] { 6, 4, 3, 3, 0 }) };
        wordList[0].WordCandidates.Add("skjerm mus");
        byte[] expectedDigits = new byte[] { 6, 4, 3, 3, 0 };

        // Act
        bool result = Program.VerifyResults(expectedDigits, wordList);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToMnemonic_SumMusStor_ShouldMapCorrectly()
    {
        // Arrange
        string input = "sum mus stor";
        string expected = "s(0) [u] m(3) [space] m(3) [u] s(0) [space] s(0) t(1) [o] r(4)";

        // Act
        string result = Program.ToMnemonic(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToMnemonic_KjokkenHandkle_ShouldMapCorrectly()
    {
        // Arrange
        string input = "kjøkkenhåndkle";
        string expected = "kj(6) [ø] kk(7) [e] n(2) [hå] n(2) d(1) k(7) l(5) [e]";

        // Act
        string result = Program.ToMnemonic(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDigitsAndToMnemonic_ComplexSentence_ShouldMapCorrectly()
    {
        // Arrange
        string input = "motorhotell penkjole milf byggeboomen omveier";
        byte[] expectedDigits = new byte[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5, 8, 9, 7, 9, 3, 2, 3, 8, 4 };
        string expectedMnemonic = "m(3) [o] t(1) [o] r(4) [ho] t(1) [e] ll(5) [space] p(9) [e] n(2) kj(6) [o] l(5) [e] [space] m(3) [i] l(5) f(8) [space] b(9) [y] gg(7) [e] b(9) [oo] m(3) [e] n(2) [space] [o] m(3) v(8) [eie] r(4)";

        // Act
        byte[] digits = Program.ParseDigits(input);
        string mnemonic = Program.ToMnemonic(input);

        // Assert
        Assert.Equal(expectedDigits, digits);
        Assert.Equal(expectedMnemonic, mnemonic);
    }
}
