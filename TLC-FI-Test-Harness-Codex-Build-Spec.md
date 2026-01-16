# CODEX SPEC — TLC-FI Real-World Test Harness (.NET 8 WinForms)

## 1) Objective

Build a single **.NET 8 WinForms** application that can operate as:

### A) Client Mode (Real-world compatible)

- Connect to an existing TLC-FI endpoint on **TCP port 11501**
- Perform **Register** authentication exactly as per captured traces
- Perform **ReadMeta** discovery for object types **0..8**
- Perform **Subscribe** to required objects
- Receive **UpdateState** notifications and update the UI live
- Support **Deregister**

### B) Server Mode (Simulator)

- Host a TLC-FI endpoint (TCP port 11501)
- Require client **Register** before allowing other methods
- Implement **ReadMeta**, **Subscribe**, and server-pushed **UpdateState**
- Expose editable object sets and defaults:
  - Detectors default: **DET1..DET255** (type 4)
  - Outputs default: **OUT1..OUT255** (type 6)
- Names and indices can be changed in UI at runtime
- Allow multiple concurrent clients

Both modes must include:

- Debug console
- Raw JSON send/receive
- Message trace (Tx/Rx list) + export
- Profiles + persistence

---

## 2) Transport & Framing

- TCP port: **11501** default (editable in UI)
- UTF-8
- **NDJSON framing**: each JSON object is terminated by `\n`
- Reader must handle:
  - partial frames
  - multiple frames in one read
  - very large messages (long `ids` arrays)

---

## 3) JSON-RPC Rules

Support:

- Requests: `{"jsonrpc":"2.0","id":"msgid22","method":"X","params":{...}}`
- Responses: `{"jsonrpc":"2.0","id":"msgid22","result":{...}}` or `error`
- Notifications: `{"jsonrpc":"2.0","method":"UpdateState","params":{...}}`

**Important:** `id` can be **string** (e.g., `"msgid22"`). Treat it as `string` or `JsonElement`.

---

## 4) Authentication / Session (REAL IMPLEMENTATION)

### 4.1 Register (mandatory before anything else)

Client sends (exact shape):

```json
{
  "method":"Register",
  "params":{
    "username":"Chameleon",
    "password":"CHAM2",
    "type":1,
    "version":{"major":1,"minor":1,"revision":0},
    "uri":""
  },
  "id":"msgid22",
  "jsonrpc":"2.0"
}
```

Fields:

- `username` string
- `password` string
- `type` int (application type)
  - `0 = Consumer`
  - `1 = Provider`
  - `2 = Control`
- `version` object `{major, minor, revision}` (ints)
- `uri` string (can be empty)

### 4.2 Register success response (exact shape)

Server returns:

```json
{
  "jsonrpc":"2.0",
  "id":"msgid22",
  "result":{
    "sessionid":"SWARCO_PT_ID9",
    "facilities":{"type":1,"ids":["SWA_PT_0106"]},
    "version":{"major":1,"minor":1,"revision":0}
  }
}
```

### 4.3 Register failure response (exact shape)

```json
{
  "jsonrpc":"2.0",
  "id":"msgid22",
  "error":{"code":1,"message":"Incorrect credentials"}
}
```

Rules:

- If a client is **not registered**, reject any other method with error:
  - `code: 2, message: "Not registered"` (or configurable)
- Never log passwords in plaintext.

### 4.4 Deregister

Client sends:

```json
{"method":"Deregister","params":{"username":"Chameleon"},"id":"msgid40","jsonrpc":"2.0"}
```

Server behavior:

- Return `{result:{deregistered:true}}` and/or close socket (configurable; default close).

---

## 5) Object Types (must support 0..8)

Real-world traces use these types, so implement support for all:

- Type **0**: Session
- Type **1**: Facilities (TLCFacilities)
- Type **2**: Intersection
- Type **3**: SignalGroup
- Type **4**: Detector
- Type **5**: Input
- Type **6**: Output
- Type **7**: SpecialVehicleEvents (generator)
- Type **8**: Variables

