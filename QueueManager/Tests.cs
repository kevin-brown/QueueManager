using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueManager
{
    class Tests
    {
        static void Main(string[] args)
        {
            QueueManager manager = new QueueManager();

            // Initialize the queue manager with test data

            manager.AddRule(new Rule("test", 1, RulePriority.High, (queues) => { return true; }));
            manager.AddRule(new Rule("test2", 1, RulePriority.High, (queues) => { if (queues["test2"].Count > 1) { return true; } return false; }));

            // Add some test queues

            manager.AddQueue("test2", TestCallback, 1000);
            manager.AddQueue("test", TestCallback, 1000);

            // Add some test items

            manager.AddItem("test", "test item 1");
            manager.AddItem("test2", "test2 item 1");
            manager.AddItem("test", "test item 2");
            manager.AddItem("test2", "test2 item 2");

            // Test to make sure the callback works

            manager.Continue();
            manager.Continue();
            manager.Continue();
            manager.Continue();
            manager.Continue();

            Console.ReadLine();
        }

        static void TestCallback(string key, object item, Action<string> callback)
        {
            Console.WriteLine(item.ToString());

            if (callback != null)
            {
                callback(key);
            }
        }
    }
}
