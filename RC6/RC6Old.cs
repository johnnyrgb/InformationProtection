using System.Security.Cryptography;
using System.Text.Json;

namespace RC6;

public class RC6Old
{
    private const int RoundsCount = 20;
    private static int WordLength { get; } = 32;

    #region Шифрование
    private struct EncryptionResult
    {
        public byte[] EncryptedBytes;
        public byte[] Key;
    }

    public static void Encrypt(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var blocks = SplitBytesIntoBlocks(fileBytes, isEncryption: true);
        var result = RunEncryption(blocks);

        File.WriteAllBytes(string.Concat(filePath, ".rc6"), result.EncryptedBytes);
        File.WriteAllBytes(string.Concat(filePath, ".key"), result.Key);
    }

    private static List<byte[]> SplitBytesIntoBlocks(byte[] fileBytes, bool isEncryption)
    {
        var blocks = new List<byte[]>();
        var blockSize = 16;
        var blocksCount = (fileBytes.Length + blockSize - 1) / blockSize;

        for (var i = 0; i < blocksCount; i++)
        {
            var start = i * blockSize;
            var length = Math.Min(blockSize, fileBytes.Length - start);
            var block = new byte[blockSize];

            Array.Copy(fileBytes, start, block, 0, length);

            if (i == blocksCount - 1 && isEncryption)
            {
                if (length < blockSize)
                {
                    block = GetPaddedBlock(block, length, blockSize);
                    blocks.Add(block);
                }
                    
                else if (length == blockSize)
                {
                    blocks.Add(block);
                    block = GetDummyBlock(blockSize);
                    blocks.Add(block);
                }
            }
            else
            {
                blocks.Add(block);
            }
            
        }

        return blocks;
    }

    private static byte[] GetPaddedBlock(byte[] buffer, int bytesRead, int bufferSize)
    {
        var paddingValue = (byte)(bufferSize - bytesRead);
        for (var i = bytesRead; i < bufferSize; i++) buffer[i] = paddingValue;

        return buffer;
    }

    private static byte[] GetDummyBlock(int bufferSize)
    {
        var buffer = new byte[bufferSize];
        for (var i = 0; i < bufferSize; i++) buffer[i] = 16;
        return buffer;
    }

    private static EncryptionResult RunEncryption(List<byte[]> blocks)
    {
        var key = GenerateKey();
        var subkeys = GenerateSubkeys(key);
        var encryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var registers = SplitBlockIntoRegisters(block);
            ModifyRegistersBeforeEncryption(registers, subkeys);
            EncryptRegisters(registers, subkeys);
            ModifyRegistersAfterEncryption(registers, subkeys);
            encryptedBlocks.Add(CombineRegistersIntoBlock(registers));
        }

