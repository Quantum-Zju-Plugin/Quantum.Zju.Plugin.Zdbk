$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    foreach ($component in @(
        @{ Name = 'Quantum.Desktop'; Repository = 'io7-cn/Quantum.Desktop'; Pin = 'desktop' },
        @{ Name = 'Quantum.Zju.Plugin.Cas'; Repository = 'Quantum-Zju-Plugin/Quantum.Zju.Plugin.Cas'; Pin = 'cas' }
    )) {
        $revision = (Get-Content ".ci/$($component.Pin)-revision.txt" -Raw).Trim()
        if ($revision -notmatch '^[0-9a-f]{40}$') { throw 'Expected a fixed dependency revision.' }
        $target = ".ci-dependencies/$($component.Name)"
        if (-not (Test-Path "$target/.git")) {
            git init $target
            git -C $target fetch --depth 1 "https://github.com/$($component.Repository).git" $revision
            git -C $target checkout --detach $revision
        }
        if ((git -C $target rev-parse HEAD) -ne $revision) { throw 'Dependency checkout differs from its pin; preserve it for inspection.' }
        if (git -C $target status --porcelain) { throw 'Dependency checkout has changes; preserve it for inspection.' }
    }
} finally { Pop-Location }
