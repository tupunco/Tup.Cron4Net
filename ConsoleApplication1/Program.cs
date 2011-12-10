using System;
using System.Collections.Generic;
using System.Text;
using Tup.Cron4Net;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // var s1 = new SchedulingPattern("* * * * *");
            var s = new Scheduler();
            s.schedule("* * * * *", new Action<TaskExecutionContext>(x => Console.WriteLine(DateTime.Now)));
            s.schedule("*/2 * * * *", new Action<TaskExecutionContext>(x => Console.WriteLine("2-{0}",DateTime.Now)));
            s.schedule("*/3 * * * *", new Action<TaskExecutionContext>(x => Console.WriteLine("3-{0}", DateTime.Now)));
            s.start();
            //var p1 = new Predictor(s1);
            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine("{0:yyyy-MM-dd HH:mm}", p1.NextMatchingDate());
            //}
            Console.ReadLine();
        }
    }
}
