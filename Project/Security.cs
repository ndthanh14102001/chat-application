using System;
using System.Security.Cryptography;
using System.Text;

public class Security
{
    private int matrixLevel = 2;
    private string key = "secretKey";
    private string hillKey = "ABCN";


    private string CaeserEncrypt(string plainText, int shift)
    {
        string result = string.Empty;

        foreach (char ch in plainText)
        {
            if (char.IsLetter(ch))
            {
                char offset = char.IsUpper(ch) ? 'A' : 'a';
                result += (char)(((ch + shift - offset) % 26) + offset);
            }
            else
            {
                result += ch;
            }
        }

        return result;
    }

    private string CaeserDecrypt(string cipherText, int shift)
    {
        return CaeserEncrypt(cipherText, 26 - shift); // Decryption is just encryption with a negative shift
    }
    private string SubsEncrypt(string plainText, string key)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder encryptedText = new StringBuilder();

        for (int i = 0; i < plainText.Length; i++)
        {
            char character = plainText[i];

            if (char.IsLetter(character))
            {
                char upperChar = char.ToUpper(character);
                int index = alphabet.IndexOf(upperChar);
                if (index >= 0)
                {
                    char encryptedChar = char.IsUpper(character) ? key[index] : char.ToLower(key[index]);
                    encryptedText.Append(encryptedChar);
                }
            }
            else
            {
                encryptedText.Append(character);
            }
        }

