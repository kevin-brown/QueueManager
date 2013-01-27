namespace QueueManager
{
    /// <summary>
    /// The different priorities that a Rule can have
    /// </summary>
    public enum RulePriority
    {
        /// <summary>
        /// Processed with high priority (first pass)
        /// </summary>
        High,

        /// <summary>
        /// Processed with medium priority (second pass)
        /// </summary>
        Medium,

        /// <summary>
        /// Processed with low priority (last pass)
        /// </summary>
        Low,
    }
}
