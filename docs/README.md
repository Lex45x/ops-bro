# Documentation Index

- [Documentation Index](#documentation-index)
- [Get Started with Docker Image](#get-started-with-docker-image)
  * [Environment variables](#environment-variables)
  * [Example](#example)
- [Concept](#concept)
- [Understanding the Configuration](#understanding-the-configuration)
  * [Recommended initial knowledge](#recommended-initial-knowledge)
  * [Types](#types)
    + [Request](#request)
    + [Event Context](#event-context)
  * [Versioning](#versioning)
  * [Listener](#listener)
    + [Unnesting rule](#unnesting-rule)
  * [Extractor](#extractor)
    + [Validation Rule](#validation-rule)
    + [Extraction Rule](#extraction-rule)
  * [Event Dispatcher](#event-dispatcher)
  * [Event Subscriber](#event-subscriber)
    + [Template Rule](#template-rule)
      - [Url](#url)
      - [Body](#body)
      - [Header](#header)
  * [Config](#config)
- [Troubleshooting and debug](#troubleshooting-and-debug)
  * [Prometheus and metrics](#prometheus-and-metrics)

# Get Started with Docker Image

You can find docker images in the [Docker Hub](https://hub.docker.com/repository/docker/opsbro/ops-bro).  
Docker image tags are the same as tags in the repository.  
Each commit to `develop` branch has its docker image with tag `develop-$commit-short-sha`.  
This is done for contributors who don't want to wait until their changes appear in the main release.

## Environment variables

Here is the list of supported environment variables:  
* `CONFIGURATION_FILE_URL` - is Http URL or filesystem path to JSON configuration file.  
* `JSON_FILE_URL` - **deprecated**, use `CONFIGURATION_FILE_URL` instead.  
This allows you to download the configuration from secured sources like [Amazon S3](https://aws.amazon.com/s3/) or put the file into the attached [docker volume](https://docs.docker.com/storage/volumes/).  
**Notice** that if your HTTP resource requires authorization to download files, OpsBro can only be authorized using query string only!  
* `LOG_LEVEL` - is [NLog Log level](https://github.com/NLog/NLog/wiki/Configuration-file#log-levels) string. If nothing is set - Info will be a default. Use the `Debug` level for configuration debugging.  

## Example

The command below will start OpsBro 0.3 container with the valid configuration from the repository. 

```sh
docker run -d --rm -e "JSON_FILE_URL=https://raw.githubusercontent.com/Lex45x/ops-bro/v0.3.1/templates/gitlab2jira.yaml" -p 8080:80 opsbro/ops-bro:0.3.1
```

After a successful image start, you can navigate to `localhost:8080` and see [Swagger Documentation](https://swagger.io/).

![Swagger Documentation](swagger&#32;documentation.png)

As you can see, there is only one endpoint for all listeners.
`listenerName` is corresponding to the name of the [Listener](#listener).

You may `Try it Out` and get 200OK for any request body.

# Concept

Ops-bro allows connecting services that have WebHooks to service that have REST API.

1. Service makes a call to a specific ops-bro [Listener](#listener).
2. [Listener](#listener) extract events from the request using a list of [Extractors](#extractor).
3. Event then processed via [Event Dispathcer](#event-dispatcher) and distributed to all [Event Subscribers](#event-subscriber). 
4. Each [Event Subscriber](#event-subscriber) converts event to HTTP request message and sends it to related service.

See generic flow on the image below.

![Concept Flow](concept&#32;flow.jpg)

# Understanding the Configuration

There is only one way to configure an application: via the JSON configuration file.
This file represents a JSON object that looks like an example below.
```yaml
"$schema": https://raw.githubusercontent.com/Lex45x/ops-bro/v0.3.1/src/OpsBro.Domain/settings-schema.json
version: ''
listeners: []
eventDispatchers: []
config: {}

```

<details>
<summary>JSON Example</summary>

```json
{
    "$schema":"https://raw.githubusercontent.com/Lex45x/ops-bro/v0.3.1/src/OpsBro.Domain/settings-schema.json",
    "version":"",
    "listeners":[],
    "eventDispatchers":[],
    "config":{}
}
```

</details>

Where the `listeners` is a collection of [Listeners](#listener) and `eventDispatchers` is a collection of [Event Dispathcer](#event-dispatchers).  
`config` is a JSON object that may hold [configuration](#config) values for the template.
`version` used to maintain compatibility between the OpsBro version and configuration file version.
`$schema` is the optional path to the configuration file schema of the stable OpsBro version. Used to simplify working with JSON editors.

The example below will describe an integration between Gitlab webhooks and Jira transitions.  
The original template can be found [HERE](/templates/gitlab2jira.yaml).

## Recommended initial knowledge

All of the links below will provide essential knowledge to understand what's going on.

* Understanding the general idea of [Webhook API](https://en.wikipedia.org/wiki/Webhook).
* Understanding [REST API](https://en.wikipedia.org/wiki/REST) and [HTTP protocol](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) in general.
* [JSON](https://en.wikipedia.org/wiki/JSON) is the heart of this application. All data is represented in JSON format.
* [JSON Path](https://restfulapi.net/json-jsonpath/) acts as the primary tool of webhooks and events processing.

## Types
The configuration is implicitly using the internal application types.  
Each type represents data that might be somehow addressed or used inside the configuration file.

### Request
Inside the request object there are three properties:
* `query` - contains all query parameters
* `body` - contains json representation of the request body
* `headers` - contains request headers
* `unnest` - contains the resluts of [Unnesting Rule](#unnesting-rule)

```json
{
    "query":{
        "key":"value"
    },
    "body":{},
    "headers":{
        "key":"value"
    },
    "unnest":{}
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

## Versioning

`version` field in the root of the configuration file is responsible for the expression of the compatible OpsBro version.  
Version field value corresponds to OpsBro releases.  
If the version from a configuration file is mismatch OpBro version - the application will fail with Config deserialization exception.  
If you need to have OpsBro working with different versions of the configuration file - you have to use multiple instances of OpsBro - one instance per configuration version.

## Listener

Each listener represents a source of incoming HTTP requests.  
E.g., You want to build one-way integration between Gitlab and Jira. So, in this case, you **listen** to Gitlab events and change the state of the Jira issues accordingly. As a result, you will create a single Listener called gitlab.  
See the example below.
```yaml
listeners:
- name: gitlab
  unnestingRules: []
  extractors: []
```
<details>
<summary>JSON Example</summary>

```json
{
    "listeners":[
        {
            "name":"gitlab",
            "unnestingRules":[],
            "extractors":[]            
        }
    ],
    "eventDispatchers":[]
}
```

</details>


`extractors` is a list of [Extractor](#extractor) that is described below.

### Unnesting rule

This feature name was inspired by PL/pgSQL array funciton [unnest](https://www.postgresql.org/docs/9.1/functions-array.html).  
Unnesting rules allows to process a single listener call as a sequence of listener calls.
Unnest operation will be applied to [Request](#request) object.
```yaml
listeners:
- name: gitlab
  unnestingRules:
  - type: PerRegexMatch
    path: body.object_attributes.source_branch
    pattern: "[A-Z]{1,10}-\\d{1,10}"
    target: issue
  extractors: []
```

<details>
<summary>JSON Example</summary>

```json
{
    "listeners":[
        {
            "name":"gitlab",
            "unnestingRules": [
              {
                "type": "PerRegexMatch",
                "path": "body.object_attributes.source_branch",
                "pattern": "[A-Z]{1,10}-\\d{1,10}",
                "target": "issue"
              }
            ],
            "extractors":[]            
        }
    ],
    "eventDispatchers":[]
}
```

</details>


`"type": "PerRegexMatch"` - right now only one unnest type is allowed. It will unnest array of all macthes of regex in property specified via Path.  
`path` - [JSON Path](https://restfulapi.net/json-jsonpath/) used to find json-token for matching.  
`pattern` - Regex itself.  
`target` - name of the property for each unnesting entry. All properties will be created in `unnest` property of [Request](#request) object.  

In the given example, call to Gitlab listener with `source_branch` may containing multiple JIRA Issue keys will be unnested to multiple calls with unnest.issue set to regex match.   
Notice, that unnesting rules can be stacked. In the result you will have [Cartesian product](https://en.wikipedia.org/wiki/Cartesian_product) of all regex matches.
Also, if property defined in unnesting rule is not found - in the result will be a single unchanged payload.

## Extractor

Each extractor represents a set of rules used to extract a single type of event from the request to a parent [Listener](#listener).  
E.g., Gitlab listener acts as a source of different events: commit pushed, merge request created, merge request taken back in progress, merge request completed, merge request merged, and others. 
So, in this case, each of the listed event types will have a dedicated extractor.
See the example below.
```yaml
listeners:
- name: gitlab
  extractors:
  - comment: Commits are pushed into branch
    eventName: ''
    extractionRules: []
    validationRules: []
  - comment: Merge request created
    eventName: ''
    extractionRules: []
    validationRules: []
  - comment: Merge request WIP status resolved
    eventName: ''
    extractionRules: []
    validationRules: []
  - comment: WIP status assigned to a Merge Request
    eventName: ''
    extractionRules: []
    validationRules: []
  - comment: Merge request merged
    eventName: ''
    extractionRules: []
    validationRules: []
```
<details>
<summary>JSON Example</summary>

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
                }
            ]            
        }
    ]
}
```

</details>


`comment` is used to describe a specific action that happened on the Webhook creator side
`eventName` is a name of an event that will be extracted from request to Listener.  
`extractionRules` is a set of [Extraction Rule](#extraction-rule) that will be described below.  
`validationRules` is a set of [Validation Rule](#validation-rule) that ensures that the current request body relates exactly to this extractor.  

### Validation Rule
Validation rule allows to validate [Request](#request) using [JSON Path](https://restfulapi.net/json-jsonpath/). Json-token that was retrieved via path will be compared to the expected value with the selected operator. See example below:
```yaml
validationRules:
- path: headers.X-Gitlab-Token[0]
  value: "{{token from webhook configuration}}"
  operator: Equals
- path: headers.X-Gitlab-Token[0]
  configPath: gitlab.token
  operator: Equals
```
<details>
<summary>JSON Example</summary>

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

</details>


`path` is json path in the [Request](#request)

An example will take first (`0`) `X-Gitlab-Token` header from request and check its equality to `{{token from webhook configuration}}` value.

Also, a comparison value could be provided from the [configuration](#config) object using `configPath` property.  
This property used to specify path inside [configuration](#config) object.  

Currently, there are two operators with self-explanatory names. 
* Equals
* NotEquals

Operators could be applied only to one JSON-token at a time - no validation across arrays.

### Extraction Rule

Extraction rules allow specifying a way to compose the event from [Request](#request).
[JSON Path](https://restfulapi.net/json-jsonpath/) used to find json-token for extraction as well. See example below:
```yaml
extractionRules:
- type: FirstRegexMatch
  pattern: "[A-Z]{1,10}-\\d{1,10}"
  path: body.ref
  property: issue
- type: Copy
  path: body.user_username
  property: author
- type: Copy
  path: body.ref
  property: ref
```
<details>
<summary>JSON Example</summary>

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

</details>


`path` is json path in the [request](#request).  
`property` is the name of the event property to set. If the event already has value in the property then it will be replaced.  
`type` determines the way to extract specific property:
* `Copy` copies found JSON-token as is
* `FirstRegexMatch` takes a first substring that matches specified `pattern`  

## Event Dispatcher
The Event dispatcher is responsible for dispatch an event with a specific name to its subscribers. See dispatcher configuration below:
```yaml
eventDispatchers:
- eventName: ''
  schema: {}
  subscribers: []
```
<details>
<summary>JSON Example</summary>

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

</details>


`eventName` - the same name as used in [Extractor](#extractor).  
`schema` - [JSON Schema](https://json-schema.org/) used to guarantee structure of the event.  
`subscribers` - list of subscribers for the event represented by this dispatcher

## Event Subscriber
The event subscriber converts an event to an HTTP call.  
Each subscriber holds templates and rules for these templates.  
See an example of event subscriber.
```yaml
subscribers:
- urlTemplate: https://example.com/{path}
  method: POST
  bodyTemplate: {}
  bodyTemplateRules: []
  headerTemplateRules: []
  urlTemplateRules: []
```
<details>
<summary>JSON Example</summary>

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

</details>

### Template Rule
The template rule describes how to fill event/config JSON-token into a predefined template.
Template rules depend on the template type, and so there is a difference between them. See the types defined below.

#### Url
The template rule for the URL replaces substring in the template with the value found by Property.
```yaml
substring: "{ISSUE}"
property: event.issue
``` 
<details>
<summary>JSON Example</summary>

```json
{
    "substring": "{ISSUE}",
    "property": "event.issue"
}
``` 

</details>

`substring` - substring to replace in urlTemplate from subscriber.  
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

#### Body
The template rule for body replaces JSON token in the template with token found by the property path.

```yaml
path: fields.assignee.name
property: event.author
```

<details>
<summary>JSON Example</summary>

```json
{
    "path": "fields.assignee.name",
    "property": "event.author"
}
```

</details>

`path` - path inside the bodyTemplate.  
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

#### Header
The template rule for the header adds a header with a defined name to the HTTP headers collection.

<details>
<summary>JSON Example</summary>

```yaml
headerName: Authorization
property: meta.auth_header
```

</details>

`headerName` - name of the header to be added
`property` - [JSON Path](https://restfulapi.net/json-jsonpath/) to the value inside [Event Context](#event-context)

## Config

Config object created to improve the reusability of the templates.
It can hold any value and may be imported into [validation rules](#validation-rule) as well as become a part of [Event Context](#event-context).

```yaml
config:
  jira:
    auth: jira-token
  gitlab:
    token: gitlab-token
```

<details>
<summary>JSON Example</summary>

```json
"config": {
    "jira": {
        "auth": "jira-token"
    },
    "gitlab": {
        "token": "gitlab-token"
    }
```

</details>

# Troubleshooting and debug

OpsBro has several mechanisms that will help you to understand that something is going wrong.

## Prometheus and metrics

OpsBro has built-in support for Prometheus metrics.  
They are available on the default `/metrics` route.  
More details about metrics could be found in dedicated [metrics specification](/docs/METRICS.md)