**Client mode:** accept arbitrary IDs like `PBU_D_P5`, `UTC_O2`, `STREAM1`, etc.
**Server mode:** can generate default DET/OUT sets (DET1..DET255, OUT1..OUT255) plus optional demo Intersection + Facilities.

---

## 6) ReadMeta (exact response shape)

Request:

```json
{"method":"ReadMeta","params":{"type":4,"ids":["X","Y"]},"id":"msgid27","jsonrpc":"2.0"}
```

Response:

```json
{
  "jsonrpc":"2.0",
  "id":"msgid27",
  "result":{
    "objects":{"type":4,"ids":["X","Y"]},
    "meta":[{...},{...}],
    "ticks":81876230
  }
}
```

Rules:

- `objects.ids` order matches request order
- `meta[]` aligns by index to ids
- `ticks` must be present

Implementation approach:

- Store and display meta as JSON (use JsonDocument/JsonElement), with a key/value view.

Meta schemas to support (based on trace):

- Type 1 meta includes lists of intersections, signalgroups, detectors, inputs, outputs, variables, plus an `info` block.
- Type 0 meta includes `{sessionid, type}`.
- Type 2 meta includes lists of outputs/inputs/signalgroups/detectors plus `spvehgenerator`.
- Type 3 meta includes `intersection`, `intergreen[]`, `timing[]`.
- Type 4 meta includes `{id, generatesEvents}`.
- Type 5 meta includes `{id}` (minimal).
- Type 6 meta includes `{id, intersection}` (intersection can be null).
- Type 7 meta includes `{id}`.
- Type 8 meta includes `{id}`.

---

## 7) Subscribe (exact response shape)

Request:

```json
{"method":"Subscribe","params":{"type":6,"ids":["UTC_O2","UTC_O3"]},"id":"msgid35","jsonrpc":"2.0"}
```

Response:

```json
{
  "jsonrpc":"2.0",
  "id":"msgid35",
  "result":{
    "objects":{"type":6,"ids":["UTC_O2","UTC_O3"]},
    "data":[{"state":0,"faultstate":0,"stateticks":123},{"state":0,"faultstate":0,"stateticks":456}],
    "ticks":81876490
  }
}
```

Notes:

- Type 0 subscribe can return empty ids/data (seen in trace):
  - `{"objects":{"type":0,"ids":[]},"data":[],"ticks":...}`

---

## 8) UpdateState notifications (server → client)

Server sends notifications (no `id`):

```json
{
  "jsonrpc":"2.0",
  "method":"UpdateState",
  "params":{
    "ticks":81881100,
    "update":[
      {
        "objects":{"type":6,"ids":["UTC_O2","UTC_O3"]},
        "states":[{"state":0},{"state":0}]
      }
    ]
  }
}
```

Rules:

- `params.update[]` is a list of update groups
- Each group:
  - `objects{type, ids[]}`
  - `states[]` aligned to ids

**Ticks/stateticks can exceed signed 32-bit** (trace shows ~4294727386).
=> store `ticks` and `stateticks` as `ulong` or `long`.

---

## 9) State Shapes per Type (based on trace)

When rendering grids, interpret state payloads like:

- Type 2 (Intersection) subscribe/update:
  - `{state, stateticks, tlcOverrule}`
- Type 3 (SignalGroup):
  - `{state, stateticks, predictions:[], dynLF}`
- Type 4 (Detector):
  - `{state, faultstate, swico, stateticks}`
- Type 5 (Input):
  - `{state, faultstate, swico, stateticks}` (same shape seen)
- Type 6 (Output):
  - `{state, faultstate, stateticks}` (and optionally `reqState` for Control use)
- Type 8 (Variable):
  - `{value, lifetime}`
- Type 7 (SpecialVehicleEvents):
  - `{faultstate}`

Implementation tip:

- Keep a generic “latest state JSON” per object ID and show it even if not parsed.

