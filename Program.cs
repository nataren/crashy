using System;
using MindTouch.Collections;

namespace Crashy {
    class Program {
        static void Project(string item) { }
        static void Main() {
            var queue = new ProcessingQueue<string>(Project, 1);     
            while(true) {
                queue.TryEnqueue(new Guid().ToString());
            }
        }
    }
}
