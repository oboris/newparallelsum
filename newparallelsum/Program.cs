namespace newparallelsum
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ArrayWorker worker = new ArrayWorker();
            worker.ArrayInit();
            Console.WriteLine(worker.PartSumm(0, worker.Array.Length));
            Console.WriteLine(worker.ParallelSum(2));
        }
    }

    public class ArrayWorker
    {
        private readonly object synchronizer = new object();
        public const int dim = 1000000;
        public readonly int[] Array = new int[dim];

        public void ArrayInit()
        {
            for (int i = 0; i < dim; i++)
            {
                Array[i] = i;
            }
        }

        public long PartSumm(int startIndex, int endIndex)
        {
            long sum = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                sum += Array[i];
            }
            return sum;
        }


        long sum = 0;
        int threadCount = 0;

        public void SetSum(long sum)
        {
            lock (synchronizer)
            {
                this.sum += sum;
                threadCount++;
                Monitor.Pulse(synchronizer);
            }
        }

        public long ParallelSum(int numThread)
        {
            new Thread(new ThreadWorker(0, dim / 2, this).Run).Start();

            ThreadWorker worker = new ThreadWorker(dim / 2, dim, this);
            new Thread(worker.Run).Start();

            lock (synchronizer)
            {
                while (threadCount < numThread)
                {
                    Monitor.Wait(synchronizer);
                }
            }
            return sum;
        }
    }

    public class ThreadWorker
    {
        private readonly int StartIndex;
        private readonly int EndIndex;
        private readonly ArrayWorker arrayWorker;

        public ThreadWorker(int startIndex, int endIndex, ArrayWorker arrayWorker)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            this.arrayWorker = arrayWorker;
        }

        public void Run ()
        {
            long sum = arrayWorker.PartSumm(StartIndex, EndIndex);
            arrayWorker.SetSum(sum);
        }
    }
}
