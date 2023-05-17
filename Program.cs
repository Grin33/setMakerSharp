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

namespace Set_MakerSharp
{
    class Program
    {
        static object locker = new(); //инициализация замка
        static List<List<decimal>> fin = new List<List<decimal>>(); // список отработавших массивов, подходящих к решению
        static List<decimal> mass = new List<decimal> { 2, 4, 6 }; //входной массив
        //static List<int> mass = new List<int> { 2, 3, 5, 7, 1, 2, 1, 1, 6, 4 };

        static void Get_Answers(ref List<List<decimal>> ans) // функция вывода ответа
        {
            for (int i = 0; i < ans.Count; i += 2)
            {
                Print_Mass(ans[i]);
                Console.Write(" = ");
                Print_Mass(ans[i + 1]);
                Console.WriteLine();

            }
        }
        static void Print_Mass(List<decimal> mass1) //функция вывода массива
        {
            foreach (decimal v in mass1)
            {
                Console.Write(v + ",");
            }
        }

        static void Sets_Maker()
        {
            //здесь должен быть последовательный алгоритм (coming soon...)
        }

        static void isSol(List<decimal> newmass, ref List<decimal> mass, ref List<List<decimal>> tempfin)
        {
            var sum1 = 0m;
            var sum2 = 0m;
            foreach (var i in newmass) { sum1 += i; }
            foreach (var i in mass) { sum2 += i; }
            sum2 -= sum1;
            if (sum1 == sum2)
            {
                var mass_temp = new List<decimal>(mass); //копирование изначального массива
                foreach (decimal i in newmass) //цикл убирает из копии изначального цикла те значения, которые имеются в рабочем массиве
                {
                    for (int k = 0; k < mass_temp.Count; k++)
                    {
                        if (i == mass_temp[k]) { mass_temp.RemoveAt(k); break; }
                    }
                }
                bool check = true;
                foreach (var v in tempfin)
                {
                    if ((newmass.SequenceEqual(v)) || (mass_temp.SequenceEqual(v))) { check = false; }
                }
                if (check)
                {
                    tempfin.Add(newmass);
                    tempfin.Add(mass_temp);
                }
            }
        }

        static void Sets_Maker(List<decimal> newmass, ref List<decimal> mass, ref List<List<decimal>> tempfin, int i)
        {
            int v = i + 1;
            for (int n = v; n < mass.Count; n++)
            {
                var newmass1 = new List<decimal>(newmass);
                newmass1.Add(mass[n]);
                isSol(newmass1, ref mass, ref tempfin);
                Sets_Maker(newmass1, ref mass, ref tempfin, n);
            }
        }

        static void Sets_Maker_Parallel(List<decimal> mass)
        {
            Parallel.For(0, mass.Count, () => new List<List<decimal>>(), (i, loop, tempfin) =>
            {
                var newMass = new List<decimal>();
                newMass.Add(mass[i]);
                isSol(newMass, ref mass, ref tempfin);
                Sets_Maker(newMass, ref mass, ref tempfin, i);

                return tempfin;
            },
            (tempfin) =>
            {
                lock (locker)
                {
                    if ((fin != null) && (tempfin != null))
                        for (int i = 0; i < tempfin.Count; i += 2)
                        {
                            for (int j = 0; j < fin.Count; j += 2)
                            {
                                if (i != tempfin.Count)
                                {
                                    var temp = fin[j];
                                    var temp1 = fin[j + 1];
                                    var temp2 = tempfin[i];
                                    var temp3 = tempfin[i + 1];
                                    if (temp.SequenceEqual(temp2) || temp1.SequenceEqual(temp3)) ;
                                    {
                                        tempfin.RemoveAt(i); tempfin.RemoveAt(i);
                                        i -= 2;
                                        break;
                                    }
                                }

                            }
                        }
                    fin.AddRange(tempfin);
                }
            }
            );
        }

        static void Main()
        {
            var mass1 = new List<decimal>(mass);
            decimal checksol = 0;
            foreach (decimal i in mass1) { checksol += i; }
            //если сумма всех чисел в изначальном массиве нечетна, то и ответа не может существовать
            if ((checksol % 2) != 0) { Console.WriteLine("No answer"); return; }
            checksol /= 2; //такая сумма чисел должна быть в обоих массивах

            Console.WriteLine("Straight");
            var sw = Stopwatch.StartNew(); //Запуск таймера
            sw.Start();
            //Sets_Maker(ref mass1);
            Get_Answers(ref fin);
            sw.Stop();
            var temptime = sw.Elapsed;
            Console.WriteLine("Time Taken for Straight: " + temptime);

            fin.Clear();
            Console.WriteLine("Parallel");
            sw.Restart();
            Sets_Maker_Parallel(mass1);
            Get_Answers(ref fin);
            sw.Stop();
            Console.WriteLine("Time Taken for Parallel: " + sw.Elapsed);
            Console.WriteLine("Time Taken for Straight: " + temptime);
        }
    }
}
