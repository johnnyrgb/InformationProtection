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
        List<List<byte?>> grid = new List<List<byte?>>();
        List<List<int>> masque = new List<List<int>>();
        for (var i = 0; i < N; i++)
        {
            grid.Add(new List<byte?>());
            for (var j = 0; j < N; j++)
                grid[i].Add(null);

            masque.Add(new List<int>());
            for (var j = 0; j < N; j++)
                masque[i].Add(0);
        }

        // 0 - нет дырки
        // 1 - дырка
        // -1 - занято
        var random = new Random();
        for (var i = 0; i < fileBytes.Count; i++)
        {
            int quadrant, x, y;
            do
            {
                quadrant = random.Next(0, 4);
                x = random.Next(0, N / 2);
                y = random.Next(0, N / 2);
            } while (masque[x][y] == 1 ||
                     masque[y][N - 1 - x] == 1 ||
                     masque[N - 1 - x][N - 1 - y] == 1 ||
                     masque[N - 1 - y][x] == 1 ||
                     masque[x][y] == -1 ||
                     masque[y][N - 1 - x] == -1 ||
                     masque[N - 1 - x][N - 1 - y] == -1 ||
                     masque[N - 1 - y][x] == -1);

            switch (quadrant)
            {
                case 0:
                    masque[x][y] = 1;
                    masque[y][N - 1 - x] = -1;
                    masque[N - 1 - x][N - 1 - y] = -1;
                    masque[N - 1 - y][x] = -1;
                    break;
                case 1:
                    masque[x][y] = -1;
                    masque[y][N - 1 - x] = 1;
                    masque[N - 1 - x][N - 1 - y] = -1;
                    masque[N - 1 - y][x] = -1;
                    break;
                case 2:
                    masque[x][y] = -1;
                    masque[y][N - 1 - x] = -1;
                    masque[N - 1 - x][N - 1 - y] = 1;
                    masque[N - 1 - y][x] = -1;
                    break;
                case 3:
                    masque[x][y] = -1;
                    masque[y][N - 1 - x] = -1;
                    masque[N - 1 - x][N - 1 - y] = -1;
                    masque[N - 1 - y][x] = 1;
                    break;
            }
        }
        for (var row = 0; row < N; row++)
        {
            for (var cell = 0; cell < N; cell++)
            {
                if (masque[row][cell] == -1 || masque[row][cell] == 0)
                {
                    grid[row][cell] = GetRandomByte();

                }
                else
                {
                    grid[row][cell] = fileBytes.First();
                    fileBytes.RemoveAt(0);
                }
            }
            Console.WriteLine();
        }

        File.WriteAllBytes(masquePath, JsonSerializer.SerializeToUtf8Bytes(masque));

        File.WriteAllBytes(string.Concat(filePath, ".bib"), JsonSerializer.SerializeToUtf8Bytes(grid));
        return true;
    }

    public static bool Decrypt(string filePath, string masquePath)
    {
        var masque = JsonSerializer.Deserialize<List<List<int>>>(File.ReadAllBytes(masquePath));
        var encryptedText = JsonSerializer.Deserialize<List<List<byte>>>(File.ReadAllBytes(filePath));
        List<byte> fileBytes = new List<byte>();
        for (var i = 0; i < masque.Count; i++)
        {
            for (var j = 0; j < masque[i].Count; j++)
            {
                if (masque[i][j] == 1)
                {
                    fileBytes.Add(encryptedText![i][j]);
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
    public static void SaveFileBytes(string filePath, List<byte> fileBytes)
    {
        var newFilePath = filePath.Substring(0, filePath.Length - 4);
        File.WriteAllBytes(newFilePath, fileBytes.ToArray());
    }


}