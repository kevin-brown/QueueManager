using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    class Tests
    {
        class TestRule : Rule
        {
            public TestRule()
            {
                Priority = RulePriority.High;
                Value = 1;
                QueueName = "test";
            }

            public override bool IsApplicable(Dictionary<string, Queue<object>> queues)
            {
                return true;
            }
        }

        static void Main(string[] args)
        {
            QueueManager manager = new QueueManager();

            // Initialize the queue manager with test data

            manager.AddRule(new TestRule());
            manager.AddQueue("test", TestCallback, 1000);
            manager.AddItem("test", "test item");

            // Test to make sure the callback works

            manager.Continue();

            Console.ReadLine();
        }

        static void TestCallback(object item, Action callback)
        {
            Console.WriteLine(item.ToString());

            if (callback != null)
            {
                callback();
            }
        }
    }
}
