﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace QueueManager
{
    /// <summary>
    /// Main class used to manage queues.
    /// </summary>
    public class QueueManager
    {
        /// <summary>
        /// A Dictionary that holds the actual queues which will be managed.
        /// </summary>
        Dictionary<string, Queue<object>> Queues = new Dictionary<string, Queue<object>>();

        /// <summary>
        /// A Dictionary that holds the callbacks that will be executed for each queue.
        /// </summary>
        Dictionary<string, Action<string, object, Action<string>>> Callbacks = new Dictionary<string, Action<string, object, Action<string>>>();

        /// <summary>
        /// A Dictionary that holds the timeouts for each queue.
        /// </summary>
        Dictionary<string, int> Timeouts = new Dictionary<string, int>();

        /// <summary>
        /// A Dictionary that holds the lists of rules for each priority.
        /// </summary>
        Dictionary<RulePriority, List<Rule>> Rules = new Dictionary<RulePriority, List<Rule>>();

        /// <summary>
        /// True if the Queue is waiting for a finish callback, False if not.
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// The timer that will handle the timeout for the queue lock.
        /// </summary>
        private Timer TimeoutTimer = new Timer();

        /// <summary>
        /// String used to determine if the callback can unlock the queue.
        /// </summary>
        private string TimeoutKey = "";

        /// <summary>
        /// Event fired when all queues are empty.  It is only fired once and is reset when an item is added to a queue.
        /// </summary>
        public event EventHandler QueuesEmpty;

        /// <summary>
        /// Event fired when the working queue is restarted.  It can be fired one time for each object in the queue.
        /// </summary>
        public event EventHandler Restarted;

        /// <summary>
        /// Flag to determine whether or not the QueuesEmpty event can be fired again.
        /// </summary>
        private bool QueuesEmptyFired = false;

        /// <summary>
        /// Flag to determine whether or not the Restarted event can be fired again.
        /// </summary>
        private bool RestartedFired = false;

        /// <summary>
        /// Initializes the QueueManager with three types of priorities and a stopped timeout timer.
        /// </summary>
        public QueueManager()
        {
            // Initialize the rules dictionary

            Rules[RulePriority.High] = new List<Rule>();
            Rules[RulePriority.Medium] = new List<Rule>();
            Rules[RulePriority.Low] = new List<Rule>();

            TimeoutTimer.Stop();
            TimeoutTimer.Elapsed += TimeoutTimer_Elapsed;
        }

        /// <summary>
        /// Adds the specified item to the specified queue.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <param name="item">The item to add to the queue.</param>
        public void AddItem(string name, object item)
        {
            // Add the item to the specified queue

            Queues[name].Enqueue(item);

            // Reset the QueuesEmpty event

            QueuesEmptyFired = false;
        }

        /// <summary>
        /// Adds a new Queue to the dictionary that is initialized to accept anything.
        /// </summary>
        /// <param name="name">The name of the Queue.</param>
        /// <param name="callback">The callback that will be called when the queue is executed.</param>
        /// <param name="timeout">The amount of time the QueueManager will wait before it continues.</param>
        public void AddQueue(string name, Action<string, object, Action<string>> callback, int timeout)
        {
            // Add the necessary data to the dictionaries

            Queues.Add(name, new Queue<object>());
            Callbacks.Add(name, callback);
            Timeouts.Add(name, timeout);
        }

        /// <summary>
        /// Adds a rule which is used to determine the order of Queues.
        /// </summary>
        /// <param name="rule">The new rule.</param>
        public void AddRule(Rule rule)
        {
            // Add the rule to the specific list for its priority

            Rules[rule.Priority].Add(rule);
        }

        /// <summary>
        /// If the queue is not waiting on a callback, executes the next available queue item.
        /// </summary>
        public void Continue()
        {
            // Don't continue if the queue is already processing an item

            if (!IsRunning)
            {
                // Reset the restarted event

                RestartedFired = false;

                // Get the name of the next queue to be processed

                string queueName = DetermineNextQueue();

                // Don't continue if the queues are empty

                if (queueName == null)
                {
                    // Check if the QueuesEmpty event has been fired

                    if (!QueuesEmptyFired)
                    {
                        // Fire the QueuesEmpty event

                        EventHandler handler = QueuesEmpty;

                        if (handler != null)
                        {
                            handler(this, new EventArgs());
                        }

                        QueuesEmptyFired = true;
                    }

                    return;
                }

                Queue<object> queue = Queues[queueName];

                // Don't continue if the queue is empty

                if (queue.Count == 0)
                {
                    return;
                }

                // Lock the queue

                IsRunning = true;

                // Start time timeout timer for the specific queue

                StartTimeoutTimer(Timeouts[queueName]);

                // Invoke the callback for the specific queue

                Callbacks[queueName](TimeoutKey, queue.Dequeue(), EndCallback);
            }
        }

        /// <summary>
        /// Determines the next queue that will be called.
        /// </summary>
        /// <returns>
        /// The name of the next Queue that will be called.
        /// Returns null if all queues are empty.
        /// Returns the first queue if all rules result in a tie.
        /// </returns>
        private string DetermineNextQueue()
        {
            // Return "null" only if all queues are empty

            if (Empty())
            {
                return null;
            }

            // Check all priorities

            foreach (KeyValuePair<RulePriority, List<Rule>> priority in Rules)
            {
                // Query for the next queue on the priority

                string queueName = DetermineNextQueue(priority.Key);

                // If the query doesn't return null, it found a queue

                if (queueName != null)
                {
                    return queueName;
                }
            }

            // In the event of a tie, return the name of the first queue.

            return Queues.Keys.ElementAt(0);
        }

        /// <summary>
        /// Determines the next queue that will be called,
        /// </summary>
        /// <param name="priority">The priority that will be used to determine the rules.</param>
        /// <returns>Returns null if there is a rule tie.  Returns the queue name if possible.</returns>
        private string DetermineNextQueue(RulePriority priority)
        {
            Dictionary<string, int> priorities = new Dictionary<string, int>();

            // Initialize all queues with the same priority

            foreach (KeyValuePair<string, Queue<object>> pair in Queues)
            {
                priorities.Add(pair.Key, 0);
            }

            // Run through all high priority rules first

            foreach (Rule rule in Rules[priority])
            {
                if (rule.IsApplicable(Queues))
                {
                    // Add the value of the rule to the queue

                    priorities[rule.QueueName] += rule.Value;
                }
            }

            // Get the queue with the highest priority

            string highestQueue = null;

            foreach (KeyValuePair<string, int> queue in priorities)
            {
                // Skip the queue if it is empty

                if (Queues[queue.Key].Count > 0)
                {
                    // Set the first queue

                    if (highestQueue == null)
                    {
                        highestQueue = queue.Key;
                    }
                    else
                    {
                        // Check for a tie

                        if (queue.Value == priorities[highestQueue])
                        {
                            highestQueue = null;

                            break;
                        }

                        // Check if the current queue has a higher value than the previous highest

                        if (queue.Value > priorities[highestQueue])
                        {
                            highestQueue = queue.Key;
                        }
                    }
                }
            }

            // Check for a tie

            if (highestQueue == null)
            {
                return null;
            }
            else
            {
                return highestQueue;
            }
        }

        /// <summary>
        /// Determine if all queues are empty.
        /// </summary>
        /// <returns>True if all queues are empty, false otherwise.</returns>
        public bool Empty()
        {
            foreach (KeyValuePair<String, Queue<object>> queue in Queues)
            {
                if (queue.Value.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The callback that should be called when a queue is done processing its item.
        /// </summary>
        private void EndCallback(string key)
        {
            // Make sure the key matches the current timeout

            if (key == TimeoutKey)
            {
                // Tell the manager it is done processing

                IsRunning = false;

                // Stop the timeout timer

                StopTimeoutTimer();
            }
        }

        /// <summary>
        /// Generates a random 8 character string that can be used to unlock the queue.
        /// </summary>
        private void GenerateTimeoutKey()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[8];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            TimeoutKey = new String(stringChars);
        }

        /// <summary>
        /// If the Queue is currently waiting on a callback, force the Queue to restart.
        /// </summary>
        public void Restart()
        {
            // Tell the manager it is no longer processing the current item

            if (IsRunning == true)
            {
                // Manually call the end callback

                EndCallback(TimeoutKey);

                // Fire the restarted command

                EventHandler handler = Restarted;

                if (handler != null)
                {
                    handler(this, new EventArgs());
                }

                RestartedFired = true;
            }
        }

        /// <summary>
        /// Start the timeout timer.
        /// </summary>
        /// <param name="timeout">The time until the timeout.</param>
        private void StartTimeoutTimer(int timeout)
        {
            GenerateTimeoutKey();

            TimeoutTimer.Interval = timeout;
            TimeoutTimer.Start();
        }

        /// <summary>
        /// Stop the timeout timer.
        /// </summary>
        private void StopTimeoutTimer()
        {
            TimeoutTimer.Stop();
        }

        /// <summary>
        /// The callback for the timeout timer, which will manually call the end callback and remove the lock.
        /// </summary>
        void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Manually call the end callback

            Restart();
        }
    }
}
