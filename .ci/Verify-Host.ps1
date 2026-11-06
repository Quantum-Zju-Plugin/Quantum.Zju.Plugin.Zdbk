$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & ./.ci/Checkout-Dependencies.ps1
    dotnet restore tests/Quantum.HostIntegration.Tests/Quantum.HostIntegration.Tests.csproj -p:QuantumCoreOnly=true
    $previousCoreOnly = $env:QuantumCoreOnly
    try {
        $env:QuantumCoreOnly = 'true'
        dotnet format tests/Quantum.HostIntegration.Tests/Quantum.HostIntegration.Tests.csproj --verify-no-changes --no-restore --include tests/Quantum.HostIntegration.Tests
    } finally { $env:QuantumCoreOnly = $previousCoreOnly }
    dotnet test tests/Quantum.HostIntegration.Tests/Quantum.HostIntegration.Tests.csproj -c Release -p:QuantumCoreOnly=true --no-restore
} finally { Pop-Location }
