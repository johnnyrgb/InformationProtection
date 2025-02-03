using System.Text;

namespace BitPermutations;

public static class BitPermutations
{
    public static string Encrypt(string plainText, string knownPhrase)
    {
        // Случайчная перестановка битов (индекс - место)
        int[] permutation = { 3, 1, 4, 0, 7, 6, 2, 5 };
        // Конкатенация известной фразы и текста и превращение их байты
        var bytes = Encoding.UTF8.GetBytes(string.Concat(knownPhrase, plainText));
        // Создание массива байтов для результата
        var resultBytes = new byte[bytes.Length];

        // Перестановка битов для каждого байта
        for (int i = 0; i < bytes.Length; i++)
        {
            resultBytes[i] = PermuteBits(bytes[i], permutation);
        }

        // Преобразование результат
        return Convert.ToHexString(resultBytes);
    }

    public static string Decrypt(string encryptedText, string knownPhrase)
    {
        // Известные байты из известной фразы
        var knownBytes = Encoding.UTF8.GetBytes(knownPhrase);
        // Зашифрованные байты
        var encryptedBytes = Convert.FromHexString(encryptedText);

        if (encryptedBytes.Length < knownBytes.Length)
            throw new ArgumentException();

        // Массив шифрованных известных байтов
        var encryptedKnownBytes = new byte[knownBytes.Length];
        // Заполняем массив зашифрованными байтами известной фразы
        Array.Copy(encryptedBytes, encryptedKnownBytes, knownBytes.Length);

        // Перестановочки
        int[] permutation = DeterminePermutation(knownBytes, encryptedKnownBytes);

        // Переставляем биты в каждом байте
        byte[] resultBytes = new byte[encryptedBytes.Length];
        for (int i = 0; i < encryptedBytes.Length; i++)
            resultBytes[i] = PermuteBits(encryptedBytes[i], permutation);

        string resultText = Encoding.UTF8.GetString(resultBytes);
        return resultText.Substring(knownPhrase.Length);
    }

    private static byte PermuteBits(byte oneByte, int[] permutation)
    {
        byte result = 0;

        // 
        for (int i = 0; i < permutation.Length; i++)
        {
            // Делаем бит младшим битом и определяем его значение
            int bit = (oneByte >> i) & 1;  // 00000001 * 00000001 = 1 или 00000000 * 00000001 = 0
            // Сдвигаем бит на новую позицию и аккумулируем результат с помощью ИЛИ
            result |= (byte)(bit << permutation[i]);
        }
        return result;
    }

    /// <summary>
    /// Перебираем биты в байте. Для каждого бита, исходя из известной подстроки, находим место его перестановки. После каждой итерации
    /// для каждого пройденного бита должно остаться только одно место перестановки. Если мест будет больше, то исключение.
    /// </summary>
    /// <param name="knownBytes"></param>
    /// <param name="encryptedKnownBytes"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static int[] DeterminePermutation(byte[] knownBytes, byte[] encryptedKnownBytes)
    {

        if (knownBytes.Length != encryptedKnownBytes.Length)
            throw new ArgumentException("Длины известного текста и зашифрованного не совпадают.");

        // Массив для хранения правил перестановки
        int[] permutation = new int[8];
        List<int>[] possiblePositions = new List<int>[8];

        // Инициализация списков возможных позиций для каждого бита
        for (int i = 0; i < 8; i++)
        {
            possiblePositions[i] = Enumerable.Range(0, 8).ToList();
        }

        // Для каждого бита в зашифрованном байте
        for (int encryptedBitIdx = 0; encryptedBitIdx < 8; encryptedBitIdx++)
        {
            // Проверяем все байты известной подстроки
            for (int byteIdx = 0; byteIdx < knownBytes.Length; byteIdx++)
            {
                // Получаем известный и зашифрованный байты
                byte knownByte = knownBytes[byteIdx];
                byte encryptedByte = encryptedKnownBytes[byteIdx];

                // Определяем ожидаемое значение бита в зашифрованном байте
                bool expectedBit = (encryptedByte & (1 << encryptedBitIdx)) != 0;

                // Фильтруем возможные исходные позиции для бита с текущим значением
                // Удаляем все, где true
                // Внутри узнаем значение бита, а потом сравнимваем его с expectedBit. Если false, то оставляем.
                possiblePositions[encryptedBitIdx].RemoveAll(srcBitIdx =>
                    ((knownByte & (1 << srcBitIdx)) != 0) != expectedBit
                );
            }

            // Проверяем однозначность
            if (possiblePositions[encryptedBitIdx].Count != 1)
                throw new InvalidOperationException();

            permutation[encryptedBitIdx] = possiblePositions[encryptedBitIdx][0];
        }

        return permutation;
    }
}
