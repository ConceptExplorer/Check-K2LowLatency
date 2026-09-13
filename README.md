# Check-K2LowLatency

A lightweight, zero-dependency PowerShell utility that inspects Windows WMI telemetry to determine if the **K2 Low Latency Profile** feature is present and currently active on your system.

## Overview

Configuring and verifying low-latency features on modern Windows builds traditionally required installing third-party hardware monitors (e.g., HWiNFO64) and digging through raw registry keys or driver logs. 

`Check-K2LowLatency` eliminates that friction by directly querying the `root\wmi:MS_SystemInformation` CIM namespace, providing an instant diagnostic readout and actionable Insider build requirements.

## Features

- **Zero Third-Party Dependencies:** Queries native Windows Management Instrumentation (WMI / CIM) APIs.
- **Defensive Property Checking:** Safely inspects WMI schema properties without throwing runtime exceptions on unsupported builds.
- **Clear Diagnostic Telemetry:** Separates feature *presence* (OS support) from feature *activation* (runtime status).

## Usage

1. Open PowerShell (v5.1 or v7+).
2. Execute the script:

```powershell
.\Check-K2LowLatency.ps1