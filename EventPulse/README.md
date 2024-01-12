# EventPulse

EventPulse provides a thin layer between your application and the event persistence
for Event Sourcing systems. It's designed to be less intrusive as possible and
avoid totally reflection (for sure we use Json serialization though), which means
the real methods are used to retrieve the aggregations based in the events stored
previously.

For more information go to the project's repository: https://github.com/hesenger/EventPulse
