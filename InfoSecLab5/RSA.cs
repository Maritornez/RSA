using System.Diagnostics;
using System.Numerics; //BigInteger 
using System.Text;
using System.Windows;

namespace InfoSecLab5
{
    /// <summary>
    /// Класс, реализующий шифрование и расшифрование по методу RSA
    /// </summary>
    /// <param name="mainWindow"></param>
    internal class RSA(MainWindow mainWindow)
    {
        private BigInteger public_key = 0;  // e
        private BigInteger private_key = 0; // d
        private BigInteger n = 0; // модуль системы
        private readonly MainWindow mainWindow = mainWindow;

        public void SetKeys()
        {
            if (!TryParseToBigInteger(mainWindow.pTextBox.Text, out BigInteger p) || p <= 0 ||    // p - большое простое число
                !TryParseToBigInteger(mainWindow.qTextBox.Text, out BigInteger q) || q <= 0 ||    // q - большое простое число
                !TryParseToBigInteger(mainWindow.dTextBox.Text, out BigInteger d) || d <= 0)      // d - большое простое число, взнаимно простое с fi
            {
                MessageBox.Show("Введены некорректные данные");
                return;
            }

            n = p * q;  // модуль системы
            Trace.WriteLine("\n" + "Nums in n = " + n.ToString().Length + "; n = " + n.ToString() + "\n");
            BigInteger fi = (p - 1) * (q - 1); // функция Эйлера
            if (fi < d)
            {
                MessageBox.Show("d - слишком большое");
                return;
            }
            private_key = d;
            Trace.WriteLine("p = " + p + "\n" + "q = " + q + "\n" + "d = " + d + "\n" + "n = " + n + "\n" + "fi = " + fi);

            // Используя расширенный алгоритм Евклида (см. раздел 2), вычисляется большое целое число e, отвечающее условию (e·d) mod fi = 1,  1 < e < fi.
            // Метод Евклида находит множество пар (e, y), каждая из которых является решением уравнения:  e · d + fi · y = 1 в целых числах.
            ExEuсlidAlg(fi, d, out BigInteger e);
            Trace.WriteLine("e = " + e);
            public_key = e;
        }

        public void Encrypt()
        {
            // Число байтов, считаваемых за один блок, должно быть меньше или равно числу байтов в n
            int symbolsNumInPlainTextBlock = n.ToString().Length;
            int byteNumInN = n.ToByteArray().Length;
            Trace.WriteLine("byteNumInN = " + byteNumInN + "\n");
            int k = byteNumInN - 1; // размер блока в байтах

            byte[] textBytes = Encoding.UTF8.GetBytes(mainWindow.inputTextBox.Text);

            // Шифрование по алгоритму RSA
            // По блокам длинной k байт: считывание байтов, преобразование байтов в BigInteger, шифрование, преобразование в текст, запись текста в cryptedTextBox
            int offset = 0;
            while (offset < textBytes.Length)
            {
                int blockLength = k < textBytes.Length - offset ? k : textBytes.Length - offset; // = Math.Min(k, textBytes.Length - offset); 

                // Считывание блока длинной k байт в массив байтов
                byte[] block = new byte[blockLength];
                Array.Copy(textBytes, offset, block, 0, blockLength);
                Trace.WriteLine("Nums in block = " + block.Length + "; block = " + Encoding.UTF8.GetString(block));

                // Шифрование
                BigInteger blockAsBigInteger = new(block, true);
                BigInteger cryptedBlockAsBigInteger = BigInteger.ModPow(blockAsBigInteger, public_key, n);
                Trace.WriteLine("Nums in cryptedBlockAsBigInteger = " + cryptedBlockAsBigInteger.ToString("D" + symbolsNumInPlainTextBlock).Length
                                + "; cryptedBlockAsBigInteger = " + cryptedBlockAsBigInteger.ToString("D" + symbolsNumInPlainTextBlock));

                // Запись в cryptedTextBox
                mainWindow.cryptedTextBox.Text += cryptedBlockAsBigInteger.ToString("D" + symbolsNumInPlainTextBlock);

                offset += k;
            }
            Trace.WriteLine("\n");
        }

