using System;
using System.Threading;
using MindTouch.Collections;

namespace Crashy {
    class Producer {
        Shard[] shards;
        TimeSpan sleep_ms;
        public Producer(Shard[] shards, double sleep_ms) {
            this.shards = shards;
            this.sleep_ms = TimeSpan.FromMilliseconds(sleep_ms);
        }

        public void Run() {
            var random = new Random((int) DateTime.UtcNow.Ticks);
            var numberOfProcessors = shards.Length;
            while(true) {
                if(random.NextDouble() < 0.5) {
                    Thread.Sleep(sleep_ms);
                }
                shards[random.Next(numberOfProcessors)].TryEnqueue(Guid.NewGuid().ToString());
            }
        }
    }

    class Shard {
        static void Project(string item) {
        }
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
            Console.WriteLine("Usage: mono.exe Crashy.exe {NUMBER_OF_PRODUCERS} {NUMBER_OF_SHARDS} {SLEEP_MS}");
            var numberOfProducers = int.Parse(args[0]);
            var numberOfShards = int.Parse(args[1]);
            var sleep = double.Parse(args[2]);
            Console.WriteLine("Will use {0} data producers", numberOfProducers);
            Console.WriteLine("Will use {0} shards per producer", numberOfShards);
            Console.WriteLine("Will randomly sleep {0} ms", sleep);
            for(var i = 0; i < numberOfProducers; i++) {
                var shards = new Shard[numberOfShards];
                for(var j = 0; j < numberOfShards; j++) {
                    shards[j] = new Shard();
                }
                var producer = new Producer(shards, sleep);
                var thread = new Thread(producer.Run);
                thread.Start();
            }
            Thread.CurrentThread.Join();
        }
    }
}
