# ttu-mms-relay

HTTP endpoint that relays incoming MMS from a Twilio webhook and copies into Dropbox.

- [ttu-mms-relay](#ttu-mms-relay)
- [Purpose](#purpose)
  - [Problem](#problem)
  - [Solution](#solution)
  - [Language](#language)
- [Installation](#installation)
  - [Configuration File](#configuration-file)
    - [AccessControl](#accesscontrol)
    - [Dropbox](#dropbox)
    - [Twilio](#twilio)
  - [Docker Container](#docker-container)

# Purpose

This application was developed to act as an MMS receiver/proxy for my sister to use during sports games for [TTU Sports](https://ttusports.com/).

## Problem

My sister wanted a way to allow guests at sports games to submit photos of themselves/friends/players to be featured on the jumbotron/big screen. Specifically:

- It had to be straightforward to use; no weird web forms
- It has to be as accessible as possible; limiting to social media logon is not ideal
- It needs to integrate with the existing Dropbox solution, [ttu-slideshow](https://github.com/kyleratti/ttu-slideshow)

## Solution

Anyone participating in this is going to have a smartphone if they'd be taking a picture and using a web app, therefore the most accessible system is to use MMS. By using MMS, we significantly lower the barrier to entry.

[Twilio](https://twilio.com) provides an excellent SMS and MMS receiving service, fair rates, cross-carrier support, and mature API. After purchasing a phone number, wherein you can even select the local exchange, Twilio can be configured to call a webhook via `POST` any time that number receives SMS/MMS messages.

Once the webhook is called and the request is validated, the picture is downloaded, uploaded to a "needs review" folder in Dropbox, tagged with the submitter's phone number, and temporary file removed from local disk.

Once the file is in Dropbox, staff can manually review the pictures for quality and ensure they're appropriate, drag them to the "live" folder, and have the picture automatically entered into the slideshow (with no modifications to the existing slideshow solution).

There is also support for a "trusted" list (photos from these phone numbers go straight to "live") and a "blocked" list (photos from these numbers are not processed).

## Language

This project is developed in ASP.NET Core in C#. I decided to switch to C# for this project as I will be using it at work much more frequently now.

The entire project was developed on my laptop in VSCode running on top of Ubuntu 20.04. It's developed against Mono and tested and deployed to an Ubuntu Server 20.04 machine in a Docker container.

# Installation

## Configuration File

For simplicity's sake, this application uses the `appsettings.json` configuration template that is recommended for use with ASP.NET Core by Microsoft.

An example file, [appsettings.example.json](appsettings.example.json), is provided. The important bits of configuration are listed under the `RelayConfig` section.

### AccessControl

Whether specifying strings into the `AccessControl.Trusted` or `AccessControl.Blocked` property, you should always use the _full_ phone number, including country code. For example, the country code for the US is `+1`, so `555-222-3456` would be entered as `"+15552223456"`.

| Key       | Value Type          | Description                                                                                                                                            | Example        |
| --------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------- |
| `Trusted` | `Array` of `string` | Array of **full** phone number strings, including country code, of phone numbers whose pictures should go straight to "live". US country code is `+1`. | `+15552350012` |
| `Blocked` | `Array` of `string` | Array of **full** phone number strings, including country code, of phone numbers whose pictures will not be processed at all. US country code is `+1`. | `+15552350012` |

### Dropbox

| Key            | Value Type | Description                                                                                                                                                                    | Example                       |
| -------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------- |
| `AccessToken`  | `string`   | Generated from [custom Dropbox app](https://www.dropbox.com/developers/apps/) -> `Generated access token`. The app will need `files.metadata.write` and `files.content.write`. | `asdk218ndasnd012eidqwkn9812` |
| `LiveFolder`   | `string`   | Path to the "live" folder that the slideshow is running from. Must begin and end with a `/`.                                                                                   | `/_testing/live/`             |
| `ReviewFolder` | `string`   | Path to the "needs review" folder that staff will look at before moving to "live". Must begin and end with a `/`.                                                              | `/_testing/needs review/`     |

### Twilio

| Key          | Value Type | Description                                                                                                                     | Example                    |
| ------------ | ---------- | ------------------------------------------------------------------------------------------------------------------------------- | -------------------------- |
| `AccountSid` | `string`   | The `Account Sid` key token from the Twilio dashboard of the associated account.                                                | `da12ehudwiao12dmiwqom120` |
| `AuthToken`  | `string`   | The `Auth Token` taken from the Twilio dashboard of the associated account. May be hidden until the `Reveal` button is clicked. | `123onkldas835h43o421j`    |

## Docker Container

These are the instructions to deploy the application via a Docker container. Running the service outside of a container is not supported.

1. Clone the repository
2. Copy the `appsettings.example.json` to `appsettings.json` and adjust settings as necessary.
3. Run `docker-compose build` to build the app's container and import `appsettings.json`
4. Run `docker-compose up` to start the container (or `docker-compose up -d` to start and detach from the console)
5. Point the Twilio `POST` endpoint to the publicly accessible URL of this application. Make sure you add `/mms/receive` to the end of the URL, as that is the listening endpoint. Example: `http://deployedapp.com/mms/receive`.

**Note:** I highly recommend running this application behind a reverse proxy!
