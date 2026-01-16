# TLC-FI Server Tool WinForms Harness

This repository contains a single **.NET 8 WinForms** application that can act as a TLC-FI client (Register/ReadMeta/Subscribe/UpdateState/Deregister) or as a TLC-FI simulator server with NDJSON framing and multi-client support.

## Solution

* `TlcFiTestHarness.sln`
* `TlcFiTestHarness/` WinForms project

## Running

> Build with Visual Studio 2022+ or `dotnet build` on Windows with .NET 8 installed.

* Server mode: choose `Server`, set port (default `11501`), click **Start**.
* Client mode: choose `Client`, set host/credentials, click **Connect**.

## JSON-RPC shapes

### Register

```json
{"method":"Register","params":{"username":"Chameleon","password":"CHAM2","type":1,"version":{"major":1,"minor":1,"revision":0},"uri":""},"id":"msgid22","jsonrpc":"2.0"}
```

### Register response (success)

```json
{"jsonrpc":"2.0","id":"msgid22","result":{"sessionid":"SIM_SESSION_1","facilities":{"type":1,"ids":["SIM_FAC_1"]},"version":{"major":1,"minor":1,"revision":0}}}
```

### Register response (error)

```json
{"jsonrpc":"2.0","id":"msgid22","error":{"code":1,"message":"Incorrect credentials"}}
```

### ReadMeta

```json
{"method":"ReadMeta","params":{"type":4,"ids":["DET1","DET2"]},"id":"msgid27","jsonrpc":"2.0"}
```

### Subscribe

```json
{"method":"Subscribe","params":{"type":6,"ids":["OUT1","OUT2"]},"id":"msgid35","jsonrpc":"2.0"}
```

### UpdateState notification

```json
{"jsonrpc":"2.0","method":"UpdateState","params":{"ticks":81881100,"update":[{"objects":{"type":6,"ids":["OUT1","OUT2"]},"states":[{"state":0},{"state":0}]}]}}
```

## Persistence

Files are created in `%AppData%/TlcFiTestHarness`:

* `users.json` (must include Jason)
* `profiles.json`
* `config.json`
* `scripts/*.json`

## Troubleshooting

* **Incorrect credentials**: verify `users.json` contains the username/password and allowed types.
* **Port in use**: change the port in the top bar before starting the server.
* **No updates**: ensure the client sent Subscribe and that the server has a configured model.
