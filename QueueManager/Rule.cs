using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueManager
{
    /// <summary>
    /// The class for rules which determine the priority of each Queue.
    /// </summary>
    class Rule
    {
        /// <summary>
        /// The priority that is associated with the rule.  Determines the order in which rules are executed.
        /// </summary>
        public RulePriority Priority = RulePriority.High;

        /// <summary>
        /// The value that will be added to the Queue if it the rule passes.
        /// </summary>
        public int Value = 0;

        /// <summary>
        /// The name of the queue that the Value will be added to.
        /// </summary>
        public string QueueName = null;

        /// <summary>
        /// Constructor for the Rule.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="value">The value that will be added to the queue.</param>
        /// <param name="priority">The priority that the rule will have.</param>
        /// <param name="isApplicable">The function that will be run to determine if the value will be added.</param>
        public Rule(string queueName, int value, RulePriority priority, Func<Dictionary<string, Queue<object>>, bool> isApplicable)
        {
            QueueName = queueName;
            Value = value;
            Priority = priority;

            IsApplicable = isApplicable;
        }

        /// <summary>
        /// Determines if the Value will be applied to the queue.
        /// </summary>
        /// <param name="queues">The dictionary of queues that can be used in the rule.</param>
        /// <returns>True if the Value will be applied, False otherwise.</returns>
        public Func<Dictionary<string, Queue<object>>, bool> IsApplicable = (queues) => { throw new NotImplementedException(); };
    }
}
