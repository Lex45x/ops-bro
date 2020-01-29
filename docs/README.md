# Documentation Index

- [Documentation Index](#documentation-index)
- [Get Started with Docker Image](#get-started-with-docker-image)
- [Concept](#concept)
- [Understanding the Configuration](#understanding-the-configuration)
  - [Types](#types)
    - [Event](#event)
    - [Request](#request)
  - [Listener](#listener)
  - [Extractor](#extractor)
    - [Validation Rule](#validation-rule)
    - [Extraction Rule](#extraction-rule)
  - [Event Dispatcher](#event-dispatcher)
  - [Event Subscriber](#event-subscriber)
    - [Template Rule](#template-rule)
      - [Url](#url)
      - [Body](#body)
      - [Header](#header)
    - [Metadata](#metadata)

# Get Started with Docker Image

//here goes link to docker hub and explanations with configuration passing

# Concept

ops-bro allows to connect services that have WebHooks to service that have REST API.

1. Service make a call to a specific ops-bro [Listener](#listener).
2. [Listener](#listener) extract Events from the request using a list of [Extractors](#extractor).
3. Event then sent to [Event Dispathcer](#event-dispatcher). 
4. Each [Event Subscriber](#event-subscriber) converts event to HTTP request message and sends it to related service.

See generic flow on the image below.

![Concept Flow](concept&#32;flow.jpg)

# Understanding the Configuration

Application could be configured with only one way: via JSON configuration file.
Basically, this file represent a JSON object that can be represented via the next example.
```json
{
    "listeners":[],
    "eventDispatchers":[]
}
```
Where the `listeners` is a collection of Listener and `eventDispatchers` is a collection of Event Dispatcher

## Types
Types are not defined in the configuration, but are implicitly used to understand the logic under configuration. A list of types are presented as sub-header entries.

### Event
Event has name and Data object adn represent an message about system state change.

### Request
Inside the request object there are three properties:
* `query` - contains all query parameters
* `body` - contains actual request body
* `headers` - contains request headers


## Listener

Each listener represents a source of incoming HTTP requests.  
E.g.: You want to build one-way integration between Gitlab and Jira. So, in this case you will be **listen** to Gitlab events an then change state of the Jira issues accordingly. As a result you will create a single Listener, called gitlab.  
See an example below.
```json
{
    "listeners":[
        {
            "name":"gitlab",
            "extractors":[]            
        }
    ],
    "eventDispatchers":[]
}
```
`extractors` is a list of [Extractor](#extractor) that is describe below.

## Extractor

Each extractor represent a single event that could be extracted from the request to Listener.  
E.g.: Gitlab listener could provide a bunch of different event to webhook subscriber: commit pushed, merge request created, merge request taken back in progress, merge request completed, merge request merged, etc. 
So, in this case, each of the listed event will have own extractor.
See an example below.
```json
{
    "listeners":[
        {
            "name":"gitlab",
            "extractors":[
                {
                    "name":"push",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "name":"merge_request_created",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "name":"merge_request_completed",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "name":"merge_request_take_in_progress",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "name":"merge_request_merged",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
            ]            
        }
    ],
    "eventDispatchers":[]
}
```

`eventName` is a name of event that will be extracted from request to Listener.  
`extractionRules` is a set of [Extraction Rule](#extraction-rule) that will be described below.  
`validationRules` is a set of [Validation Rule](#validation-rule) that helps to ensure that current request body relates exactly to this extractor.  

### Validation Rule
Validation rule allows to validate json using [JSON Path](https://restfulapi.net/json-jsonpath/). Token that retrieved via path will be compared to expected value with selected operator. See example below:
```json
{
    "validationRules":[
        {
              "path": "headers.X-Gitlab-Token[0]",
              "value": "{{token from webhook configuration}}",
              "operator": "Equals"
        }
    ]
}
```

`path` is json path in the [request](#request)

An example will take first (`0`) `X-Gitlab-Token` header from request and check its equality to `{{token from webhook configuration}}` value.

Currently, there is two operators with self-explanatory names. 
* Equals
* NotEquals

Operators could be applied to single token at the same time - no validation across arrays.

### Extraction Rule

Extraction rules allows to extract values from request directly to event.
[JSON Path](https://restfulapi.net/json-jsonpath/) used to find token for extraction as well. See example below:
```json
{
    "extractionRules": [
        {
            "type": "FirstRegexMatch",
            "pattern": "[A-Z]{1,10}-\\d{1,10}",
            "path": "body.ref",
            "property": "issue"
        },
        {
            "type": "Copy",
            "path": "body.user_username",
            "property": "author"
        },
        {
            "type": "Copy",
            "path": "body.ref",
            "property": "ref"
        }
    ]
}
```

`path` is json path in the [request](#request).  
`property` is name of event property to set. If event already has value in the property then it will be replaced.  
`type` determines the way to extract specific property:
* `Copy` copies found json token as is
* `FirstRegexMatch` takes a first substring that match specified `pattern`  

## Event Dispatcher
Event dispatcher is responsible for dispatch an event with specific name to it's subscribers. See dispatcher configuraiton below:
```json
{
    "eventDispatchers": [
        {
            "eventName":"",
            "schema":{},
            "subscribers":[]
        }
    ]
}
```

`eventName` - the same name as used in [Extractor](#extractor).  
`schema` - [JSON Schema](https://json-schema.org/) used to guarantee data integrity and structure for event.
`subscribers` - list of subscribers for event represented by this dispatcher

## Event Subscriber
### Template Rule
#### Url
#### Body
#### Header
### Metadata