using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class PasswordHashHandler
{
    private static int _iterationCount = 10000;
    private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
{
    buffer[offset + 0] = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)(value);
}

private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
{
    return ((uint)buffer[offset + 0] << 24)
         | ((uint)buffer[offset + 1] << 16)
         | ((uint)buffer[offset + 2] << 8)
         | buffer[offset + 3];
}


    public static string HashPassword(string password)
    {
        int saltSize = 128/8;
        byte[] salt = new byte[saltSize];
        _rng.GetBytes(salt);
        var subKey = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: _iterationCount,
            numBytesRequested: 256 / 8);
        var outputBytes = new byte[13 + salt.Length + subKey.Length];
        outputBytes[0] = 0x01;
        WriteNetworkByteOrder(outputBytes, 1, (uint)KeyDerivationPrf.HMACSHA256);
        WriteNetworkByteOrder(outputBytes, 5, (uint)_iterationCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)salt.Length);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subKey, 0, outputBytes, 13 + salt.Length, subKey.Length);

        return Convert.ToBase64String(outputBytes);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashedPassword = Convert.FromBase64String(hash);
            var KeyDerivationPrf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            var iterationCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);
            if (saltLength < 128 / 8)
                return false;
            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);
            var subKeyLength = hashedPassword.Length - 13 - salt.Length;
            byte[] expectedSubKey = new byte[subKeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubKey, 0, expectedSubKey.Length);
            var actualSubKey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf, iterationCount, subKeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
        }
        catch
        {
            return false;
        }
    }
}