---

## 10) Permission Rules

Use `Register.params.type`:

- `0 Consumer`: read-only (ReadMeta/Subscribe allowed; no writes)
- `1 Provider`: can publish detector/events in simulator; in client mode, still allow sending but expect server may reject
- `2 Control`: may write Output request attributes (if/when implemented)

Server mode must enforce:

- not registered → reject
- consumer cannot write
- control can write only permitted fields

Return JSON-RPC errors on violations.

---

## 11) WinForms UI Requirements

### 11.1 Top connection bar

- Mode: Server / Client
- Host (client)
- Port (default 11501)
- Username / Password
- Type dropdown:
  - Consumer (0)
  - Provider (1)
  - Control (2)
- Version inputs:
  - Major default 1
  - Minor default 1
  - Revision default 0
- URI textbox (can be empty)
- Start/Stop or Connect/Disconnect
- Status display:
  - Registered / not
  - sessionid
  - facilities ids
  - last ticks
  - server client count

### 11.2 Object Explorer (Discovery)

TreeView built from Facilities meta (type 1):

- Facilities ID
  - Intersection(s)
    - SignalGroups
    - Detectors
    - Inputs
    - Outputs
    - Variables
  - SpecialVehicleEvents
- Session

### 11.3 Tabs per object type

Tabs: Session(0), Facilities(1), Intersection(2), SignalGroup(3), Detector(4), Input(5), Output(6), SpVeh(7), Variables(8)

Each tab:

- Meta grid (key/value) + raw JSON viewer
- State grid (DataGridView) + raw JSON for selected
- Search/filter textbox

### 11.4 Simulator Config tab (Server mode)

- Generate defaults:
  - DET1..DET255
  - OUT1..OUT255
- Editable columns:
  - Index, Name/ID, all relevant state fields
- Buttons:
  - Toggle detectors
  - Apply output state changes
  - Random pulses
  - Bulk set ranges
- Save/load `config.json`

### 11.5 Debug panel (right side)

- Timestamped log
- Tx/Rx message list (time, dir, method, id, bytes)
- Raw JSON send box:
  - send to server (client mode)
  - broadcast to all clients (server mode)
- Pretty format + validate JSON
- Export trace (NDJSON or JSON array)

---

## 12) Client Workflow (Auto-discover + auto-subscribe)

On Connect:

1. TCP connect
2. Send Register
3. On success store `sessionid`, `facilities.ids[]`
4. Auto discovery sequence (toggle):
   - ReadMeta type 1 for facilities id
   - ReadMeta type 0 for sessionid
   - ReadMeta types 2..8 based on lists in facilities meta
5. Auto subscribe sequence (toggle):
   - Subscribe type 0,2,3,4,5,6,8,7 (order like trace is fine)
6. Start listen loop for UpdateState notifications and update UI live

---

## 13) Server Workflow (Simulator)

- Accept multiple clients concurrently
- Require Register first
- Validate users via `users.json` (must include Jason)
- Respond with Register success result including:
  - sessionid (generate `SIM_SESSION_<n>`)
  - facilities ids (default `SIM_FAC_1`)
  - version echo
- Support ReadMeta/Subscribe for types 0..8
- Emit UpdateState notifications to subscribed clients:
  - tick counter increments
  - stateticks updated on change

---

## 14) Persistence Files

- `users.json` (must include Jason user)
- `profiles.json` (connection presets)
- `config.json` (simulator objects and settings)
- `traces/` exported logs

---

## 15) Key Implementation Gotchas (MANDATORY)

1. Use `ulong` (or `long`) for ticks/stateticks (values exceed int32).
2. JSON-RPC id may be string; handle generically.
3. NDJSON framing must handle split/multiple frames robustly.
4. Support type 8 variables (`value/lifetime`) separately from normal state objects.
5. Meta and ids arrays can be very large; UI must stay responsive (async + UI invoke, virtual mode grid or paging recommended).
