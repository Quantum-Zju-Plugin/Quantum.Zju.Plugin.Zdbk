$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    python .standards/.ci/check-adoption.py
    dotnet restore Quantum.Zju.Plugin.Zdbk.slnx
    dotnet format Quantum.Zju.Plugin.Zdbk.slnx --verify-no-changes --no-restore
    dotnet build Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-restore
    dotnet test Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-build
    $output = Join-Path (Get-Location) 'artifacts/plugin'
    $artifactRoot = [IO.Path]::GetFullPath((Join-Path (Get-Location) 'artifacts')) + [IO.Path]::DirectorySeparatorChar
    $resolved = [IO.Path]::GetFullPath($output)
    if (-not $resolved.StartsWith($artifactRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected artifact path.' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
    dotnet publish src/Quantum.ZdbkPlugin/Quantum.ZdbkPlugin.csproj -c Release --no-restore --output $output -p:DebugType=None -p:DebugSymbols=false
    Copy-Item LICENSE,NOTICE $output
    Compress-Archive -Path "$output/*" -DestinationPath artifacts/quantum.plugin.zdbk.zip -Force
} finally { Pop-Location }
