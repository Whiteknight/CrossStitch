## Timer Module

The Timer module is an internal timer for the Core. It publishes timer tick messages at 10 second intervals, which modules can subscribe to for a variety of scheduled tasks.

The Timer module only sends out simple messages on an interval. It does not handle more complicated cron-like scheduling tasks by itself. 