        return encryptedText.ToString();
    }

    private string SubsDecrypt(string encryptedText, string key)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder decryptedText = new StringBuilder();

        for (int i = 0; i < encryptedText.Length; i++)
        {
            char character = encryptedText[i];

            if (char.IsLetter(character))
            {
                char upperChar = char.ToUpper(character);
                int index = key.IndexOf(upperChar);
                if (index >= 0)
                {
                    char decryptedChar = char.IsUpper(character) ? alphabet[index] : char.ToLower(alphabet[index]);
                    decryptedText.Append(decryptedChar);
                }
            }
            else
            {
                decryptedText.Append(character);
            }
        }

        return decryptedText.ToString();
    }
    private string HillEncrypt(string input, int[,] key)
    {
        int n = key.GetLength(0);
        int[] vector = new int[n];

        // Pad the input if its length is not a multiple of n
        int padding = n - (input.Length % n);
        if (padding != n)
            input += new string('X', padding);

        string encryptedText = "";

        for (int i = 0; i < input.Length; i += n)
        {
            // Create the vector from the current n characters of the input
            for (int j = 0; j < n; j++)
                vector[j] = input[i + j] - 'A';

            // Multiply the key matrix with the vector
            int[] result = new int[n];
            for (int j = 0; j < n; j++)
                for (int k = 0; k < n; k++)
                    result[j] += key[k, j] * vector[k];

            // Modulo 26 to keep the result in the range of the alphabet
            for (int j = 0; j < n; j++)
                result[j] = result[j] % 26;

            // Convert the result back to characters and append to the encrypted text
            for (int j = 0; j < n; j++)
                encryptedText += (char)(result[j] + 'A');
        }

        return encryptedText;
    }


    private int[,] StringToMatrix(string text)
    {
        int[,] result = new int[(int)Math.Ceiling(Decimal.Divide(text.Length, 2)), matrixLevel];
        int iMatrix = 0;
        int jMatrix = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (jMatrix == matrixLevel)
            {
                iMatrix = iMatrix + 1;
                jMatrix = 0;
            }
            result[iMatrix, jMatrix] = text[i] - 'A'; // Convert characters to numeric values
            jMatrix = jMatrix + 1;
        }
        return result;
    }
    private string HillDecrypt(string input, int[,] key)
    {
        int n = key.GetLength(0);
        int[] vector = new int[n];

        // Find the modular inverse of the determinant of the key matrix
        int det = MatrixDeterminant(key, n);
        int detInverse = ModuloInverse(det, 26);

        // Find the adjugate of the key matrix
        int[,] adjugate = Adjugate(key);

        // Multiply the adjugate with the modular inverse of the determinant
        int[,] inverseKey = ScalarMultiply(adjugate, detInverse, n);

        string decryptedText = "";

        for (int i = 0; i < input.Length; i += n)
        {
            // Create the vector from the current n characters of the input
            for (int j = 0; j < n; j++)
                vector[j] = input[i + j] - 'A';

            // Multiply the inverse key matrix with the vector
            int[] result = new int[n];
            for (int j = 0; j < n; j++)
                for (int k = 0; k < n; k++)
                    result[j] += inverseKey[k, j] * vector[k];

            // Modulo 26 to keep the result in the range of the alphabet
            for (int j = 0; j < n; j++)
                result[j] = (result[j] % 26 + 26) % 26;

            // Convert the result back to characters and append to the decrypted text
            for (int j = 0; j < n; j++)
                decryptedText += (char)(result[j] + 'A');
        }

        return decryptedText;
    }
    private int MatrixDeterminant(int[,] matrix, int n)
    {
        // Recursive function to find the determinant of a matrix
        if (n == 1)
            return matrix[0, 0];

        int det = 0;
        int sign = 1;

        for (int i = 0; i < n; i++)
        {
            int[,] submatrix = new int[n - 1, n - 1];

            for (int j = 1; j < n; j++)
                for (int k = 0; k < n; k++)
                    if (k < i)
                        submatrix[j - 1, k] = matrix[j, k];
                    else if (k > i)
                        submatrix[j - 1, k - 1] = matrix[j, k];

            det += sign * matrix[0, i] * MatrixDeterminant(submatrix, n - 1);
            sign = -sign;
        }

        return det;
    }

    private int ModuloInverse(int a, int m)
    {
        // Extended Euclidean Algorithm to find the modular inverse
        int m0 = m, t, q;
        int x0 = 0, x1 = 1;

        if (m == 1)
            return 0;

        while (a > 1)
        {
            q = a / m;
            t = m;

            m = a % m;
            a = t;
            t = x0;

            x0 = x1 - q * x0;
            x1 = t;
        }

        if (x1 < 0)
            x1 += m0;

        return x1;
    }

    static int[,] Adjugate(int[,] matrix)
    {
        int n = matrix.GetLength(0);
        int[,] adjugate = new int[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                int[,] submatrix = new int[n - 1, n - 1];

                for (int k = 0; k < n; k++)
                    for (int l = 0; l < n; l++)
                        if (k != i && l != j)
                            submatrix[k < i ? k : k - 1, l < j ? l : l - 1] = matrix[k, l];

                int sign = ((i + j) % 2 == 0) ? 1 : -1;
                adjugate[j, i] = sign * MatrixDeterminant(submatrix, n - 1);
            }

        return adjugate;
    }

    private int[,] ScalarMultiply(int[,] matrix, int scalar, int n)
    {
        int[,] result = new int[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = (matrix[i, j] * scalar) % 26;

        return result;
    }
    private string VigenereCipher(string input, string key, bool encipher)
    {
        for (int i = 0; i < key.Length; ++i)
            if (!char.IsLetter(key[i]))
                return null; // Error

        string output = string.Empty;
        int nonAlphaCharCount = 0;

        for (int i = 0; i < input.Length; ++i)
        {
            if (char.IsLetter(input[i]))
            {
                bool cIsUpper = char.IsUpper(input[i]);
                char offset = cIsUpper ? 'A' : 'a';
                int keyIndex = (i - nonAlphaCharCount) % key.Length;
                int k = (cIsUpper ? char.ToUpper(key[keyIndex]) : char.ToLower(key[keyIndex])) - offset;
                k = encipher ? k : -k;
                char ch = (char)((Mod(((input[i] + k) - offset), 26)) + offset);
                output += ch;
            }
            else
            {
                output += input[i];
                ++nonAlphaCharCount;
            }
        }

        return output;
    }
    static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }

    private string VigenereEncipher(string input, string key)
    {
        return VigenereCipher(input, key, true);
    }

    private string VigenereDecipher(string input, string key)
    {
        return VigenereCipher(input, key, false);
    }

    private string EncryptKey(string key)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        string alphabetCaeserCipher = CaeserEncrypt(alphabet, key.Length);
        string hillEncrypt = HillEncrypt(alphabetCaeserCipher, StringToMatrix(hillKey));
        return hillEncrypt;
    }
    public string Encrypt(string plainText)
    {
        string encryptedKey = EncryptKey(key);
        string subsCipher = SubsEncrypt(plainText, encryptedKey);
        string vigenereCipher = VigenereEncipher(subsCipher, key);
        return vigenereCipher;
    }
    public string Decrypt(string cipherText)
    {
        string vigenereDecrypted = VigenereDecipher(cipherText, key);
        string subsDecrypted = SubsDecrypt(vigenereDecrypted, EncryptKey(key));
        return subsDecrypted;
    }
}
