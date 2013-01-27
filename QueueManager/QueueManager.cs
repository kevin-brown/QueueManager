using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    class QueueManager
    {
        Dictionary<string, Queue<object>> Queues = new Dictionary<string, Queue<object>>();
        Dictionary<string, Action<object, Action>> Callbacks = new Dictionary<string, Action<object, Action>>();
        Dictionary<string, int> Timeouts = new Dictionary<string, int>();

        /// <summary>
        /// A Dictionary that holds the lists of rules for each priority
        /// </summary>
        Dictionary<RulePriority, List<Rule>> Rules = new Dictionary<RulePriority, List<Rule>>();

        /// <summary>
        /// True if the Queue is waiting for a finish callback, False if not
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// Adds a new Queue to the dictionary that is initialized to accept anything.
        /// </summary>
        /// <param name="name">The name of the Queue</param>
        /// <param name="callback">The callback that will be called when the queue is executed</param>
        /// <param name="timeout">The amount of time the QueueManager will wait before it continues</param>
        public void AddQueue(string name, Action<object, Action> callback, int timeout)
        {
            Queues.Add(name, new Queue<object>());
            Callbacks.Add(name, callback);
            Timeouts.Add(name, timeout);
        }

        /// <summary>
        /// Adds a rule which is used to determine the order of Queues
        /// </summary>
        /// <param name="rule">The new rule</param>
        public void AddRule(Rule rule)
        {
            Rules[rule.Priority].Add(rule);
        }

        /// <summary>
        /// If the queue is not waiting on a callback, executes the next available queue item
        /// </summary>
        public void Continue()
        {
            if (!IsRunning)
            {

            }
        }

        /// <summary>
        /// If the Queue is currently waiting on a callback, force the Queue to restart
        /// </summary>
        public void Restart()
        {
            if (IsRunning == true)
            {
                IsRunning = false;
            }
        }
    }
}
