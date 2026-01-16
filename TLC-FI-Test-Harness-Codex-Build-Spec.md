# TLC-FI Test Harness (.NET 8 WinForms) — Codex Build Spec (Real-World Ready)

## 0) Goal

Build a single WinForms app that can run as:

1. **Server (TLC-FI Provider/Facilities Simulator)**
   - Hosts a TCP listener on port 11501
   - Authenticates clients (TLS mutual auth preferred; password fallback)
   - Supports multiple concurrent clients
   - Implements TLC-FI semantics for Detectors and Outputs (defaults 1..255)
   - Supports Subscribe / UpdateState / NotifyEvent / ReadMeta
   - Tracks Session objects + ApplicationType (Provider/Consumer/Control)
2. **Client (ITS Application simulator)**
   - Connects to server
   - Authenticates using configured mode
   - Acts as Provider, Consumer, or Control
   - Allows interactive editing of DET/OUT and sending/receiving TLC-FI calls
   - Has robust debug console + traffic monitor + replay

## 1) Assumptions and “Spec vs Implementation”

TLC-FI defines:
- Objects (Detector, Output, Session / ControlApplication etc.)
- Methods: Subscribe, UpdateState, NotifyEvent, ReadMeta

TLC-FI does **not** define:
- TCP framing
- Port numbers
- Exact auth handshake (delegated to underlying mechanisms / Generic Facilities Interface / security)

Therefore:
- We implement a real-world auth layer in front of TLC-FI methods.
- After auth, all messages follow TLC-FI method semantics.

## 2) Network & Framing

- Default port: 11501
- Transport: TCP
- Encoding: UTF-8
- Framing: NDJSON (newline-delimited JSON objects)
- Each JSON-RPC object is 1 line terminated by `\n`
- Reader must support partial frames and multiple frames per read

## 3) Authentication Modes (Most likely + fallback)

### 3.1 Auth Mode A (Preferred / “Real-world”): TLS Mutual Authentication (mTLS)

**Server:**
- Uses `SslStream` over `TcpClient` after accept
- Requires a server certificate (PFX)
- Requires client certificate (optional toggle; default **ON** in “Real-world” profile)
- Validates client cert chain and (optionally) thumbprint allow-list

**Client:**
- Loads client PFX cert
- Validates server certificate (optional: allow self-signed in test profile)

**Outcome:**
- Once TLS is established and client cert is accepted, the client is “authenticated”.
- The client still must declare `ApplicationType` via the Session object exchange (see Section 4).

**Why this is most likely:**
- Many roadside / ITS systems use PKI and mTLS.
- TLC-FI doc points security to underlying mechanisms rather than embedding credentials in messages.

### 3.2 Auth Mode B (Fallback / Lab): JSON-RPC Register handshake (username/password)

This is for depot / quick vendor trials when certs aren’t available yet.

Important: This handshake happens **before** sending TLC-FI Subscribe/UpdateState etc.

#### 3.2.1 Registration handshake (MANDATORY)

Before any TLC-FI methods (Subscribe, ReadMeta, UpdateState, NotifyEvent) are accepted, the client must successfully call:

**JSON-RPC method: Register**

Client → Server request:

```json
{
  "method": "Register",
  "params": {
    "username": "CHAM",
    "password": "CHAM2",
    "type": 1,
    "version": 1,
    "revision": 0,
    "uri": "127.0.0.1"
  },
  "id": "msgid12",
  "jsonrpc": "2.0"
}
```

Field meanings (implement exactly):
- `username` (string) – required
- `password` (string) – required
- `type` (int) – required
  - `0` = Consumer
  - `1` = Provider
  - `2` = Control
- `version` (int) – required (store and log; enforce if needed)
- `revision` (int) – required (store and log; enforce if needed)
- `uri` (string) – required (store; may be used for audit/debug)

#### 3.2.2 Registration failure

Server responds with JSON-RPC error:

```json
{
  "jsonrpc": "2.0",
  "id": "msgid12",
  "error": { "code": 1, "message": "Incorrect credentials" }
}
```

Rules:
- On any auth failure:
  - Return the above error shape
  - Keep socket connected OR close it (configurable; default: keep connected to allow retry)
  - Do not allow any TLC-FI method calls until registered successfully.
- If an unregistered client calls TLC-FI methods:
  - Respond with JSON-RPC error (example):
    - `{ "jsonrpc":"2.0", "id":<same>, "error":{"code":2,"message":"Not registered"} }`
  - Keep the code configurable; some systems are picky.

#### 3.2.3 Registration success (define result)

Success payload:

```json
{
  "jsonrpc": "2.0",
  "id": "msgid12",
  "result": {
    "accepted": true,
    "sessionid": "S1",
    "type": 1,
    "version": 1,
    "revision": 0
  }
}
```

Notes:
- Keep `accepted` and `sessionid` stable across reconnects only if you want; otherwise a new session per connection is fine.
- Also create/track an internal Session object for the connection:
  - SessionId, ApplicationType, Username, ClientEndpoint, LastSeen, Version, Revision, etc.

## 4) Session / ApplicationType (TLC-FI-aligned)

