using System;

class Program
{
    static void Main()
    {
        var grid = CardanoGrid.Encrypt(filePath: "primer.docx", "masque.json");
        Console.WriteLine("");
        var biba = CardanoGrid.Decrypt(filePath: "primer.docx.bib", "masque.json");
    }
}