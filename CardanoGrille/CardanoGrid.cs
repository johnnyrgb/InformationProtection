using System.Diagnostics;
using System.Text;
using System.Text.Json;

public static class CardanoGrid
{  
public static bool Encrypt(string filePath, string masquePath)
    {
        var fileBytes = File.ReadAllBytes(filePath).ToList();

        var N = 0;
        while (N * N < fileBytes.Count)
            N++;

        N = N * 2;
        byte[] grid = new byte[N * N];
        byte[] masque = new byte[N * N];

        var random = new Random();
        for (var i = 0; i < fileBytes.Count; i++)
        {
            int quadrant, x, y;
            do
            {
                quadrant = random.Next(0, 4);
                x = random.Next(0, N / 2);
                y = random.Next(0, N / 2);
            } while (masque[x * N + y] == 1 ||
                     masque[y * N + (N - 1 - x)] == 1 ||
                     masque[(N - 1 - x) * N + (N - 1 - y)] == 1 ||
                     masque[(N - 1 - y) * N + x] == 1 ||
                     masque[x * N + y] == 255 ||
                     masque[y * N + (N - 1 - x)] == 255 ||
                     masque[(N - 1 - x) * N + (N - 1 - y)] == 255 ||
                     masque[(N - 1 - y) * N + x] == 255);

            switch (quadrant)
            {
                case 0:
                    masque[x * N + y] = 1;
                    masque[y * N + (N - 1 - x)] = 255;
                    masque[(N - 1 - x) * N + (N - 1 - y)] = 255;
                    masque[(N - 1 - y) * N + x] = 255;
                    break;
                case 1:
                    masque[x * N + y] = 255;
                    masque[y * N + (N - 1 - x)] = 1;
                    masque[(N - 1 - x) * N + (N - 1 - y)] = 255;
                    masque[(N - 1 - y) * N + x] = 255;
                    break;
                case 2:
                    masque[x * N + y] = 255;
                    masque[y * N + (N - 1 - x)] = 255;
                    masque[(N - 1 - x) * N + (N - 1 - y)] = 1;
                    masque[(N - 1 - y) * N + x] = 255;
                    break;
                case 3:
                    masque[x * N + y] = 255;
                    masque[y * N + (N - 1 - x)] = 255;
                    masque[(N - 1 - x) * N + (N - 1 - y)] = 255;
                    masque[(N - 1 - y) * N + x] = 1;
                    break;
            }
        }
        for (var row = 0; row < N; row++)
        {
            for (var cell = 0; cell < N; cell++)
            {
                if (masque[row * N + cell] == 255 || masque[row * N + cell] == 0)
                {
                    grid[row * N + cell] = GetRandomByte();
                }
                else
                {
                    grid[row * N + cell] = fileBytes.First();
                    fileBytes.RemoveAt(0);
                }
            }
        }

        File.WriteAllBytes(masquePath, masque);
        File.WriteAllBytes(string.Concat(filePath, ".bib"), grid);
        return true;
    }

    public static bool Decrypt(string filePath, string masquePath)
    {
        var masque = File.ReadAllBytes(masquePath);
        var encryptedText = File.ReadAllBytes(filePath);
        List<byte> fileBytes = new List<byte>();
        int N = (int)Math.Sqrt(masque.Length);
        for (var i = 0; i < N; i++)
        {
            for (var j = 0; j < N; j++)
            {
                if (masque[i * N + j] == 1)
                {
                    fileBytes.Add(encryptedText![i * N + j]);
                }
            }
        }
        File.WriteAllBytes(filePath.Substring(0, filePath.Length - 4), fileBytes.ToArray());

        return true;
    }

    private static byte GetRandomByte()
    {
        var random = new Random();
        return (byte)random.Next(0, 256);
    }

}