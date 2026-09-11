#!/usr/bin/env pwsh

[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $GithubToken = $null,

    [Parameter()]
    [switch]
    $NoBuild,

    [Parameter()]
    [string]
    $BuildType = 'Debug',

    [Parameter()]
    [switch]
    $NoCache,

    [Parameter()]
    [switch]
    $NoUpdateREADME
)

Set-Location $PSScriptRoot/..

# Build plugins
if (-not $NoBuild) {
    Remove-Item ./out/$BuildType -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item ./SubmoduleAssembly -Recurse -Force -ErrorAction SilentlyContinue
    & $PSScriptRoot/submodule_build.ps1 -BuildType $BuildType -ErrorAction Stop
    dotnet build Plugin.slnx -c $BuildType
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

New-Item -Name ./cache -ItemType Directory -Force
Remove-Item ./publish -Recurse -Force -ProgressAction SilentlyContinue -ErrorAction Ignore
New-Item -Name ./publish -ItemType Directory -Force

function Get-TShockZip {
    param (
        [Parameter(Position = 0)]
        [string] $OutFile
    )
    function Invoke-GitHubRequest {
        param(
            [Parameter(Mandatory = $true)][string]$Uri,
            [Object]$Body,
            [String]$ContentType,
            [PSCredential]$Credential,
            [System.Collections.IDictionary]$Headers,
            [String]$InFile,
            [Int32]$MaximumRedirection,
            [Microsoft.PowerShell.Commands.WebRequestMethod]$Method,
            [String]$OutFile,
            [String]$SessionVariable,
            [Int32]$TimeoutSec,
            [String]$TransferEncoding,
            [String]$UserAgent,
            [Microsoft.PowerShell.Commands.WebRequestSession]$Session
        )
        $p = @{}
        $PSBoundParameters.GetEnumerator() | ForEach-Object { $p.Add( $_.Key, $_.Value) }
        if ($GithubToken) {
            $secureGithubToken = ConvertTo-SecureString $GithubToken -AsPlainText -Force
            return Invoke-WebRequest -Authentication Bearer -Token $secureGithubToken @p
        }
        else {
            return Invoke-WebRequest @p
        }
    }

    $rid = if ([System.Environment]::OSVersion.Platform -Match "Unix") { "linux-(x64|amd64)" } else { "win-(x64|amd64)" }
    $release = Invoke-GitHubRequest -Uri 'https://api.github.com/repos/UnrealMultiple/Ref-Tile-TShock/releases' | ConvertFrom-Json | Select-Object -First 1
    if (-not $release) {
        throw "Ref-Tile-TShock 仓库没有任何 Release,无法下载 TShock!"
    }
    $downloadUrl = $release.assets | Where-Object browser_download_url -Match $rid | Select-Object -First 1 -ExpandProperty browser_download_url
    if (-not $downloadUrl) {
        $available = ($release.assets | ForEach-Object { $_.name }) -Join ", "
        throw "在 Release '$($release.tag_name)' 中找不到匹配 '$rid' 的资产。可用资产: $available"
    }
    Write-Host "下载 TShock 资产: $downloadUrl"
    Invoke-GitHubRequest -Uri $downloadUrl -OutFile $OutFile
    $size = (Get-Item $OutFile).Length
    if ($size -lt 1024) {
        throw "下载的 TShock 文件异常过小($size 字节),内容: $(Get-Content $OutFile -Raw)"
    }
    Write-Host "TShock 下载完成: $OutFile ($size 字节)"
}

# Prepare TShock
if (-not(Test-Path ./cache/TShock.zip -PathType Leaf) -or $NoCache) {
    Get-TShockZip ./cache/TShock.zip
}
# 下载的资产可能是 zip 或 tar.gz,先识别真实格式
$zipBytes = [System.IO.File]::ReadAllBytes((Resolve-Path ./cache/TShock.zip))[0..1]
$isZip = ($zipBytes[0] -eq 0x50 -and $zipBytes[1] -eq 0x4B)        # PK
$isGzip = ($zipBytes[0] -eq 0x1F -and $zipBytes[1] -eq 0x8B)        # \x1f\x8b
Write-Host "下载文件格式: $(if ($isZip) {'zip'} elseif ($isGzip) {'tar.gz'} else {'未知'})"
if ($isGzip) {
    if ([System.Environment]::OSVersion.Platform -Match "Unix") {
        tar xzf ./cache/TShock.zip --directory ./publish
    }
    else {
        throw "Windows 环境暂不支持 tar.gz 资产,请在 Unix 环境运行"
    }
}
else {
    Expand-Archive ./cache/TShock.zip -DestinationPath ./publish
}
if ([System.Environment]::OSVersion.Platform -Match "Unix") {
    $tarFile = Get-ChildItem ./publish -Filter *.tar -File | Select-Object -First 1
    if ($tarFile) {
        Write-Host "解压 TShock tar: $($tarFile.Name)"
        tar xvf $tarFile.FullName --directory ./publish
    }
    else {
        Write-Warning "publish 目录中未找到 .tar 文件,跳过 tar 解压(assets: $((Get-ChildItem ./publish | Select-Object -ExpandProperty Name) -Join ', '))"
    }
}

# Prepare plugin dlls
Copy-Item ./out/**/*.dll ./publish/ServerPlugins/
Copy-Item ./SubmoduleAssembly/*.dll ./publish/ServerPlugins/

# Prepare manifests
Set-Location $PSScriptRoot/../publish
New-Item -Name ./manifests -ItemType Directory -Force
foreach ($p in @(Get-ChildItem ../src/**/*.csproj)) {
    $manifestPath = Join-Path $p.DirectoryName manifest.json
    if (Test-Path $manifestPath -PathType Leaf) {
        Copy-Item $manifestPath $(Join-Path manifests "$($p.Basename).json")
    }
}
Copy-Item ../.config/submodule-manifests/* ./manifests

# Start generating plugin list
if (-not (Test-Path './TShock.Server' -PathType Leaf)) {
    throw "未找到 ./TShock.Server!publish 目录内容: $((Get-ChildItem ./publish | Select-Object -ExpandProperty Name) -Join ', ')"
}
Write-Host "启动 TShock.Server 生成插件列表..."
$proc = Start-Process -NoNewWindow -PassThru './TShock.Server' -ArgumentList '-dump-plugins-list-only','./manifests'
$proc | Wait-Process -Timeout 180 -ErrorAction SilentlyContinue -ErrorVariable timeouted
if ($timeouted) {
    $proc | Stop-Process -Force -ErrorAction SilentlyContinue
    throw "TShock.Server 超时(180s)!"
}
elseif ($proc.ExitCode -ne 0) {
    throw "TShock.Server 退出码: $($proc.ExitCode)"
}


if (-not $NoUpdateREADME) {
    & $PSScriptRoot/generate-readme.ps1
}

# Packing Plugins.zip
Set-Location $PSScriptRoot/..
Remove-Item ./out/Target -Recurse -Force -ProgressAction SilentlyContinue -ErrorAction Ignore
Remove-Item ./out/Plugins.zip -Recurse -Force -ProgressAction SilentlyContinue -ErrorAction Ignore
New-Item -Path ./out/Target -Name Plugins -ItemType Directory -Force
$ErrorActionPreference = "SilentlyContinue"
foreach ($p in @(Get-ChildItem src/**/*.csproj)) {             
    foreach ($r in @(Get-ChildItem "$($p.Directory)/README*.md")) {
        $ext_parts = ($r.Name -split '\.')
        $ext = $ext_parts[1..$ext_parts.Length] -join '.'
        Copy-Item $r "out/Target/Plugins/$($p.BaseName).$ext"
    }
}
$ErrorActionPreference = "Continue"
Copy-Item ./out/$BuildType/*.dll, ./out/$BuildType/*.pdb ./out/Target/Plugins/
Copy-Item ./SubmoduleAssembly/* ./out/Target/Plugins/
Copy-Item ./publish/Plugins.json, ./README*.md, ./Usage.txt, ./LICENSE ./out/Target/
# APM
New-Item -Path ./out/Target -Name Apm -ItemType Directory -Force
Copy-Item ./out/$BuildType/AutoPluginManager.* ./out/Target/Apm/

Compress-Archive -Path ./out/Target/* -DestinationPath ./out/Plugins.zip
