# TLC-FI-Server-Tool
TLC-FI Server Tool WinForm.

## Purpose
This WinForms application acts as a lightweight TLC-Fi server-side test tool. It listens for TCP connections, logs inbound payloads, and can optionally echo data back to connected clients for quick protocol validation.

## Features
- Start/stop a TCP listener on a configurable port.
- View connected clients and send messages to a selected client.
- Log inbound/outbound traffic with optional hex payload display.
- Auto-echo mode for basic protocol handshake verification.

## References
- TLC-FI protocol overview (iVRI2 RIS-FI v1.0): https://www.ivera.nl/wp-content/uploads/2017/06/iVRI2_del_1b_IDD_RIS-FI_v1.0.pdf
- TLC-FI .NET reference implementation: https://github.com/CodingConnected/CodingConnected.TLCFI.NET
