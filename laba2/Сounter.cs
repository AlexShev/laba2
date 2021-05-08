using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace laba2
{
    class Сounter
    {
        // метод по анализу програмного кода из файла
        public double Analiz(string file)
        {
            Inizializet();

            // открытие файла
            System.IO.StreamReader sr = new(file, Encoding.Default);
            string line;

            // пока не дойдём до класса Программа - пропускаем строки
            while (!sr.ReadLine().Contains("Main(")) { }

            while ((line = sr.ReadLine()) != null)
            {
                // анализ строки
                AnalizString(line);
            }

            // закрытие файла
            sr.Close();

            // возврат результата
            return СalculateVolume(SumDictionaryKey(_operators) + SumDictionaryKey(_methods),
                SumDictionaryKey(_operands), _operators.Count + _methods.Count, _operands.Count);
        }

        // анализ одной строки
        private void AnalizString(string line)
        {
            // разделение строки по прбелу
            string[] words = line.Trim().Split();

            int j = 0;

            if (words.Length == 0)
                return;

            // это метод возвращающий void
            if (IsMathod(words[0]))
            {
                AddToConteiner(_methods, words[0]);

                AddToConteiner(_operators, ".", CountWords(words[0], "."));

                j = 1;
            }

            // анализ полученного слова
            for (int i = j; i < words.Length; i++)
            {
                // если слова не надо пропускать
                if (!_SPECIAL.Contains(words[i]))
                {
                    // коментарии в анализе не участвуют
                    if (words[i] == "//")
                    {
                        return;
                    }

                    // если это строковый тип данных у которого в строке встречается пробел
                    if (CountWords(words[i], "\"") == 1)
                    {
                        string temp = words[i++] + " ";

                        // пока не встретиться закрывающая кавычка
                        while (!words[i].Contains("\""))
                        {
                            temp += (words[i++] + " ");
                        }

                        words[i] = temp + words[i];
                    }

                    // это метод
                    if (IsMathod(words[i]))
                    {
                        AddToConteiner(_methods, words[i]);

                        AddToConteiner(_operands, words[i]);
                    }
                    // это операторы
                    else if (_OPERATORS.Contains(words[i]))
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operators, words[i]);
                    }
                    else // это операнд
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operands, words[i]);
                    }

                    // переменная может быть полем, к которому обращаются через оператор .
                    AddToConteiner(_operators, ".", CountWords(words[i], "."));
                }
            }

        }

        private static bool IsMathod(string word) => word.Contains("(") && !word.Equals("(");


        // прибавить слово и количество слов
        private static void AddToConteiner(Dictionary<string, int> dictionary, string data, int count = 1)
        {
            if (dictionary.ContainsKey(data))
            {
                dictionary[data] += count;
            }
            else
            {
                dictionary.Add(data, count);
            }
        }

        // сколько раз данная продстока в данной строке
        private static int CountWords(string baseString, string substring) => baseString.Split(new[] { substring }, StringSplitOptions.None).Length - 1;

        // очистка полей
        private void Inizializet()
        {
            _operators = new();
            _operands = new();
            _methods = new();
        }

        // подсчёт количества элементов в контейнере
        private static int SumDictionaryKey(Dictionary<string, int> dictionary)
        {
            int counter = 0;

            foreach (var item in dictionary)
            {
                counter += item.Value;
            }

            return counter;
        }

        // вычисление
        //N1 - Общее число всех операторов, N2 - общее число всех операндов
        //n1 - число простых операторов, n2 - число простых операндов
        private static double СalculateVolume(int N1, int N2, int n1, int n2) => (N1 + N2) * Math.Log2(n1 + n2);

        // операторы
        private static readonly string[] _OPERATORS = {  "}", "(", "[", ",", ".", ";", "=", "+", "-", "*", "/", "<<", ">>",
            "+=", "-=", "*=", "/=", "<<=", ">>=", "++", "--", "new", "==", "!=", "<=", ">=", ">", "<", "!"};

        // символы которые нужно пропускать
        private static readonly string[] _SPECIAL = { "", " ", "\n", "{", ")", "]", "int", "string", "double", "char", "float", "return", "static", "void", "public", "private", "protected" };
        
        // словарь в котором по ключу храниться количество операторов в программном коде
        private Dictionary<string, int> _operators;

        // словарь в котором по ключу храниться количество операндов в программном коде
        private Dictionary<string, int> _operands;

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        private Dictionary<string, int> _methods;
    }
}