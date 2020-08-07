# Documentation Index

- [Documentation Index](#documentation-index)
- [Get Started with Docker Image](#get-started-with-docker-image)
- [Concept](#concept)
- [Understanding the Configuration](#understanding-the-configuration)
  - [Types](#types)
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
Docker image tags are the same as tags in the repository.
Each commit to `develop` branch has own docker image with tag `develop-$commit-short-sha`.  
This is done for contributors that don't want to wait 'till their changes appear in main release.

## Environment variables

Here is the list of supported environment variables:  
* `JSON_FILE_URL` - is Http URL or filesystem path to JSON configuration file.  
* `LOG_LEVEL` - is [NLog Log level](https://github.com/NLog/NLog/wiki/Configuration-file#log-levels) string. If nothing is set - Info will be a default. Use `Debug` level for configuraiton debugging.
This allows you to download configuration from secured sources like [Amazon S3](https://aws.amazon.com/s3/), or put file into attached [docker volume](https://docs.docker.com/storage/volumes/).
**Notice** that if your HTTP resource require authorization to download files, OpsBro can be authorized using query string only!

## Example

The command below will start OpsBro 0.2 container with valid configuration from repository. 

```sh
docker run -d --rm -e "JSON_FILE_URL=https://raw.githubusercontent.com/Lex45x/ops-bro/v0.2/src/OpsBro.Api/g2j.json" -p 8080:80 opsbro/ops-bro:0.2
```

After successfull image start you can navigate to `localhost:8080` and see [Swagger Documentation](https://swagger.io/).

![Swagger Documentation](swagger&#32;documentation.png)

As you can see, there is only one endpoint for all listeners.
`listenerName` is corrseponding to name of the [Listener](#listener).

You may `Try it Out` and get 200OK for any request body.

# Concept

ops-bro allows to connect services that have WebHooks to service that have REST API.

1. Service make a call to a specific ops-bro [Listener](#listener).
2. [Listener](#listener) extract events from the request using a list of [Extractors](#extractor).
3. Event then processed via [Event Dispathcer](#event-dispatcher) and distributed to all [Event Subscribers](#event-subscriber). 
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
Where the `listeners` is a collection of [Listeners](#listener) and `eventDispatchers` is a collection of [Event Dispathcer](#event-dispatchers).  
`config` is a json object that could hold [configuration](#config) values for the template.

Example below will describe an integraiton between Gitlab webhooks and Jira transitions.  
Original template could be found [HERE](../src/OpsBro.Api/g2j.json).

## Recommended initial knowledge

All of the links below will provide basic knowledge to understand what's going on.

* Understanding general idea of [Webhook API](https://en.wikipedia.org/wiki/Webhook).
* Understanding [REST API](https://en.wikipedia.org/wiki/REST) and [HTTP protocol](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) in general.
* [JSON](https://en.wikipedia.org/wiki/JSON) is a heart of this application. All data is represented in JSON format.
* [JSON Path](https://restfulapi.net/json-jsonpath/) act as a main tool of webhooks and events processing.

## Types
Configuration is implicitly use the internal application types.  
Each type represents a data, that might be somehow addressed or used inside the configuration file.

### Request
Inside the request object there are three properties:
* `query` - contains all query parameters
* `body` - contains json representation of the request body
* `headers` - contains request headers

```json
{
    "query":{
        "key":"value"
    },
    "body":{},
    "headers":{
        "key":"value"
    }
}
```

### Event Context
Inside the event context there is two properties:
* `event` - event data in json
* `config` - [configuration object](#config)

```json
{
    "event":{},
    "config":{}
}
```

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

# Troubleshooting and debug

OpsBro has several mechanisms that will help you to understand that somehting is going wrong.

## Prometheus and metrics

OpsBro has built-in support for Prometheus metrics.  
They are available on default `/metrics` route.  
More details about metrics could be found in dedicated [metrics specification](/METRICS.md)