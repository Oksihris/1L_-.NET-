using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace Vector_1L
{
    public class Vector
    {
        protected int[] elements;

        public Vector(int size)
        {
            if (size <= 0) throw new ArgumentException("Size must be greater than zero.");
            elements = new int[size];
            Random rand = new Random();

            for (int i = 0; i < size; i++)
            {
                elements[i] = rand.Next(100);
            }
        }

        public void Print()
        {
            for (int i = 0; i < Math.Min(10, elements.Length); i++)
            {
                Console.Write(elements[i] + " ");
            }
            Console.WriteLine();
        }

        public int[] GetElements() => elements;

        public void ShiftRight(int shiftAmount)
        {
            int size = elements.Length;
            if (size == 0 || shiftAmount == 0 || Math.Abs(shiftAmount) % size == 0) return;

            if (shiftAmount < 0)
            {
                shiftAmount = size + (shiftAmount % size);
            }
            else
            {
                shiftAmount %= size;
            }

            int[] temp = new int[size];
            for (int i = 0; i < size; i++)
            {
                int newIndex = (i + shiftAmount) % size;
                temp[newIndex] = elements[i];
            }

            Array.Copy(temp, elements, size);
        }
    }

    public class ParallelVector : Vector
    {
        public ParallelVector(int size) : base(size) { }

        public void ShiftRightParallel(int shiftAmount, int numThreads)
        {
            int size = elements.Length;
            if (size == 0 || shiftAmount == 0 || Math.Abs(shiftAmount) % size == 0) return;

            if (shiftAmount < 0)
            {
                shiftAmount = size + (shiftAmount % size);
            }
            else
            {
                shiftAmount %= size;
            }

            int[] temp = new int[size];

            Parallel.For(0, numThreads, threadIndex =>
            {
                int segmentSize = size / numThreads;
                int start = threadIndex * segmentSize;
                int end = (threadIndex == numThreads - 1) ? size : start + segmentSize;

                for (int i = start; i < end; i++)
                {
                    int newIndex = (i + shiftAmount) % size;
                    temp[newIndex] = elements[i];
                }
            });

            Array.Copy(temp, elements, size);
        }

        private void ShiftPart(int start, int end, int shiftAmount, int size, int[] temp)
        {
            for (int i = start; i < end; i++)
            {
                int newIndex = (i + shiftAmount) % size;
                temp[newIndex] = elements[i];
            }
        }
    }

    class Program
    {
        static double CalculateSpeedup(long singleThreadTime, long multiThreadTime)
        {
            return (double)singleThreadTime / multiThreadTime;
        }

        static void Main(string[] args)
        {
            Console.Write("Enter vector size: ");
            int size = int.Parse(Console.ReadLine() ?? "30");
            Console.Write("Enter shift amount: ");
            int shiftAmount = int.Parse(Console.ReadLine() ?? "20");
            Console.Write("Enter number of threads: ");
            int numThreads = int.Parse(Console.ReadLine() ?? "2");

            ParallelVector vector = new ParallelVector(size);
            Console.WriteLine("First 10 elements of the initial vector:");
            vector.Print();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            vector.ShiftRight(shiftAmount);
            stopwatch.Stop();
            long singleThreadTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("\nFirst 10 elements after single-threaded shift:");
            vector.Print();
            Console.WriteLine($"Single-threaded time: {singleThreadTime} ms");

            vector = new ParallelVector(size); 
            stopwatch.Restart();
            vector.ShiftRightParallel(shiftAmount, numThreads);
            stopwatch.Stop();
            long multiThreadTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("\nFirst 10 elements after multi-threaded shift:");
            vector.Print();
            Console.WriteLine($"Multi-threaded time: {multiThreadTime} ms");

            double speedup = CalculateSpeedup(singleThreadTime, multiThreadTime);
            Console.WriteLine($"\nSpeedup: {speedup:F2}x");

            Console.WriteLine("\nPress any key to finish...");
            Console.ReadKey();
        }
    }
}