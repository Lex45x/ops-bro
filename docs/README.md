# Documentation Index

- [Documentation Index](#documentation-index)
- [Get Started with Docker Image](#get-started-with-docker-image)
- [Concept](#concept)
- [Understanding the Configuration](#understanding-the-configuration)
  - [Types](#types)
    - [Event](#event)
    - [Request](#request)
    - [Event Context](#event-context)
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
  - [Config](#config)

# Get Started with Docker Image

You can find docker images in the [Docker Hub](https://hub.docker.com/repository/docker/opsbro/ops-bro).  

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
    "eventDispatchers":[],
    "config":{}
}
```
Where the `listeners` is a collection of Listener and `eventDispatchers` is a collection of Event Dispatcher.  
`config` is a json object that could hold configuration values for the template.

## Types
Types are not defined in the configuration, but are implicitly used to understand the logic under configuration. A list of types are presented as sub-header entries.

### Event
Event has name and Data object and represent a message about the system state change.

### Request
Inside the request object there are three properties:
* `query` - contains all query parameters
* `body` - contains json representation of the request body
* `headers` - contains request headers

### Event Context
Inside the event context there is two properties:
* `event` - event data in json
* `config` - [configuration object](#config)

## Listener

Each listener represents a source of incoming HTTP requests.  
E.g.: You want to build one-way integration between Gitlab and Jira. So, in this case you **listen** to Gitlab events and change state of the Jira issues accordingly. As a result you will create a single Listener, called gitlab.  
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

Each extractor represent a set of rules used to extract a single type of event from the request to a parent [Listener](#listener).  
E.g.: Gitlab listener acts as a source of different events: commit pushed, merge request created, merge request taken back in progress, merge request completed, merge request merged, etc. 
So, in this case, each of the listed event types will have dedicated extractor.
See an example below.
```json
{
    "listeners":[
        {
            "name":"gitlab",
            "extractors":[
                {
                    "comment":"Commits are pushed into branch",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "comment":"Merge request created",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "comment":"Merge request WIP status resolved",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "comment":"WIP status assigned to a Merge Request",
                    "eventName":"",
                    "extractionRules":[],
                    "validationRules":[]
                },
                {
                    "comment":"Merge request merged",
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

`comment` is used to describe a specific action that happened on the Webhook creator side
`eventName` is a name of event that will be extracted from request to Listener.  
`extractionRules` is a set of [Extraction Rule](#extraction-rule) that will be described below.  
`validationRules` is a set of [Validation Rule](#validation-rule) that helps to ensure that current request body relates exactly to this extractor.  

### Validation Rule
Validation rule allows to validate [Request](#request) using [JSON Path](https://restfulapi.net/json-jsonpath/). Json-token that was retrieved via path will be compared to expected value with selected operator. See example below:
```json
{
    "validationRules":[
        {
              "path": "headers.X-Gitlab-Token[0]",
              "value": "{{token from webhook configuration}}",
              "operator": "Equals"
        },
        {
              "path": "headers.X-Gitlab-Token[0]",
              "configPath": "gitlab.token",
              "operator": "Equals"
        }
    ]
}
```

`path` is json path in the [Request](#request)

An example will take first (`0`) `X-Gitlab-Token` header from request and check its equality to `{{token from webhook configuration}}` value.

Also, a comparison value could be provided from the [configuration](#config) object using `configPath` property.  
This property used to specify path inside [configuration](#config) object.  

Currently, there is two operators with self-explanatory names. 
* Equals
* NotEquals

Operators could be applied only to one json-token at a time - no validation across arrays.

### Extraction Rule

Extraction rules allows to specify a way to compose event from [Request](#request).
[JSON Path](https://restfulapi.net/json-jsonpath/) used to find json-token for extraction as well. See example below:
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
* `Copy` copies found json-token as is
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
`schema` - [JSON Schema](https://json-schema.org/) used to guarantee structure of the event.  
`subscribers` - list of subscribers for event represented by this dispatcher

## Event Subscriber
Event subscriber converts an event to an HTTP call.  
Each subscriber holds templates and rules for this templates.  
See an example of event subscriber
```json
{
    "subscribers": [
        {
            "urlTemplate": "https://example.com/{path}",
            "method": "POST",
            "bodyTemplate": {},
            "bodyTemplateRules": [],
            "headerTemplateRules": [],
            "urlTemplateRules": []
        }
    ]
}
```

### Template Rule
Template rule describes how to fill event/config json-token into predefined template.
Basically, template rules depends on the template type and so there are difference between them. See types defined below.

#### Url
Template rule for the URL replaces substring in the template with value found by Property.

```json
{
    "substring": "{ISSUE}",
    "property": "event.issue"
}
``` 
`substring` - substring to replace in urlTemplate from subscriber.  
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

#### Body
Template rule for body replaces json token in the template with token found by property path.

```json
{
    "path": "fields.assignee.name",
    "property": "event.author"
}
```
`path` - path inside the bodyTemplate.  
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

#### Header
Template rule for header adds a header with defined name to the http headers collection.

```json
{
    "headerName": "Authorization",
    "property": "meta.auth_header"
}
```

`headerName` - name of the header to be added
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

## Config

Config object created to improove reusability of the templates.
It can hold any value and may be imported into [validation rules](#validation-rule) as well as become a part of [Event Context](#event-context).

```json
"config": {
    "jira": {
        "auth": "jira-token"
    },
    "gitlab": {
        "token": "gitlab-token"
    }
```
