# QueueManager

QueueManager allows you to have one central queue with multiple sub-queues that are managed using rules.

## Why use a QueueManager?

QueueManager should be used in situations which require a background queue to handle different tasks.  It allows different queues to be prioritized based on rules, giving you enough flexibility to make sure that the correct queue is called when it is needed.

Only one item from all queues will be processed at a time, which allows you to give certain items a higher priority.  Each queue must have a timeout, to prevent queue deadlock.

## Adding Queues

New queues can be added by calling AddQueue on the QueueManager instance.  You must provide a name, callback, and timeout for the new queue.

The callback will be used when the queue is called.  The next item and the finish callback will be provided as parameters.

## Adding Rules

New rules can be added by calling AddRule on the QueueManager instance.  You just need to pass in the rule, and the QueueManager will call it every time it is needed.