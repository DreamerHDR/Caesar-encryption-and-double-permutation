using System.Collections.Specialized;
using System.Text;

string ABC = "";

for (int i = 0; i < 256; i++)
{
    ABC += (char)i;
}

string basePath = @"C:\code\Sec_1\";
string text = File.ReadAllText(basePath + "input.txt");


string[] keys = File.ReadAllLines(basePath + "keys.txt");

int key = int.Parse(keys[0]);
string keyWord = keys[1];
string key1 = keys[2];   // ключ для перестановки столбцов
string key2 = keys[3];    // ключ для перестановки строк

Console.WriteLine("=== НАЧАЛО РАБОТЫ ПРОГРАММЫ ===");
Console.WriteLine("Исходный текст:\n\t'" + text + "'");
Console.WriteLine($"Ключи: Caesar key={key}, keyword='{keyWord}', col key='{key1}', row key='{key2}'");
Console.WriteLine();

// ШИФРОВАНИЕ
Console.WriteLine("=== ЭТАП ШИФРОВАНИЯ ===");

Console.WriteLine("1. ШИФР ЦЕЗАРЯ:");
string cEncrypt = CaesarEncrypt(text, key, keyWord, ABC);
Console.WriteLine($"\tРезультат: '{cEncrypt}'");
Console.WriteLine();

Console.WriteLine("2. ДВОЙНАЯ ПЕРЕСТАНОВКА:");
string dblEncrypt = DoublePermutationEncrypt(cEncrypt, key1, key2, true);
Console.WriteLine($"\tИтоговый результат шифрования: '{dblEncrypt}'");
File.WriteAllText(basePath + "encrypted.txt", dblEncrypt);
Console.WriteLine();

// ДЕШИФРОВАНИЕ
Console.WriteLine("=== ЭТАП ДЕШИФРОВАНИЯ ===");

Console.WriteLine("1. ОБРАТНАЯ ДВОЙНАЯ ПЕРЕСТАНОВКА:");
string dblDecrypt = DoublePermutationDecrypt(dblEncrypt, key1, key2, true);
Console.WriteLine($"\tРезультат: '{dblDecrypt}'");
Console.WriteLine();

Console.WriteLine("2. ДЕШИФРОВАНИЕ ЦЕЗАРЯ:");
string cDecrypt = CaesarDecrypt(dblDecrypt, key, keyWord, ABC);
Console.WriteLine($"\tИтоговый результат дешифрования: '{cDecrypt}'");
File.WriteAllText(basePath + "decrypted.txt", cDecrypt);

Console.WriteLine("\n=== ПРОВЕРКА ===");
Console.WriteLine($"Исходный текст:  '{text}'");
Console.WriteLine($"Результат:       '{cDecrypt}'");
Console.WriteLine($"Совпадение: {text == cDecrypt}");


/* удаление повторяющихся символов
 */
static string NormalizeKey(string key, string ABC)
{
    if (string.IsNullOrEmpty(key)) return key;

    string result = new(key.Where(c => ABC.Contains(c))
        .Distinct()
        .ToArray());

    return result;
}


/* Построение заменяющего алфавита */
static string BuildKeyABC(string keyword, int key, string ABC)
{
    string normalKey = NormalizeKey(keyword, ABC);
    Console.WriteLine($"\tНормализованный ключ: '{normalKey}'");

    string shiftedABC = ABC.Substring(key) + ABC.Substring(0, key); //смещение
    Console.WriteLine($"\tСмещенный алфавит (сдвиг {key}): '{shiftedABC}'");

    string newABC = normalKey; //постр. нового алф
    foreach (char c in shiftedABC)
    {
        if (!newABC.Contains(c))
            newABC += c;
    }

    Console.WriteLine($"\tНовый алфавит замены: '{newABC}'");
    return newABC;
}

