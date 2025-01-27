using System;

class Program
{
    static void Main()
    {
        var grid = CardanoGrid.Encrypt(filePath: "primer.docx", "masque.json");
        zConsole.WriteLine("");
        var biba = CardanoGrid.Decrypt(filePath: "primer.docx.bib", "masque.json");
    }
}