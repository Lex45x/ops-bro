#Prometheus Metrics

This file contains description of the existing metrics that could be retrieved from the applicaiton.

## Default metrics

Default metrics defined in [prometheus-net](https://github.com/prometheus-net/prometheus-net) package are included.

## Additional metrics

There are few additional metrics that will help you to understand health of the OpsBro instance.  

### events_dispatched

`events_dispatched` represents amount of evens that has been dispatched since application start.  
Expected: counter is grow with time.  
Labels:  
* `event_name` contains the name of event that has been dispatched.  

### listener_calls

`listener_calls` contains amount of calls each listener received.  
Expected: counter is grow with time and equals to `http_requests_received_total{controller="Listener",action="Call"}`.  
Labels:  
* `listener_name` contains name of listener that has been called.

### listener_events_extracted
`listener_events_extracted` contains amount of events has been extracted so far via specific listener. 
Expected: grow with time and greater or equals to `listener_calls` per specific listener.  
Labels:  
* `listener_name` contains name of listener that extracts event.
* `event_name` contains name of event that has been extracted.

### events_without_dispatcher
`events_without_dispatcher` contains amount of events has been extracted so far via specific listener but not event dispatcher has been found.  
Expected: to be zero for the whole application lifetime. Values greater then zero is a misconfiguration signal.    
Labels:  
* `listener_name` contains name of listener that extracts event.
* `event_name` contains name of event that has been extracted.

### failed_event_subscription
`failed_event_subscription` contains amount of failed calls to EventSubscriber.  
Expected: to be zero for the whole application lifetime. Values greater then zero may be misconfiguration signal as well as target service failure.    
Labels:  
* `event_name` contains name of event that has been extracted.