TLC-FI models application sessions as Session objects (e.g., ControlApplication / ProviderApplication / ConsumerApplication). The tool must represent this, regardless of auth mode.

### 4.1 Application Types

Enum:
- `0` = Consumer
- `1` = Provider
- `2` = Control

### 4.2 Session object tracking

For each connected client on server:
- `sessionId` (server-generated)
- `username` (if password auth) **OR** `certSubject/Thumbprint` (if mTLS)
- `applicationType` (declared/negotiated)
- `version`, `revision`, `uri` (from Register, stored for audit/debug)
- `controlState` etc. (if ControlApplication)

### 4.3 Realistic control state logic

Implement control states (minimum):
- NotConfigured
- Offline
- ReadyToControl
- StartControl
- InControl
- EndControl
- Error

Server must enforce:
- Control clients can write `reqState` etc only when appropriate (practical enforcement rules below).

## 5) Permissions (Mandatory)

Enforce based on `ApplicationType`:

| Action | Provider | Consumer | Control |
| --- | --- | --- | --- |
| ReadMeta | ✅ | ✅ | ✅ |
| Subscribe | ✅ | ✅ | ✅ |
| Receive UpdateState/NotifyEvent | ✅ | ✅ | ✅ |
| Send NotifyEvent (Detector events) | ✅ | ❌ | ❌ |
| Write Detector.state | ✅ | ❌ | ❌ |
| Write Output.reqState | ❌ | ❌ | ✅ |

On violation:
- Return JSON-RPC error `-32010` “Permission denied”
- Additionally, if implementing TLC-FI SessionEvent codes, emit a SessionEvent consistent with “incorrect application type”.

## 6) TLC-FI Methods (Implemented)

### 6.1 ReadMeta
- Return META fields for requested object type and IDs.

### 6.2 Subscribe
- Params include object reference `{ type, ids }`
- Replace any existing subscription for that object type (per TLC-FI)
- Return initial “complete object” state **without META**

### 6.3 UpdateState
- Used by:
  - ITS-A to write writeable attributes only (e.g., Output.reqState)
  - Facilities to publish readable attributes only
- Support atomic updates: multiple objects updated in one group must be applied all-or-nothing.

### 6.4 NotifyEvent
- Used to notify event objects (e.g., DetectorEvent)
- Event is volatile: delivered once, not tracked as normal state.

## 7) Objects Implemented (Minimum for your use)

### 7.1 Detector (Type = 4)

**META:**
- `id` (string) — editable in UI (default `DET{Index}`)
- `generatesEvents` (bool)

**STATE:**
- `state` (0/1)
- `faultstate` (enum int)
- `swico` (enum int)
- `stateticks` (long)

### 7.2 Output (Type = 6)

**META:**
- `id` (string) — editable in UI (default `OUT{Index}`)

**STATE:**
- `state` (nullable int allowed)
- `faultstate` (int)
- `reqState` (nullable int) — writeable by Control

Note: Include “exclusive vs non-exclusive” flag as metadata (enhancement) even if you don’t fully simulate both.

### 7.3 Session (Type = 0)

Maintain session objects for:
- ControlApplication
- ProviderApplication
- ConsumerApplication

At minimum:
- `sessionid`
- `type` (ApplicationType)
- For Control:
  - `reqIntersection`, `reqControlState`, `controlState`, etc.

## 8) Defaults and Naming Rules

### 8.1 Defaults
- Detectors: 255 default
- Outputs: 255 default
- Names:
  - `DET1..DET255`
  - `OUT1..OUT255`

### 8.2 Editing

UI must allow editing:
- Index
- Name/ID
- All state fields relevant

And must validate:
- Index 1..255
- Unique name per list
- Unique index per list

## 9) WinForms UI Requirements (All features included)

### 9.1 Top Panel (Connection & Identity)
- Mode: Server / Client
- Host (client)
- Port (default 11501)
- Auth Mode dropdown:
  - “TLS (mTLS)”
  - “Username/Password”
- TLS inputs (enabled only in TLS mode):
  - Server PFX path + password (server mode)
  - Client PFX path + password (client mode)
  - “Require client certificate” checkbox (server mode)
  - “Allow self-signed server cert” checkbox (client mode)
- Username / Password fields (enabled only in password mode)
- ApplicationType dropdown (Consumer/Provider/Control, mapped to 0/1/2)
- Version numeric input (default 1)
- Revision numeric input (default 0)
- URI textbox (default `127.0.0.1` or local endpoint)
- Start/Stop or Connect/Disconnect button
- Status indicators:
  - server running / client connected
  - authenticated (Yes/No)
  - sessionId
  - client count (server mode)

### 9.2 Tabs (Left)

**A) Detectors (DataGridView)**
- Index, Name, generatesEvents, state, faultstate, swico, stateticks, notes
- Buttons: Toggle, Set Occupied/Unoccupied, Pulse, Random generator

**B) Outputs (DataGridView)**
- Index, Name, state, reqState, faultstate, stateticks, notes
- Buttons: Apply Req→State (server sim), Clear Req, Bulk set

**C) Intersections (Enhancement)**
- Support multiple intersections as separate object groups
- Assign detectors/outputs to intersection(s)

