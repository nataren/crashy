using System;
using System.Threading;
using MindTouch.Collections;

namespace Crashy {
    class Processor {
        Shard[] shards;
        Random random;
        int numberOfShards;
        int threadNumber;

        public Processor(int numberOfShards, int threadNumber) {
            this.numberOfShards = numberOfShards;
            this.threadNumber = threadNumber;
            random = new Random((int)DateTime.UtcNow.Ticks);
            shards = new Shard[numberOfShards];
            for(var i = 0; i < numberOfShards; i++) {
                shards[i] = new Shard();
            }
        }

        public void Run() {
            Console.WriteLine("Thread {0} runs", threadNumber);
            while(true) {
                shards[random.Next(numberOfShards)].TryEnqueue(random.NextDouble().ToString());
            }
        }
    }

    class Shard {
        static void Project(string item) { }
        ProcessingQueue<string> queue;

        public Shard() {
            queue = new ProcessingQueue<string>(Project, 1);
        }

        public void TryEnqueue(string s) {
            queue.TryEnqueue(s);
        }
    }

    class Program {
        static void Main(string[] args) {
            var numberOfProcessors = int.Parse(args[0]);
            var numberOfShards = int.Parse(args[1]);
            var processors = new Processor[numberOfProcessors];
            for(var j = 0; j < numberOfProcessors; j++) {
                processors[j] = new Processor(numberOfShards, j);
                var thread = new Thread(processors[j].Run);
                thread.Start();
            }
            Thread.CurrentThread.Join();
        }
    }
}
