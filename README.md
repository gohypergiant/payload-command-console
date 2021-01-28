# Hypergiant Payload Command Console

The Payload Command Concole (PCC) is a desktop console application to assist in commanding CubeSats, Cygnus Hosted Payloads and ISS Experiments.

PCC is a .NET 3.1 Core Application that uses the Avalonia UI framework for rendering, and as such should be cross-platform compatible (but currently has been developed and tested only under Windows 10).

PCC is comprised of three larger "systems":

1. The UI Console itself
2. A Storage micro-service
3. A Executive micro-service

## Entities

- GroundStation
- Pass
- PassHistory
- Command
- CommandHistory
- CommandResult

## Payload Command Console

![screenshot](doc/assets/console_screenshot.png)

## Proxy Store Service

The purpose of Proxy Store Service is to provide a persistent store for commands and results.

### REST API

| Path | Verb | Description
|---|---|
| /api/v1/commands?passid={pass_id} | GET | Gets all Commands from the Store associated with a Pass |
| /api/v1/commands | POST | Inserts a Command into the Store |
| /api/v1/commands/{command_id}/history | POST | Inserts a History record for a Command into the Store |
| /api/v1/commands/{command_id}/history | GET | Retrieves all History records for a Command from the Store |
| /api/v1/groundstations | GET | Gets all Ground Stations from the Store |
| /api/v1/passes | GET | Gets all Passes from the Store |
| /api/v1/passes/{pass_id} | GET | Gets the specified Pass from the Store |
| /api/v1/passes/{pass_id} | PUT | Updates the specified Pass in the Store |
| /api/v1/passes | POST | Inserts a new Pass into the Store |
| /api/v1/history/passes/{pass_id} | GET  | Gets all history for the specified Pass |
| /api/v1/history/passes | POST  | Inserts a PassHistory record into the Store |


## ExecutiveService

The purpose of the Executive Service is to provide an implementation for how Commands get sent to the payload and how Results get retrieved (e.g. CFDP or SCP).

### REST API

| Path | Verb | Description
|---|---|
| /api/v1/configuration | GET | Gets the current Executive configuration |
| /api/v1/configuration | PUT | Updates the current Executive configuration |
| /api/v1/commands | POST | Sends a Command to the Executive for execution or delivery |


