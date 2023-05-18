//Задача разбиения множеств : на вход поступает множество чисел.
//Требуется разбить это множество на два, с одинаковой суммой чисел в обеих.
//Использовать Параллелизм
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Channels;
using System.Linq;
using System.Collections;

namespace Set_Maker_New
{
    class Program
    {
        static object locker = new(); //инициализация замка
        static List<List<int>> fin = new List<List<int>>(); // список отработавших массивов, подходящих к решению
        static List<int> mass = new List<int> { 2, 6, 8, 4, 2, 6, 9, 1, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, }; //входной массив
        //static List<int> mass = new List<int> { 2, 3, 5, 7, 1, 2, 1, 1, 6, 4 };

        static void Get_Answers(ref List<List<int>> ans) // функция вывода ответа
        {
            for (int i = 0; i < ans.Count; i += 2)
            {
                Print_Mass(ans[i]);
                Console.Write(" = ");
                Print_Mass(ans[i + 1]);
                Console.WriteLine();

            }
        }
        static void Print_Mass(List<int> mass1) //функция вывода массива
        {
            foreach (int v in mass1)
            {
                Console.Write(v + ",");
            }
        }

        static void isSol(ref List<int> mass1, ref List<int> mass)
        {
            var sum1 = 0;
            var sum2 = 0;
            foreach (var i in mass1) { sum1 += i; }
            foreach (var i in mass) { sum2 += i; }
            sum2 -= sum1;
            if (sum1 == sum2)
            {
                var mass_temp = new List<int>(mass); //копирование изначального массива
                foreach (int i in mass1) //цикл убирает из копии изначального цикла те значения, которые имеются в рабочем массиве
                {
                    for (int k = 0; k < mass_temp.Count; k++)
                    {
                        if (i == mass_temp[k]) { mass_temp.RemoveAt(k); break; }
                    }
                }
                bool check = true;
                lock (locker) //Замок, т.к далее идет работа с глобальной переменной (отмена гонки данных)
                {
                    foreach (List<int> v in fin) //проверяем, были ли уже такие же массивы
                    {
                        bool equal = v.OrderBy(x => x).ToList().SequenceEqual(mass1.OrderBy(x => x).ToList());
                        bool equal1 = v.OrderBy(x => x).ToList().SequenceEqual(mass_temp.OrderBy(x => x).ToList());
                        if (equal || equal1) { check = false; }
                    }
                    if (check) //если нет
                    {
                        fin.Add(mass1); //запись отработавших массивов в глобальную переменную
                        fin.Add(mass_temp); //для того чтобы не выводить одни и те же массивы несколько раз
                    }

                }
            }

        }

        static void Sets_Maker_Str(List<int> newmass, ref List<int> mass, int i)
        {
            int v = i + 1;
            for (int n = v; n < mass.Count; n++)
            {
                var newmass1 = new List<int>(newmass);
                newmass1.Add(mass[n]);
                isSol(ref newmass1, ref mass);
                Sets_Maker_Str(newmass1, ref mass, n);
            }
        }
        static void Sets_Maker_Str(ref List<int> mass)
        {
            for (int i = 0; i < mass.Count; i++)
            {
                var newmass = new List<int>();
                newmass.Add(mass[i]);
                isSol(ref newmass, ref mass);
                Sets_Maker_Str(newmass, ref mass, i);
            }
        }