static string CaesarEncrypt(string text, int key, string keyword, string ABC)
{
    Console.WriteLine($"\tИсходный алфавит: '{ABC}'");
    string newABC = BuildKeyABC(keyword, key, ABC);

    StringBuilder result = new();
    Console.WriteLine("\tЗамена символов:");
    foreach (char c in text)
    {
        int idx = ABC.IndexOf(c);
        if (idx == -1)
        {
            result.Append(c);
            Console.WriteLine($"\t  '{c}' -> '{c}' (символ не найден в алфавите)");
        }
        else
        {
            char encryptedChar = newABC[idx];
            result.Append(encryptedChar);
            Console.WriteLine($"\t  '{c}' (позиция {idx}) -> '{encryptedChar}'");
        }
    }

    return result.ToString();
}


static string CaesarDecrypt(string text, int key, string keyword, string ABC)
{
    Console.WriteLine($"\tАлфавит замены: '{ABC}'");
    string newABC = BuildKeyABC(keyword, key, ABC);

    StringBuilder result = new();
    Console.WriteLine("\tОбратная замена символов:");
    foreach (char c in text)
    {
        int idx = newABC.IndexOf(c);
        if (idx == -1)
        {
            result.Append(c);
            Console.WriteLine($"\t  '{c}' -> '{c}' (символ не найден в алфавите замены)");
        }
        else
        {
            char decryptedChar = ABC[idx];
            result.Append(decryptedChar);
            Console.WriteLine($"\t  '{c}' (позиция {idx}) -> '{decryptedChar}'");
        }
    }

    return result.ToString();
}


static string DoublePermutationEncrypt(string text, string key1, string key2, bool verbose = false)
{
    int cols = key1.Length;
    int rows = key2.Length;
    if (cols == 0 || rows == 0) return text ?? string.Empty;

    int blockSize = rows * cols;

    int[] colOrder = GetOrderIndices(key1);
    int[] rowOrder = GetOrderIndices(key2);

    if (verbose)
    {
        Console.WriteLine($"\tРазмер блока: {rows}×{cols} = {blockSize} символов");
        Console.WriteLine($"\tПорядок столбцов: {string.Join(" ", colOrder.Select((x, i) => $"{i}->{x}"))}");
        Console.WriteLine($"\tПорядок строк: {string.Join(" ", rowOrder.Select((x, i) => $"{i}->{x}"))}");
    }

    var sb = new StringBuilder();

    for (int blockNum = 0; blockNum * blockSize < text.Length; blockNum++)
    {
        int pos = blockNum * blockSize;
        string block = text.Substring(pos, Math.Min(blockSize, text.Length - pos))
                          .PadRight(blockSize, ' ');

        if (verbose)
        {
            Console.WriteLine($"\n\tБЛОК {blockNum + 1}: '{block}'");
            Console.WriteLine($"\tИсходная таблица ({rows}×{cols}):");
        }

        // Заполнение исходной таблицы построчно
        char[,] table = new char[rows, cols];
        int k = 0;
        for (int i = 0; i < rows; i++)
        {
            if (verbose) Console.Write("\t");
            for (int j = 0; j < cols; j++)
            {
                table[i, j] = block[k++];
                if (verbose) Console.Write($"{table[i, j]} ");
            }
            if (verbose) Console.WriteLine();
        }

        // Переставляем столбцы
        char[,] colPermuted = new char[rows, cols];
        for (int newJ = 0; newJ < cols; newJ++)
            for (int i = 0; i < rows; i++)
                colPermuted[i, newJ] = table[i, colOrder[newJ]];

        if (verbose)
        {
            Console.WriteLine($"\tПосле перестановки столбцов:");
            for (int i = 0; i < rows; i++)
            {
                Console.Write("\t");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{colPermuted[i, j]} ");
                Console.WriteLine();
            }
        }

        // Переставляем строки
        char[,] rowPermuted = new char[rows, cols];
        for (int newI = 0; newI < rows; newI++)
            for (int j = 0; j < cols; j++)
                rowPermuted[newI, j] = colPermuted[rowOrder[newI], j];

        if (verbose)
        {
            Console.WriteLine($"\tПосле перестановки строк:");
            for (int i = 0; i < rows; i++)
            {
                Console.Write("\t");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{rowPermuted[i, j]} ");
                Console.WriteLine();
            }
            Console.WriteLine($"\tРезультат блока: '{new string(rowPermuted.Cast<char>().ToArray())}'");
        }

        // Читаем по строкам весь зашифрованный блок
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sb.Append(rowPermuted[i, j]);
    }

    return sb.ToString();
}

