using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    //class for data transmission to a stream
    class ThreadData
    {
        public int Start { get; set; }
        public int End { get; set; }
        public double[] Input { get; set; }
        public double[] Output { get; set; }
        public int K { get; set; } = 1;
        public bool Unbalanced { get; set; } = false;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("=======================welcome to the show=======================");

        //values int the task:
        //N = (10, 100, 1000, 100000)
        //M = (2, 3, 4, 5, 10)
        int[] N = { 10, 100, 1000, 100000 };
        int[] M = { 2, 3, 4, 5, 10 };
        //difficulty parameters
        int[] K = { 1, 10, 100 };

        //note:
        //basic
        ////investigation with increasing complexity of processing
        ///analysis of non‑uniform loading
        ///analysis of circular separation
        //Warm-up run (like in the task)
        Console.WriteLine("Warm-up run...");
        double[] warmupData = generating(1000);
        double[] warmupResult = new double[1000];
        ///////////////////////////////////////////////////////////////////////////
        //two variants like in text task
        DateTime dt1 = DateTime.Now;
        serial(warmupData, warmupResult);
        DateTime dt2 = DateTime.Now;
        TimeSpan ts = dt2 - dt1;
        Console.WriteLine("Warming up sequential: {0} ms", ts.TotalMilliseconds);
        ///////////////////////////////////////////////////////////////////////////
        Stopwatch sw = new Stopwatch();
        sw.Start();
        diapazone_of_process(warmupData, warmupResult, 2);
        sw.Stop();
        Console.WriteLine("Warming up parallel: {0} ms", sw.Elapsed.TotalMilliseconds);
        ///////////////////////////////////////////////////////////////////////////
        Console.WriteLine("Warming up done!!!\n");

        //Parallel.For
        Console.WriteLine("demonstration parallel.For:");
        double[] demoData = generating(100);
        double[] demoResult = new double[100];
        Parallel.For(0, demoData.Length, i =>
        {
            demoResult[i] = Math.Pow(demoData[i], 1.789);
        });
        Console.WriteLine("Parallel.For done!!!\n");
        ///////////////////////////////////////////////////////////////////////////
        Console.WriteLine("\n№1, basic efficiency multithreaded processing:");
        basic_analisis(N, M);
        Console.WriteLine("\n№2, investigation with increasing complexity of processing:");
        hard_analysis(N, M, K);
        Console.WriteLine("\n№3, analysis of non‑uniform loading:");
        analysis_load(N, M);
        Console.WriteLine("\n№4, analysis of circular separation:");
        circle_vs_diapazone(N, M);
        Console.WriteLine("\nSUCCESS!!!");
        ///////////////////////////////////////////////////////////////////////////
    }

    //method like in the text task -> (static void Run(object some_data))
    static void Run(object some_data)
    {
        ThreadData td = (ThreadData)some_data;
        if (td.Unbalanced)
        {
            //Uneven load
            for (int i = td.Start; i < td.End; i++)
            {
                td.Output[i] = 0;
                for (int j = 0; j < i; j++)
                {
                    td.Output[i] += Math.Pow(td.Input[i], 1.789);
                }
            }
        }
        else
        {
            //Basic processing
            for (int i = td.Start; i < td.End; i++)
            {
                td.Output[i] = 0;
                for (int j = 0; j < td.K; j++)
                {
                    td.Output[i] += Math.Pow(td.Input[i], 1.789);
                }
            }
        }
    }

    //№1 task, basic efficiency multithreaded processing
    static void basic_analisis(int[] n, int[] m)
    {
        Console.WriteLine("|n - lenght of array|\t|m - array|\t|second time(ms)|\t|par time(ms)|\tspeedup|");
        Console.WriteLine("-------------------------------------------------------------------------------");
        foreach (int N in n)
        {
            double[] data = generating(N);
            double[] second = new double[N];
            double[] par = new double[N];
            DateTime dt1 = DateTime.Now;
            serial(data, second);
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            double s_time = ts.TotalMilliseconds;
            foreach (int M in m)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                diapazone_of_process(data, par, M);
                sw.Stop();
                double p_time = sw.Elapsed.TotalMilliseconds;

                //Checking the correctness of the results
                if (!check(second, par))
                {
                    Console.WriteLine("ERROR!!!");
                    return;
                }
                double speedup = s_time / p_time;
                Console.WriteLine($"{N}\t{M}\t{s_time:F4}\t\t{p_time:F4}\t\t{speedup:F2}");
            }
            Console.WriteLine();
        }
    }

    //№2, investigation with increasing complexity of processing
    static void hard_analysis(int[] values_of_n, int[] values_of_m, int[] _values_of_k)
    {
        Console.WriteLine("|n - lenght of array|\t|m - array|\t|k|\t|second time(ms)|\t|par time(ms)|\tspeedup|");
        Console.WriteLine("------------------------------------------------------------------------------------");
        foreach (int N in values_of_n)
        {
            foreach (int K in _values_of_k)
            {
                double[] data = generating(N);
                double[] result_seq = new double[N];
                double[] result_par = new double[N];
                //sequential processing with increasing complexity LIEK IN THE TASK
                Stopwatch sw = new Stopwatch();
                sw.Start();
                hard_process_serial(data, result_seq, K);
                sw.Stop();
                double seq_t = sw.Elapsed.TotalMilliseconds;
                foreach (int M in values_of_m)
                {
                    //multithreaded processing with increasing complexity
                    sw.Restart();
                    parallel_complex(data, result_par, M, K);
                    sw.Stop();
                    double par_t = sw.Elapsed.TotalMilliseconds;
                    if (!check(result_seq, result_par))
                    {
                        Console.WriteLine("ERROR!!!");
                        return;
                    }
                    double speedup = seq_t / par_t;
                    Console.WriteLine($"{N}\t{M}\t{K}\t{seq_t:F4}\t\t{par_t:F4}\t\t{speedup:F2}");
                }
                Console.WriteLine();
            }
        }
    }

    //№3, analysis of non‑uniform loading
    static void analysis_load(int[] values_n, int[] values_m)
    {
        Console.WriteLine("N\tM\tseq time(ms)\tRange time(ms)\tRR time(ms)\tspeedup RR");
        Console.WriteLine("----------------------------------------------------------------");
        foreach (int N in values_n)
        {
            double[] data = generating(N);
            double[] result_seq = new double[N];
            double[] result_range = new double[N];
            double[] result_rr = new double[N];
            DateTime dt1 = DateTime.Now;
            unbalanced_process(data, result_seq);
            DateTime dt2 = DateTime.Now;
            double seq_tt = (dt2 - dt1).TotalMilliseconds;

            foreach (int M in values_m)
            {
                //Range separation with uneven load
                Stopwatch sw = Stopwatch.StartNew();
                unbalanced_parallel_process(data, result_range, M);
                sw.Stop();
                double range_ = sw.Elapsed.TotalMilliseconds;
                //Circular separation with uneven load
                sw.Restart();
                unbalanced_parallel(data, result_rr, M);
                sw.Stop();
                double rrTime = sw.Elapsed.TotalMilliseconds;
                if (!check(result_seq, result_range) || !check(result_seq, result_rr))
                {
                    Console.WriteLine("ERROR!!!");
                    return;
                }
                double speedupRR = seq_tt / rrTime;
                Console.WriteLine($"{N}\t{M}\t{seq_tt:F4}\t{range_:F4}\t\t{rrTime:F4}\t\t{speedupRR:F2}");
            }
            Console.WriteLine();
        }
    }

    //№4, analysis of circular separation
    static void circle_vs_diapazone(int[] nn, int[] mm)
    {
        Console.WriteLine("N\tM\trange timr(ms)\tRR time(ms)\tRR efficiency");
        Console.WriteLine("-----------------------------------------------------");

        foreach (int N in nn)
        {
            double[] data = generating(N);
            double[] result_range = new double[N];
            double[] result_rr = new double[N];
            foreach (int M in mm)
            {
                //Range separation
                Stopwatch sw = Stopwatch.StartNew();
                diapazone_of_process(data, result_range, M);
                sw.Stop();
                double range_t = sw.Elapsed.TotalMilliseconds;
                //Circular separation
                sw.Restart();
                parallel(data, result_rr, M);
                sw.Stop();
                double rrtime = sw.Elapsed.TotalMilliseconds;
                if (!check(result_range, result_rr))
                {
                    Console.WriteLine("ERROR!!!");
                    return;
                }
                double eff = range_t / rrtime;
                Console.WriteLine($"{N}\t{M}\t{range_t:F4}\t\t{rrtime:F4}\t\t{eff:F2}");
            }
            Console.WriteLine();
        }
    }
    //checking the correctness of the results
    static bool check(double[] a, double[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (Math.Abs(a[i] - b[i]) > 1e-9)
                return false;
        }
        return true;
    }
    //generating test data
    static double[] generating(int N)
    {
        double[] data = new double[N];
        Random rand = new Random();
        for (int i = 0; i < N; i++)
        {
            data[i] = rand.NextDouble() * 100;
        }
        return data;
    }
    //sequential processing
    static void serial(double[] input, double[] output)
    {
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = Math.Pow(input[i], 1.789);
        }
    }
    //multithreaded processing with range separation EXACTLY as in the manual
    static void diapazone_of_process(double[] input, double[] output, int threadCount)
    {
        int chunkSize = input.Length / threadCount;
        Thread[] threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) 
                ? input.Length 
                : start + chunkSize;
            ThreadData td = new ThreadData
            {
                Start = start,
                End = end,
                Input = input,
                Output = output,
                K = 1
            };

            //LIKE IN THE TASK: new Thread(Run) и Start(object)
            threads[t] = new Thread(Run);
            threads[t].Start(td);
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
    //sequential processing with increasing complexity
    static void hard_process_serial(double[] input, double[] output, int K)
    {
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = 0;
            for (int j = 0; j < K; j++)
            {
                output[i] += Math.Pow(input[i], 1.789);
            }
        }
    }
    //multithreaded processing with increasing complexity
    static void parallel_complex(double[] input, double[] output, int threadCount, int K)
    {
        int chunkSize = input.Length / threadCount;
        Thread[] threads = new Thread[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) 
                ? input.Length 
                : start + chunkSize;
            ThreadData td = new ThreadData
            {
                Start = start,
                End = end,
                Input = input,
                Output = output,
                K = K
            };
            threads[t] = new Thread(Run);
            threads[t].Start(td);
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
    //Sequential processing with uneven load LIKE IN TEXT TASK
    static void unbalanced_process(double[] input, double[] output)
    {
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = 0;
            for (int j = 0; j < i; j++)
            {
                output[i] += Math.Pow(input[i], 1.789);
            }
        }
    }
    //multithreaded processing with uneven load (range)
    static void unbalanced_parallel_process(double[] input, double[] output, int threadCount)
    {
        int chunkSize = input.Length / threadCount;
        Thread[] threads = new Thread[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) 
                ? input.Length 
                : start + chunkSize;
            ThreadData td = new ThreadData
            {
                Start = start,
                End = end,
                Input = input,
                Output = output,
                Unbalanced = true
            };
            threads[t] = new Thread(Run);
            threads[t].Start(td);
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
    //multithreaded processing with circular partitioning
    //          Process Parallel Round Robin
    static void parallel(double[] input, double[] output, int threadCount)
    {
        Thread[] threads = new Thread[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            int threadIndex = t;
            threads[t] = new Thread(() =>
            {
                for (int i = threadIndex; i < input.Length; i += threadCount)
                {
                    output[i] = Math.Pow(input[i], 1.789);
                }
            });
            threads[t].Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    //multithreaded processing with circular partitioning and uneven load
    static void unbalanced_parallel(double[] input, double[] output, int threadCount)
    {
        Thread[] threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int threadIndex = t;
            threads[t] = new Thread(() =>
            {
                for (int i = threadIndex; i < input.Length; i += threadCount)
                {
                    output[i] = 0;
                    for (int j = 0; j < i; j++)
                    {
                        output[i] += Math.Pow(input[i], 1.789);
                    }
                }
            });
            threads[t].Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    //Realization Parallel.For
    static void ProcessParallelFor(double[] input, double[] output)
    {
        Parallel.For(0, input.Length, i =>
        {
            output[i] = Math.Pow(input[i], 1.789);
        });
    }
}