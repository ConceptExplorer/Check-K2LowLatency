# Get Windows build info
$os = Get-CimInstance -Namespace root\cimv2 -ClassName Win32_OperatingSystem

# Get K2 fields (if they exist)
# Standardize to Get-CimInstance for modern PowerShell 7+ cross-compatibility
$k2 = Get-CimInstance -Namespace root\wmi -ClassName MS_SystemInformation -ErrorAction SilentlyContinue | Select-Object *

Write-Host "==============================="
Write-Host "   Windows K2 Status Checker"
Write-Host "==============================="

Write-Host ""
Write-Host "Your Windows Version:"
Write-Host " - $($os.Caption)"
Write-Host " - Build $($os.BuildNumber)"
Write-Host ""

# 1) Check if K2 Low Latency Profile exists
Write-Host "1) Checking if your system has the K2 Low Latency Profile feature..."

if ($k2.PSObject.Properties.Name -contains "LowLatencyProfile") {
    Write-Host "   ✔ K2 Low Latency Profile is INCLUDED in this Windows build."
} else {
    Write-Host "   ✘ K2 Low Latency Profile is NOT included in this Windows build."
}

Write-Host ""

# 2) Check if K2 Low Latency Profile is active
Write-Host "2) Checking if the Low Latency Profile is currently active..."

if ($k2.PSObject.Properties.Name -contains "LowLatencyProfile") {
    if ($k2.LowLatencyProfile -eq $true) {
        Write-Host "   ✔ Low Latency Profile is ACTIVE and in use."
    } else {
        Write-Host "   ✘ Low Latency Profile exists but is NOT active."
    }
} else {
    Write-Host "   ✘ Cannot check activation because the feature does not exist on this build."
}

Write-Host ""
Write-Host "---------------------------------------------"
Write-Host "Notes:"
Write-Host " - The K2 Low Latency Profile is only available on certain Insider builds."
Write-Host " - Your build: $($os.BuildNumber)"
Write-Host " - Required: Insider Canary/Dev builds above ~26100.5000"
Write-Host "---------------------------------------------"
