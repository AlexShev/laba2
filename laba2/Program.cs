using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace laba2
{
    class Program
    {
        static void Main()
        {
            string temp = "C:\\Users\\Alex\\source\\repos\\programula\\programula\\Program.cs";

            Console.WriteLine(new Caunter().Analiz(temp));
        }
    }

    class Caunter
    {
        int N1 = 0, N2 = 0; //N1 - Общее число всех операторов, N2 - общее число всех операндов
        int n1 = 0, n2 = 0; //n1 - число простых операторов, n2 - число простых операндов

        // вычисление
        double СalculateVolume() => (N1 + N2) * Math.Log2(n1 + n2); 

        // операторы
        private static readonly string[] _OPERATORS = {  "}", "(", "[", ",", ".", ";", "=", "+", "-", "*", "/", "<<", ">>",
            "+=", "-=", "*=", "/=", "<<=", ">>=", "++", "--", "new" };

        // символы которые нужно пропускать
        private static readonly string[] _SPECIAL = { "{", ")", "]", "int", "string", "double", "char", "float", "return", "static", "void", "", "\n" };

        // словарь в котором по ключу храниться количество операторов в программном коде
        private Dictionary<string, int> _operators = new();

        // словарь в котором по ключу храниться количество операндов в программном коде
        private Dictionary<string, int> _operands = new();

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        private Dictionary<string, int> _standartMethod = new();

        // словарь в котором по ключу храниться количество собственно разработанных методов в программном коде
        private Dictionary<string, Caunter> _programMethod = new();

        // метод по анализу програмного кода из файла
        public double Analiz(string file)
        {
            // открытие файла
            System.IO.StreamReader sr = new(file, Encoding.Default);
            string line;

            // пока не дойдём до класса Программа - пропускаем строки
            while (!sr.ReadLine().Contains("Program")) { }

            // считывание строки
            while ((line = sr.ReadLine()) != null)
            {
                // если строка содержит метку подпрограммы
                if (line.Contains("//=="))
                {
                    // счётчик по анализу для подпрограммы
                    Caunter caunter = new();

                    // добавление найденрго метода в словарь для собственно разработанных методов
                    _programMethod.Add(caunter.AnalizMethod(sr), caunter);
                }
                else // иначе
                {
                    // анализ строки
                    Analize(line);
                }
            }

            // закрытие файла
            sr.Close();

            // цикл для прибавления того что храниться в подпрограммах
            foreach (var item in _programMethod)
            {
                // сколько раз метод встречается в коде
                int k = (_standartMethod.ContainsKey(item.Key)) ? _standartMethod[item.Key] : 0;

                // проход по контейнеру операторов
                foreach (var it in item.Value._operators)
                {
                    AddToConteiner(_operators, it.Key, it.Value * k);
                }
                // проход по контейнеру операндов
                foreach (var it in item.Value._operands)
                {
                    AddToConteiner(_operands, it.Key, it.Value * k);
                }
                // проход по контейнеру стандартных методов
                foreach (var it in item.Value._standartMethod)
                {
                    AddToConteiner(_standartMethod, it.Key, it.Value * k);
                }
            }

            // складываем количество уникальных операторов и методов
            n1 = _operators.Count + _standartMethod.Count;
            n2 = _operands.Count;

            // складываем количество операторов и методов
            N1 = CountInt(_operators) + CountInt(_standartMethod);
            N2 = CountInt(_operands);

            // возврат результата
            return СalculateVolume();
        }

        // подсчёт количества элементов в контейнере
        private int CountInt(Dictionary<string, int> dictionary)
        {
            int counter = 0;

            foreach (var item in dictionary)
            {
                counter += item.Value;
            }

            return counter;
        }

        // анализ подпрограммы
        string AnalizMethod(System.IO.StreamReader sr)
        {
            string line = sr.ReadLine();

            string[] temp = line.Split();
            string methodName = string.Empty;

            // поиск названия метода
            foreach (var item in temp)
            {
                if (item.Contains("("))
                {
                    methodName = item;
                    break;
                }
            }

            // пока не встретится метка - анализировать строку
            while (true)
            {
                line = sr.ReadLine();

                if (line.Contains("//=="))
                {
                    break;
                }

                Analize(line); // в line содержится очередная строчка из файла
            }

            // название метода
            return methodName;
        }


        // анализ одной строки
        public void Analize(string line)
        {
            // коментарии в анализе не участвуют
            if (line.Contains("//"))
            {
                return;
            }

            // разделение строки по прбелу
            string[] words = line.Split();

            // для переменных строковых типов
            string temp = string.Empty;

            // анализ полученного слова
            for (int i = 0; i < words.Length; i++)
            {
                // если это строковый тип данных
                if (CountWords(words[i], "\"") == 1)
                {
                    temp = words[i++] + " ";

                    // пока не встретиться закрывающая кавычка
                    while (!words[i].Contains("\""))
                    {
                        temp += (words[i++] + " ");
                    }

                    words[i] = temp + words[i];
                }

                // если слова не надо пропускать
                if (!_SPECIAL.Contains(words[i]))
                {
                    // это метод
                    if (words[i].Contains("(") && !words[i].Equals("("))
                    {
                        AddToConteiner(_standartMethod, words[i]);
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
                    if (!words[i].All(char.IsDigit))
                    {
                        AddToConteiner(_operators, ".", CountWords(words[i], "."));
                    }
                }
            }
        }

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
    }
}
