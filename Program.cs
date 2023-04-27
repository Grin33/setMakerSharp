﻿//Задача разбиения множеств : на вход поступает множество чисел.
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

namespace test1
{
    class Program
    {
        static object locker = new(); //инициализация замка
        static List<List<int>> fin = new List<List<int>>(); // список отработавших массивов, подходящих к решению
        static List<int> mass = new List<int> { 2, 3, 5, 7, 1, 2, 1, 1, 6, 4, 4, 6, 4, 2 }; //входной массив
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
        static bool Check_Set(ref List<int> mass1, ref int ans) //проверка массивов чисел, являются ли они решением
        {
            if (ans < 0) //если перебор чисел, выход из цикла
                return false;
            else if (ans > 0) //если не добор, продолжить итерации
                return true;
            else //ans == 0 //если массивы совпадают по сумме
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
                        if ((mass1.SequenceEqual(v)) || (mass_temp.SequenceEqual(v))) { check = false; }
                    }
                    if (check) //если нет
                    {
                        fin.Add(mass1); //запись отработавших массивов в глобальную переменную
                        fin.Add(mass_temp); //для того чтобы не выводить одни и те же массивы несколько раз
                        //Print_Mass(ref mass1); Console.Write(" = "); Print_Mass(ref mass_temp); Console.WriteLine();
                    }

                }
                return false;
            }
        }
        static void Sets_Maker(ref List<int> mass1, ref int ans) //цикл выборки 
        {
            for (int i = 0; i < mass1.Count; i++)
            {
                int ans_new = ans; //копировние переменных для каждой итерации
                var mass_new = new List<int>(mass1);
                ans_new -= mass_new[i]; //вычитание рабочего числа из требуемой суммы
                mass_new.RemoveAt(i); //удаление рабочего числа из локальной копии массива
                if (Check_Set(ref mass_new, ref ans_new)) { Sets_Maker(ref mass_new, ref ans_new); } //если сумма не равна 0, запустить еще одну итерацию
            }
        }


        static bool Check_Set(ref List<int> mass1, ref int ans, ref List<List<int>> t_fin)
        {
            if (ans < 0) //если перебор чисел, выход из цикла
                return false;
            else if (ans > 0) //если не добор, продолжить итерации
                return true;
            else //ans == 0 //если массивы совпадают по сумме
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
                {
                    foreach (List<int> v in t_fin) //проверяем, были ли уже такие же массивы (в потоке)
                    {
                        if ((mass1.SequenceEqual(v)) || (mass_temp.SequenceEqual(v))) { check = false; }
                    }
                    if (check) //если нет
                    {
                        t_fin.Add(mass1); //запись отработавших массивов в глобальную переменную
                        t_fin.Add(mass_temp); //для того чтобы не выводить одни и те же массивы несколько раз
                        //Print_Mass(ref mass1); Console.Write(" = "); Print_Mass(ref mass_temp); Console.WriteLine();
                    }

                }
                return false;
            }
        }

        static void Sets_Maker(ref List<int> mass1, ref int ans, ref List<List<int>> t_fin)
        {
            for (int i = 0; i < mass1.Count; i++)
            {
                int ans_new = ans; //копировние переменных для каждой итерации
                var mass_new = new List<int>(mass1);
                ans_new -= mass_new[i]; //вычитание рабочего числа из требуемой суммы
                mass_new.RemoveAt(i); //удаление рабочего числа из локальной копии массива
                if (Check_Set(ref mass_new, ref ans_new, ref t_fin)) { Sets_Maker(ref mass_new, ref ans_new, ref t_fin); } //если сумма не равна 0, запустить еще одну итерацию
            }
        }
        static void Sets_Maker_Parallel(List<int> mass1, int ans)
        {
            Parallel.For(0, mass1.Count, () => fin, (i, loop, t_fin) =>
            {
                t_fin = new List<List<int>>() { };
                var ans_new = ans;
                var mass_new = new List<int>(mass1);
                ans_new -= mass_new[i];
                mass_new.RemoveAt(i);
                if (Check_Set(ref mass_new, ref ans_new, ref t_fin)) { Sets_Maker(ref mass_new, ref ans_new, ref t_fin); }
                return t_fin;
            },
            (x) =>
            {
                lock (locker)
                {
                    if ((fin != null) && (x != null))
                        for (int i = 0; i < x.Count; i+=2)
                        {
                            for (int j = 0; j < fin.Count; j+=2)
                            {
                                if(i != x.Count)
                                {
                                    var temp = fin[j];
                                    var temp1 = fin[j + 1];
                                    var temp2 = x[i];
                                    var temp3 = x[i + 1];
                                    if (temp.SequenceEqual(temp2) || temp1.SequenceEqual(temp3)) ;
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

            Console.WriteLine("Straight");
            var sw = Stopwatch.StartNew(); //Запуск таймера
            sw.Start();
            Sets_Maker(ref mass1, ref checksol);
            Get_Answers(ref fin);
            sw.Stop();
            var temptime = sw.Elapsed;
            Console.WriteLine("Time Taken for Straight: " + temptime);

            fin.Clear();
            Console.WriteLine("Parallel");
            sw.Restart();
            Sets_Maker_Parallel(mass1, checksol);
            Get_Answers(ref fin);
            sw.Stop();
            Console.WriteLine("Time Taken for Parallel: " + sw.Elapsed);
            Console.WriteLine("Time Taken for Straight: " + temptime);
        }
    }
}
