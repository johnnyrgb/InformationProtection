class Program
{
    static void Main()
    {
        var grid = CardanoGrid.Encrypt(filePath: "iom-16-rus.jpg", "masque");
        Console.WriteLine("");
        var biba = CardanoGrid.Decrypt(filePath: "iom-16-rus.jpg.bib", "masque");
    }
}