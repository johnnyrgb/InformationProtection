using System;

namespace BitPermutations;

class Programs
{
    static void Main(string[] args)
    {
        var knownPhrase = "Совершенно секретно";
        var plainText = "Привет, мир!";
        var encryptedText = BitPermutations.Encrypt(plainText, knownPhrase);
        var decryptedText = BitPermutations.Decrypt(encryptedText, knownPhrase);
    }
}

    