        public void Decrypt()
        {
            // По подстрокам текста из cryptedTextBox длинной n.ToString().Length символов: считывание текста, преобразование текста в BigInteger, расшифрование и запись в decryptedTextBox
            int symbolsNumInPlainTextBlock = n.ToString().Length;
            List<byte> decryptedBytes = [];
            int offset1 = 0;
            while (offset1 < mainWindow.cryptedTextBox.Text.Length)
            {
                string cryptedBlock = mainWindow.cryptedTextBox.Text.Substring(offset1, symbolsNumInPlainTextBlock);

                // Расшифрование
                if (!TryParseToBigInteger(cryptedBlock, out BigInteger cryptedBlockAsBigInteger))
                {
                    MessageBox.Show("Некорректные данные в поле шифрованных данных. Должно быть число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Trace.WriteLine("Nums in cryptedBlockAsBigInteger = " + cryptedBlockAsBigInteger.ToString().Length
                                + "; cryptedBlockAsBigInteger = " + cryptedBlockAsBigInteger.ToString());
                BigInteger decryptedBlock = BigInteger.ModPow(cryptedBlockAsBigInteger, private_key, n); // можно быть спокойным, что decryptedBlock < n, потому что decryptedBlock является остатком от деления на n


                // Преобразование BigInteger в массив байтов
                byte[] decryptedBlockBytes = decryptedBlock.ToByteArray();
                Trace.WriteLine("Nums in block = " + decryptedBlockBytes.Length + "; block = " + Encoding.UTF8.GetString(decryptedBlockBytes));
                decryptedBytes.AddRange(decryptedBlockBytes);

                offset1 += symbolsNumInPlainTextBlock;
            }
            // Расшифровка в decryptedTextBox
            mainWindow.decryptedTextBox.Text = Encoding.UTF8.GetString(decryptedBytes.ToArray());
            Trace.WriteLine("\n");

        }

        private void SimpleEncryptAndDecrypt()
        {
            // Простое шифрование и разшифрование
            {
                if (!TryParseToInt(mainWindow.inputTextBox.Text, out BigInteger input) || input <= 0)
                {
                    MessageBox.Show("Введены некорректные данные в поле ввода входных данных. Введите число");
                }

                // Зашифрование
                BigInteger crypted = BigInteger.ModPow(input, public_key, n);
                mainWindow.cryptedTextBox.Text = crypted.ToString();

                // Расшифрование
                BigInteger decrypted = BigInteger.ModPow(crypted, private_key, n);
                mainWindow.decryptedTextBox.Text = decrypted.ToString();
            }
        }

        private static bool TryParseToBigInteger(string text, out BigInteger result)
        {
            // Проверка на пустое поле
            if (string.IsNullOrWhiteSpace(text))
            {
                result = BigInteger.Zero;
                return false;
            }

            // Попытка парсинга в BigInteger
            bool success = BigInteger.TryParse(text, out result);
            return success;
        }

        private static bool TryParseToInt(string text, out BigInteger result)
        {
            // Проверка на пустое поле
            if (string.IsNullOrWhiteSpace(text))
            {
                result = 0;
                return false;
            }

            // Попытка парсинга в int
            bool success = BigInteger.TryParse(text, out result);
            return success;
        }

        private static void ExEuсlidAlg(BigInteger a, BigInteger b, out BigInteger e)
        {
            BigInteger[] U = [a, 1, 0];
            BigInteger[] V = [b, 0, 1];
            BigInteger[] T = new BigInteger[3];

            while (V[0] != 0)
            {
                BigInteger q = U[0] / V[0];
                T[0] = U[0] % V[0];
                T[1] = U[1] - q * V[1];
                T[2] = U[2] - q * V[2];
                for (int i = 0; i < U.Length; i++)
                    U[i] = V[i];
                for (int i = 0; i < V.Length; i++)
                    V[i] = T[i];
            }

            e = U[2];
        }

    }
}
