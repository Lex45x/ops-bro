#Prometheus Metrics

This file contains description of the existing metrics that could be retrieved from the applicaiton.

## Default metrics

Default metrics defined in [prometheus-net](https://github.com/prometheus-net/prometheus-net) package are included.

## Additional metrics

There are few additional metrics that will help you to understand health of the OpsBro instance.  

### events_dispatched

`events_dispatched` represents amount of evens that has been dispatched since application start.  
Labels:  
* `event_name` contains the name of event that has been dispatched.  

### listener_calls

`listener_calls` contains amount of calls each listener received.  
Labels:  
* `listener_name` contains name of listener that has been called.

### listener_events_extracted
`listener_events_extracted` contains amount of events has been extracted so far via specific listener.  
Labels:  
* `listener_name` contains name of listener that extracts event.
* `event_name` contains name of event that has been extracted.