        static void isSol(List<int> newmass, ref List<int> mass, ref List<List<int>> tempfin)
        {
            var sum1 = 0;
            var sum2 = 0;
            foreach (var i in newmass) { sum1 += i; }
            foreach (var i in mass) { sum2 += i; }
            sum2 -= sum1;
            if (sum1 == sum2)
            {
                var mass_temp = new List<int>(mass); //копирование изначального массива
                foreach (int i in newmass) //цикл убирает из копии изначального цикла те значения, которые имеются в рабочем массиве
                {
                    for (int k = 0; k < mass_temp.Count; k++)
                    {
                        if (i == mass_temp[k]) { mass_temp.RemoveAt(k); break; }
                    }
                }
                bool check = true;
                foreach (var v in tempfin)
                {
                    bool equal = v.OrderBy(x => x).ToList().SequenceEqual(newmass.OrderBy(x => x).ToList());
                    bool equal1 = v.OrderBy(x => x).ToList().SequenceEqual(mass_temp.OrderBy(x => x).ToList());
                    if (equal || equal1) { check = false; }
                }
                if (check)
                {
                    tempfin.Add(newmass);
                    tempfin.Add(mass_temp);
                }
            }
        }

        static void Sets_Maker(List<int> newmass, ref List<int> mass, ref List<List<int>> tempfin, int i)
        {
            int v = i + 1;
            for (int n = v; n < mass.Count; n++)
            {
                var newmass1 = new List<int>(newmass);
                newmass1.Add(mass[n]);
                isSol(newmass1, ref mass, ref tempfin);
                Sets_Maker(newmass1, ref mass, ref tempfin, n);
            }
        }

        static void Sets_Maker_Parallel(List<int> mass)
        {
            Parallel.For(0, mass.Count, () => new List<List<int>>(), (i, loop, tempfin) =>
            {
                var newMass = new List<int>();
                newMass.Add(mass[i]);
                isSol(newMass, ref mass, ref tempfin);
                Sets_Maker(newMass, ref mass, ref tempfin, i);

                return tempfin;
            },
            (x) =>
            {
                lock (locker)
                {
                    if ((fin != null) && (x != null))
                        for (int i = 0; i < x.Count; i += 2)
                        {
                            for (int j = 0; j < fin.Count; j += 2)
                            {
                                //if (i != x.Count)
                                {
                                    var temp = fin[j];
                                    var temp1 = fin[j + 1];
                                    var temp2 = x[i];
                                    var temp3 = x[i + 1];
                                    bool equal = temp.OrderBy(f => f).ToList().SequenceEqual(temp2.OrderBy(f => f).ToList());
                                    bool equal1 = temp1.OrderBy(f => f).ToList().SequenceEqual(temp3.OrderBy(f => f).ToList());
                                    bool equal2 = temp.OrderBy(f => f).ToList().SequenceEqual(temp3.OrderBy(f => f).ToList());
                                    bool equal3 = temp1.OrderBy(f => f).ToList().SequenceEqual(temp2.OrderBy(f => f).ToList());
                                    if (equal1 || equal || equal2 || equal3)
                                    {
                                        x.RemoveAt(i); x.RemoveAt(i);
                                        i -= 2;
                                        break;
                                    }
                                }

                            }
                        }
                    fin.AddRange(x);
                }
            }
            );
        }

        static void Main()
        {
            var mass1 = new List<int>(mass);
            int checksol = 0;
            foreach (int i in mass1) { checksol += i; }
            //если сумма всех чисел в изначальном массиве нечетна, то и ответа не может существовать
            if ((checksol % 2) == 1) { Console.WriteLine("No answer"); return; }
            checksol /= 2; //такая сумма чисел должна быть в обоих массивах

            Print_Mass(mass);
            Console.WriteLine();

            Console.WriteLine("Straight");
            var sw = Stopwatch.StartNew(); //Запуск таймера
            sw.Start();
            Sets_Maker_Str(ref mass1);
            Get_Answers(ref fin);
            sw.Stop();
            var temptime = sw.Elapsed;
            Console.WriteLine("Time Taken for Straight: " + temptime);

            fin = new List<List<int>>();
            Console.WriteLine("Parallel");
            sw.Restart();
            Sets_Maker_Parallel(mass1);

            Get_Answers(ref fin);
            sw.Stop();
            //Console.WriteLine(fin.Count);
            Console.WriteLine("Time Taken for Parallel: " + sw.Elapsed);
            Console.WriteLine("Time Taken for Straight: " + temptime);
        }
    }
}
