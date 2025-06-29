using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class BigramAnalysis
{
    static readonly string alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
    // Словарь для сопоставления символов алфавита с их индексами
    static Dictionary<char, int> alphabetIndexMap;

    static void Main()
    {
        // Инициализация словаря для сопоставления индексов алфавита
        InitializeAlphabetIndexMap();

        // Загрузка текста и преобразование его в нижний регистр
        string text = File.ReadAllText("WarAndWorld.txt").ToLower();

        // Создание таблицы частот биграмм
        double[,] bigrams = GetBigrams(text);

        // Сохранение таблицы биграмм в файл для дальнейшего использования
        SaveBigrams("bigrams.txt", bigrams);

        // Дешифровка текста из файла input.txt
        string inputText = File.ReadAllText("input.txt").ToLower();
        // Инициализация ключа (алфавит в исходном порядке)
        string key1 = alphabet;

        // Выполняем биграммный анализ и дешифровку с использованием алгоритма "восхождения на холм"
        string decryptedText = HillClimbDecrypt(inputText, key1, bigrams);

        // Выводим дешифрованный текст
        Console.WriteLine(decryptedText);
    }

    // Метод для выполнения дешифровки с помощью "восхождения на холм"
    static string HillClimbDecrypt(string text, string key1, double[,] bigrams)
    {
        // Дешифруем текст с текущим ключом
        string text1 = Decrypt(text, key1);
        // Получаем биграммы дешифрованного текста
        double[,] textBigrams = GetBigrams(text1);
        // Рассчитываем расстояние (разницу) между биграммами текста и исходными биграммами
        double dist1 = GetDistance(textBigrams, bigrams);
        int noSwapCount = 0; // Счетчик количества неудачных замен

        // Цикл продолжается до тех пор, пока не пройдет 1000 итераций без улучшений
        while (noSwapCount < 10000)
        {
            // Меняем два случайных символа в ключе
            string key2 = SwapRandomChars(key1);
            // Дешифруем текст с новым ключом
            string text2 = Decrypt(text, key2);
            // Получаем биграммы нового дешифрованного текста
            double[,] textBigrams2 = GetBigrams(text2);
            // Считаем расстояние между биграммами
            double dist2 = GetDistance(textBigrams2, bigrams);

            // Если расстояние с новым ключом меньше, обновляем ключ и расстояние
            if (dist2 < dist1)
            {
                key1 = key2;
                dist1 = dist2;
                noSwapCount = 0; // Сбрасываем счетчик неудачных замен
                Console.WriteLine(dist1); // Выводим текущее расстояние
            }
            else
            {
                noSwapCount++; // Увеличиваем счетчик, если улучшений нет
            }
        }

        // Возвращаем дешифрованный текст с наилучшим найденным ключом
        return Decrypt(text, key1);
    }

    // Метод для дешифровки текста с заданным ключом
    static string Decrypt(string text, string key)
    {
        StringBuilder decrypted = new StringBuilder();
        // Создаем карту индексов для символов ключа
        Dictionary<char, int> keyMap = key.Select((value, index) => new { value, index })
                                          .ToDictionary(pair => pair.value, pair => pair.index);

        // Проходим по каждому символу текста
        foreach (char c in text)
        {
            // Если символ есть в алфавите, дешифруем его
            if (alphabetIndexMap.ContainsKey(c))
            {
                decrypted.Append(alphabet[keyMap[c]]);
            }
        }

        return decrypted.ToString(); // Возвращаем дешифрованный текст
    }

    // Метод для инициализации карты индексов алфавита
    static void InitializeAlphabetIndexMap()
    {
        alphabetIndexMap = new Dictionary<char, int>();
        for (int i = 0; i < alphabet.Length; i++)
        {
            // Присваиваем каждому символу его индекс
            alphabetIndexMap[alphabet[i]] = i;
        }
    }

    // Метод для получения частот биграмм из текста
    static double[,] GetBigrams(string text)
    {
        double[,] bigrams = new double[33, 33]; // Матрица частот
        int sum = 0; // Общая сумма биграмм

        // Проходим по всем символам текста, исключая последний, так как он не может образовывать биграмму
        for (int i = 0; i < text.Length - 1; i++)
        {
            // Если оба символа биграммы есть в алфавите
            if (alphabetIndexMap.ContainsKey(text[i]) && alphabetIndexMap.ContainsKey(text[i + 1]))
            {
                // Увеличиваем счетчик соответствующей биграммы
                bigrams[alphabetIndexMap[text[i]], alphabetIndexMap[text[i + 1]]]++;
                sum++; // Увеличиваем общую сумму биграмм
            }
        }

        // Нормализуем частоты биграмм, деля каждую частоту на общую сумму биграмм
        for (int i = 0; i < 33; i++)
        {
            for (int j = 0; j < 33; j++)
            {
                bigrams[i, j] /= sum;
            }
        }

        return bigrams; // Возвращаем таблицу частот биграмм
    }

    // Метод для сохранения частот биграмм в файл
    static void SaveBigrams(string fileName, double[,] bigrams)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            // Проходим по строкам и столбцам таблицы биграмм
            for (int i = 0; i < 33; i++)
            {
                for (int j = 0; j < 33; j++)
                {
                    // Записываем значение каждой биграммы в файл
                    writer.Write(bigrams[i, j] + " ");
                }
                writer.WriteLine(); // Переход на новую строку после записи столбцов
            }
        }
    }
    // Метод для расчета расстояния (разницы) между двумя матрицами биграмм
    static double GetDistance(double[,] bigrams1, double[,] bigrams2)
    {
        double sum = 0;

        // Суммируем абсолютные разности между частотами соответствующих биграмм
        for (int i = 0; i < 33; i++)
        {
            for (int j = 0; j < 33; j++)
            {
                sum += Math.Abs(bigrams1[i, j] - bigrams2[i, j]);
            }
        }

        return sum; // Возвращаем суммарное расстояние
    }

    // Метод для случайной замены двух символов в строке
    static string SwapRandomChars(string input)
    {
        Random random = new Random();
        int index1 = random.Next(0, input.Length); // Первый случайный индекс
        int index2;

        // Генерируем второй индекс, чтобы он не совпадал с первым
        do
        {
            index2 = random.Next(0, input.Length);
        } while (index1 == index2);

        // Меняем местами два символа
        char[] chars = input.ToCharArray();
        (chars[index1], chars[index2]) = (chars[index2], chars[index1]);

        return new string(chars); // Возвращаем строку с замененными символами
    }
}