        return new EncryptionResult
        {
            EncryptedBytes = encryptedBlocks.SelectMany(b => b).ToArray(),
            Key = key
        };
    }

    private static Dictionary<string, uint> SplitBlockIntoRegisters(byte[] block)
    {
        return new Dictionary<string, uint>
        {
            { "A", BitConverter.ToUInt32(block, 0) },
            { "B", BitConverter.ToUInt32(block, 4) },
            { "C", BitConverter.ToUInt32(block, 8) },
            { "D", BitConverter.ToUInt32(block, 12) }
        };
    }

    private static byte[] CombineRegistersIntoBlock(Dictionary<string, uint> registers)
    {
        var block = new byte[16];
        Array.Copy(BitConverter.GetBytes(registers["A"]), 0, block, 0, 4);
        Array.Copy(BitConverter.GetBytes(registers["B"]), 0, block, 4, 4);
        Array.Copy(BitConverter.GetBytes(registers["C"]), 0, block, 8, 4);
        Array.Copy(BitConverter.GetBytes(registers["D"]), 0, block, 12, 4);
        return block;
    }

    private static byte[] GenerateKey()
    {
        var key = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
        }

        return key;
    }

    private static List<uint> GenerateSubkeys(byte[] key)
    {
        var splitKey = new List<uint>(4);
        for (var i = 0; i < key.Length; i += 4)
        {
            var subkey = BitConverter.ToUInt32(key, i);
            splitKey.Add(subkey);
        }

        var P32 = 0xB7E15163;
        var Q32 = 0x9E3779B9;

        var subkeysCount = 2 * (RoundsCount + 2);
        var subkeys = new List<uint>(subkeysCount);
        subkeys.Add(P32);
        for (var i = 1; i < subkeysCount; i++) subkeys.Add(P32);

        int subkeyIndex = 0, splitKeyIndex = 0;
        uint bufferA = 0, bufferB = 0;

        for (var i = 0; i < 3 * Math.Max(subkeysCount, 4); i++)
        {
            subkeys[subkeyIndex] = bufferA = RotateLeft(subkeys[subkeyIndex] + bufferA + bufferB, 3);
            splitKey[splitKeyIndex] =
                bufferB = RotateLeft(splitKey[splitKeyIndex] + bufferA + bufferB,
                    (int)(bufferA + bufferB)); 

            subkeyIndex = (subkeyIndex + 1) % subkeysCount;
            splitKeyIndex = (splitKeyIndex + 1) % 4;
        }

        return subkeys;
    }

    private static void ModifyRegistersBeforeEncryption(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        registers["B"] = registers["B"] + subkeys[0];
        registers["D"] = registers["D"] + subkeys[1];
    }

    private static void ModifyRegistersAfterEncryption(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        registers["A"] = registers["A"] + subkeys[2 * RoundsCount + 2];
        registers["C"] = registers["C"] + subkeys[2 * RoundsCount + 3];
    }

    private static uint RotateRight(uint value, int shift)
    {
        return (value >> shift) | (value << (WordLength - shift));
    }

    //Сдвиг влево без потери
    private static uint RotateLeft(uint value, int shift)
    {
        return (value << shift) | (value >> (WordLength - shift));
    }

    private static void EncryptRegisters(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        for (var round = 0; round < RoundsCount; round++)
        {
            var t = RotateLeft(registers["B"] * (2 * registers["B"] + 1), (int)Math.Log(WordLength, 2));
            var u = RotateLeft(registers["D"] * (2 * registers["D"] + 1), (int)Math.Log(WordLength, 2));
            registers["A"] = RotateLeft(registers["A"] ^ t, (int)u) + subkeys[round * 2]; // Плюс в кружке XOR
            registers["C"] = RotateLeft(registers["C"] ^ u, (int)t) + subkeys[round * 2 + 1]; // Плюс в кружке XOR

            var temp = registers["A"];
            registers["A"] = registers["B"];
            registers["B"] = registers["C"];
            registers["C"] = registers["D"];
            registers["D"] = temp;
        }
    }
    #endregion

    #region Decryption

    public static void Decrypt(string filePath, string keysPath)
    {
        var encryptionResult = new EncryptionResult()
        {
            EncryptedBytes = File.ReadAllBytes(filePath),
            Key = File.ReadAllBytes(keysPath)
        };
        var blocks = SplitBytesIntoBlocks(encryptionResult.EncryptedBytes, isEncryption: false);
        var result = RunDecryption(blocks, encryptionResult.Key);
        result = RemovePadding(result);
        File.WriteAllBytes(String.Concat(filePath.Substring(0, filePath.Length - 4), "bib"), result);
    }

    private static byte[] RunDecryption(List<byte[]> blocks, byte[] key)
    {
        var decryptedBlocks = new List<byte[]>();
        var subkeys = GenerateSubkeys(key);
        foreach (var block in blocks)
        {
            var registers = SplitBlockIntoRegisters(block);
            ModifyRegistersBeforeDecryption(registers, subkeys);
            DecryptRegisters(registers, subkeys);
            ModifyRegistersAfterDecryption(registers, subkeys);
            decryptedBlocks.Add(CombineRegistersIntoBlock(registers));
        }
        return decryptedBlocks.SelectMany(b => b).ToArray();
    }
    private static void ModifyRegistersBeforeDecryption(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        registers["A"] = registers["A"] - subkeys[2 * RoundsCount + 2];
        registers["C"] = registers["C"] - subkeys[2 * RoundsCount + 3];
    }

    private static void DecryptRegisters(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        for (var round = RoundsCount - 1; round >= 0; round--)
        {
            var temp = registers["D"];
            registers["D"] = registers["C"];
            registers["C"] = registers["B"];
            registers["B"] = registers["A"];
            registers["A"] = temp;
            var u = RotateLeft(registers["D"] * (2 * registers["D"] + 1), (int)Math.Log(WordLength, 2));
            var t = RotateLeft(registers["B"] * (2 * registers["B"] + 1), (int)Math.Log(WordLength, 2));
            registers["C"] = RotateRight(registers["C"] - subkeys[round * 2 + 1], (int)t) ^ u;
            registers["A"] = RotateRight(registers["A"] - subkeys[round * 2], (int)u) ^ t;
        }
    }

    private static void ModifyRegistersAfterDecryption(Dictionary<string, uint> registers, List<uint> subkeys)
    {
        registers["B"] = registers["B"] - subkeys[0];
        registers["D"] = registers["D"] - subkeys[1];
    }

    private static byte[] RemovePadding(byte[] decryptedData)
    {
        var paddingLength = decryptedData.Last();
;       return decryptedData[..^paddingLength];
    }
    #endregion
}