// Вспомогательный: возвращает массив индексов старых позиций в порядке сортировки ключа.
static int[] GetOrderIndices(string key)
{
    return key
        .Select((c, idx) => new { c, idx })
        .OrderBy(x => x.c)
        .ThenBy(x => x.idx)
        .Select(x => x.idx)
        .ToArray();
}

static string DoublePermutationDecrypt(string cipherText, string key1, string key2, bool verbose = false)
{
    int cols = key1.Length;
    int rows = key2.Length;
    if (cols == 0 || rows == 0) return cipherText ?? string.Empty;

    int blockSize = rows * cols;

    int[] colOrder = GetOrderIndices(key1);
    int[] rowOrder = GetOrderIndices(key2);

    // Строим обратные перестановки
    int[] invColOrder = new int[cols];
    for (int newJ = 0; newJ < cols; newJ++)
        invColOrder[colOrder[newJ]] = newJ;

    int[] invRowOrder = new int[rows];
    for (int newI = 0; newI < rows; newI++)
        invRowOrder[rowOrder[newI]] = newI;

    if (verbose)
    {
        Console.WriteLine($"\tРазмер блока: {rows}×{cols} = {blockSize} символов");
        Console.WriteLine($"\tОбратный порядок столбцов: {string.Join(" ", invColOrder.Select((x, i) => $"{i}->{x}"))}");
        Console.WriteLine($"\tОбратный порядок строк: {string.Join(" ", invRowOrder.Select((x, i) => $"{i}->{x}"))}");
    }

    var sb = new StringBuilder();

    for (int blockNum = 0; blockNum * blockSize < cipherText.Length; blockNum++)
    {
        int pos = blockNum * blockSize;
        string block = cipherText.Substring(pos, Math.Min(blockSize, cipherText.Length - pos));

        if (verbose)
        {
            Console.WriteLine($"\n\tБЛОК {blockNum + 1}: '{block}'");
            Console.WriteLine($"\tЗашифрованная таблица ({rows}×{cols}):");
        }

        // Загружаем зашифрованный блок построчно
        char[,] rowPermuted = new char[rows, cols];
        int k = 0;
        for (int i = 0; i < rows; i++)
        {
            if (verbose) Console.Write("\t");
            for (int j = 0; j < cols; j++)
            {
                rowPermuted[i, j] = block[k++];
                if (verbose) Console.Write($"{rowPermuted[i, j]} ");
            }
            if (verbose) Console.WriteLine();
        }

        // Обратно переставляем строки
        char[,] colPermuted = new char[rows, cols];
        for (int newI = 0; newI < rows; newI++)
            for (int j = 0; j < cols; j++)
                colPermuted[rowOrder[newI], j] = rowPermuted[newI, j];

        if (verbose)
        {
            Console.WriteLine($"\tПосле обратной перестановки строк:");
            for (int i = 0; i < rows; i++)
            {
                Console.Write("\t");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{colPermuted[i, j]} ");
                Console.WriteLine();
            }
        }

        // Обратно переставляем столбцы
        char[,] table = new char[rows, cols];
        for (int newJ = 0; newJ < cols; newJ++)
            for (int i = 0; i < rows; i++)
                table[i, colOrder[newJ]] = colPermuted[i, newJ];

        if (verbose)
        {
            Console.WriteLine($"\tПосле обратной перестановки столбцов:");
            for (int i = 0; i < rows; i++)
            {
                Console.Write("\t");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{table[i, j]} ");
                Console.WriteLine();
            }
            Console.WriteLine($"\tРезультат блока: '{new string(table.Cast<char>().ToArray()).TrimEnd()}'");
        }

        // Читаем по строкам исходный блок
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sb.Append(table[i, j]);
    }

    return sb.ToString().TrimEnd();
}