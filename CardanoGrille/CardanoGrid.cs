namespace CardanoGrid;

public class CardanoGrid
{
    private int Width { get; set; }
    private int Height { get; set; }

    private int[] Holes { get; set; }
    public CardanoGrid(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool Encrypt(string text)
    {
        return true;
    }
}