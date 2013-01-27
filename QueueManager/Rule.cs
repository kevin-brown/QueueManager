using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    /// <summary>
    /// The base class for rules which determine the priority of each Queue.
    /// </summary>
    abstract class Rule
    {
        /// <summary>
        /// The priority that is associated with the rule.  Determines the order in which rules are executed.
        /// </summary>
        public RulePriority Priority;

        /// <summary>
        /// The value that will be added to the Queue if it the rule passes.
        /// </summary>
        public int Value = 0;

        /// <summary>
        /// The name of the queue that the Value will be added to.
        /// </summary>
        public string QueueName;

        /// <summary>
        /// Determines if the Value will be applied to the queue.
        /// </summary>
        /// <returns>True if the Value will be applied, False otherwise.</returns>
        abstract public bool IsApplicable()
        {
            return false;
        }
    }
}