**D) Control (Only when ApplicationType=Control)**
- ControlState display + ReqControlState selection
- Buttons to request StartControl / EndControl / ReadyToControl
- “Atomic Update builder” UI to build one UpdateState with multiple objects

**E) Scripts / Scenarios (Enhancement)**
- JSON-defined scenario runner:
  - set detector state
  - set output reqState
  - wait
  - loop
- Start/Stop
- Save/load scripts

### 9.3 Right Side (Debug)
- Log console with timestamps
- Message Trace table: time, dir, method, bytes, summary
- Raw JSON send textbox + Send button
- Pretty-print and Validate JSON buttons
- Export trace to JSON file
- Replay trace (offline) feature

## 10) Server Implementation Details

### 10.1 Connection pipeline

On accept:
1. Establish transport:
   - If TLS mode: wrap socket stream in `SslStream` and authenticate as server
   - Else: plain `NetworkStream`
2. Perform authentication:
   - TLS mode: validate client certificate (if required)
   - Password mode: require `Register` JSON-RPC before any TLC-FI method
3. Create server-side `ClientSession`
4. Allow TLC-FI method processing

### 10.2 Multi-client
- Maintain concurrent dictionary of sessions
- Each session has subscriptions per object type
- Broadcast UpdateState only to matching subscriptions

### 10.3 Tick service
- Global ticks counter at 100ms configurable
- `stateticks` updated per object on state change

### 10.4 Heartbeat & reconnect guidance
- Implement Heartbeat notification or method
- Track last-seen time per client
- Disconnect idle or heartbeat-failed client after timeout
- Client uses exponential backoff reconnect (enhancement)

## 11) Client Implementation Details

### 11.1 Connection pipeline
1. Connect TCP
2. If TLS: `SslStream.AuthenticateAsClient`
3. If password: send `Register`
4. After authenticated:
   - Optionally send ReadMeta + Subscribe-all (toggles)
5. Handle incoming UpdateState/NotifyEvent and update UI

### 11.2 Control writes

When user edits `reqState`:
- Send UpdateState with writeable attributes only
- Mark row “pending” until echoed back (enhancement)

## 12) Persistence Files (Required)

Create these next to the EXE (or in AppData):

### 12.1 `config.json`
- detectors, outputs, intersections
- sim settings
- UI preferences

### 12.2 `users.json`

Must include “Jason” entry.
Do not log or hardcode the real password in code. Store it in file.

Example:

```json
{
  "users": [
    { "username": "Jason", "password": "<set_me>", "allowedTypes": [2] },
    { "username": "admin", "password": "<set_me>", "allowedTypes": [0, 1, 2] }
  ]
}
```

Enhancement: support hashed passwords with per-user salt (toggle).

### 12.3 `profiles.json`

Saved connection profiles:
- host, port
- auth mode
- cert paths
- username
- app type
- autosubscribe options

### 12.4 `/scripts/*.json`

Scenario scripts.

## 13) Enhancements (Must include)

- Search/filter grids
- Bulk edit dialog (ranges and selections)
- Copy/paste grid blocks
- Client list in server mode with:
  - username/cert subject
  - app type
  - subscriptions
  - kick/disconnect
- Trace export and replay
- Auto-reconnect with exponential backoff
- Heartbeat monitoring
- Robust error handling (never crash UI)

## 14) Code Structure (Required)

- `Models/*` (Detector, Output, Intersection, Session, enums)
- `Core/*` (StateStore, TickService, Validation, Persistence)
- `Transport/*` (NdjsonFramer, StreamReadLoop, StreamWriteQueue)
- `Auth/*`
  - `TlsAuth.cs` (cert validation)
  - `PasswordAuth.cs` (users.json)
  - `RegistrationHandler.cs` (Register handler)
- `JsonRpc/*` (dispatcher, message types, errors)
- `Server/*` (ServerHost, ClientConnection, SubscriptionManager)
- `Client/*` (ClientHost, AutoReconnect, Protocol helpers)
- `UI/*` (MainForm + dialogs)
- `Logging/*` (UiLogger, FileLogger)

## 15) Testing Checklist (Codex must implement)

- Start server (TLS off) → connect 2 clients with password auth
- Verify wrong password rejected and logged
- Verify Consumer cannot write reqState
- Verify Control can write reqState and server echoes state
- Verify subscriptions isolate updates (client sees only subscribed objects)
- Verify NDJSON frame handling (partial/multi frames)
- Verify TLS mode works with self-signed certs in test profile

## 16) Notes for tomorrow’s “exact implementation”

Because vendor implementations vary, this tool must be pluggable:
- Auth mode is configurable per profile
- Password auth supports:
- Register method (JSON-RPC)
  - Optional “pre-RPC handshake line” (easy to add later)
- TLS supports:
  - require client cert ON/OFF
  - thumbprint allow list
  - allow self-signed ON/OFF

So tomorrow, when you find out “how it’s exactly implemented”, you’ll likely only need to tweak:
- whether they use mTLS only
- whether they have a JSON-RPC login method name/shape
- whether token is required in each message
