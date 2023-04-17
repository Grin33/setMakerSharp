using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Channels;
using System.Linq;

namespace setMakerSharp
{
    class Program
    {
        static object locker = new();
        static List<List<int>> fin = new List<List<int>>();
        static List<int> mass = new List<int> { 2, 3, 5, 7, 1, 2, 1, 1, 6, 4, 4, 6, 4, 2 };
        static void Print_Mass(ref List<int> mass1)
        {
            foreach(int v in mass1)
            {
                Console.Write(v + ",");
            }
        }
        static bool Check_Set(ref List<int> mass1,ref  int ans)
        {
            if(ans < 0)
                return false;
            else if(ans > 0)
                return true;
            else //ans == 0
            {
                var mass_temp = new List<int>(mass);
                foreach(int i in mass1)
                {
                    for(int k = 0; k < mass_temp.Count; k ++)
                    {
                        if (i == mass_temp[k]) { mass_temp.RemoveAt(k); break; }
                    }
                }
                bool check = true;
                lock (locker)
                {
                    foreach (List<int> v in fin)
                    {
                        if ((mass1.SequenceEqual(v)) || (mass_temp.SequenceEqual(v))){ check = false; }
                    }
                    if (check)
                    {
                        fin.Add(mass1);
                        fin.Add(mass_temp);
                        Print_Mass(ref mass1); Console.Write(" = "); Print_Mass(ref mass_temp); Console.WriteLine();
                    }

                }
                return false;
            }
        }
        static void Sets_Maker(ref List<int> mass1,ref int ans)
        {
            for(int i = 0; i < mass1.Count; i++)
            {
                int ans_new = ans;
                var mass_new = new List<int>(mass1);
                ans_new -= mass_new[i];
                mass_new.RemoveAt(i);
                if (Check_Set(ref mass_new, ref ans_new)) { Sets_Maker(ref mass_new, ref ans_new); }
            }
        }

        static void Sets_Maker_Parallel(List<int> mass1, int ans)
        {
            Parallel.For(0, mass1.Count, i =>
            {
                var ans_new = ans;
                var mass_new = new List<int>(mass1);
                ans_new -= mass_new[i];
                mass_new.RemoveAt(i);
                if (Check_Set(ref mass_new,ref ans_new)) { Sets_Maker(ref mass_new,ref ans_new); }
            });
        }
        static void Main()
        {
            var mass1 = new List<int>(mass);
            int checksol = 0;
            foreach (int i in mass1) {checksol+=i;}
            if ( (checksol % 2) == 1){ Console.WriteLine("No answer"); return; }
            checksol /= 2;

            Console.WriteLine("Straight");
            var sw = Stopwatch.StartNew(); //Запуск таймера
            sw.Start();
            Sets_Maker(ref mass1, ref checksol);
            sw.Stop();
            var temptime = sw.Elapsed;
            Console.WriteLine("Time Taken for Straight: " + temptime);

            fin.Clear();
            Console.WriteLine("Parallel");
            sw.Restart();
            Sets_Maker_Parallel(mass1,checksol);
            sw.Stop();
            Console.WriteLine("Time Taken for Parallel: " + sw.Elapsed);
            Console.WriteLine("Time Taken for Straight: " + temptime);
        }
    }
}
