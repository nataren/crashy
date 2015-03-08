using System;
using System.Threading;
using MindTouch.Collections;
using MindTouch.Dream;
using MindTouch.Xml;

namespace Crashy {
    class Producer {
        readonly Shard[] shards;
        readonly TimeSpan sleep_ms;
        readonly int numberOfShards;
        static XDoc doc;
        readonly Random random;
        
        public Producer(Shard[] shards, double sleep_ms) {
            this.shards = shards;
            numberOfShards = shards.Length;
            random = new Random((int) DateTime.UtcNow.Ticks);
            this.sleep_ms = TimeSpan.FromMilliseconds(sleep_ms);
            doc = XDocFactory.From(
                @"<event id=""id""
                    datetime=""2011-12-22T17:17:21Z"" type=""workflow:next.step""
                    wikiid=""site_0"" journaled=""false"" version=""2"">
                    <request id=""id"" seq=""1"" count=""1"">
                        <signature>POST:workflow/*</signature>
                        <ip>127.0.0.1</ip>
                        <session-id>id</session-id>
                        <parameters />
                        <user id=""2"" anonymous=""true"" />
                    </request>
                    <workflow name=""submit-issue"">
                        <uri.next>http://myuri.com</uri.next>
                        <data>
                            <_userid>2</_userid>
                            <_username>Anonymous</_username>
                            <_customeractivityid>id</_customeractivityid>
                            <_requestid>id</_requestid>
                            <_email>foo@hotmail.com</_email>
                            <_search></_search>
                            <_path>/</_path>
                            <firstname>Firstname</firstname>
                            <phone1>111</phone1>
                            <phone2>222</phone2>
                            <phone3>333</phone3>
                            <lastname>Lastname</lastname>
                            <model>1</model>
                            <subject>My subject</subject>
                            <serial>1</serial>
                            <description>My description</description>
                            <brand>My brand</brand>
                        </data>
                </workflow>
            </event>", MimeType.XML);
        }

        public void Run() {
            while(true) {
                if(random.NextDouble() <= 0.5) {
                    Thread.Sleep(sleep_ms);
                } else {
                    shards[random.Next(numberOfShards)].TryEnqueue(doc.ToString());
                }
            }
        }
    }

    class Shard {
        static void Project(string item) {
        }
        ProcessingQueue<string> queue;
        object sync = new object();

        public Shard() {
            queue = new ProcessingQueue<string>(Project, 1);
        }

        public void TryEnqueue(string s) {
            lock(sync) {
                queue.TryEnqueue(s);
            }
        }
    }

    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Usage: mono Crashy.exe {NUMBER_OF_PRODUCERS} {NUMBER_OF_SHARDS} {SLEEP_MS}\n");

            // Config
            var numberOfProducers = int.Parse(args[0]);
            var numberOfShards = int.Parse(args[1]);
            var sleep = double.Parse(args[2]);
            Console.WriteLine("Will use {0} data producers, {1} shared shard(s), and will randomly sleep {2}ms", numberOfProducers, numberOfShards, sleep);
                        
            // Build shards
            var shards = new Shard[numberOfShards];
            for(var j = 0; j < numberOfShards; j++) {
                shards[j] = new Shard();
            }

            // Build producers
            for(var i = 0; i < numberOfProducers; i++) {
                var producer = new Producer(shards, sleep);
                var thread = new Thread(producer.Run);
                thread.Start();
            }
            Thread.CurrentThread.Join();
        }
    }
}
