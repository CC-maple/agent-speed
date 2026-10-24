#requires -Version 7.0
<#
.SYNOPSIS
Updates one currency in Agent Speed's local USD-based exchange rate file without network access.
.DESCRIPTION
Agent Speed reads exchange rates once at startup. After updating this file, fully exit and restart Agent Speed.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Z]{3}$')][string] $Currency,
    [Parameter(Mandatory)][ValidateRange(0.000000000001, 1000000000000)][decimal] $Rate,
    [string] $Date,
    [string] $RatesFile
)
$ErrorActionPreference = 'Stop'
if (-not $RatesFile) { $RatesFile = $env:AGENT_SPEED_RATES_FILE }
if (-not $RatesFile) { $RatesFile = Join-Path $env:LOCALAPPDATA 'AgentSpeed\rates\local-rates.json' }
if (-not [IO.Path]::IsPathFullyQualified($RatesFile) -or $RatesFile.StartsWith('\\')) {
    throw 'RatesFile must be an absolute local path.'
}
if ($Currency -eq 'USD' -and $Rate -ne 1) { throw 'USD must equal 1.' }
$catalog = Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\..\spec\assets\currencies.json') -Raw | ConvertFrom-Json -AsHashtable
if (-not $catalog.fallbackRates.rates.ContainsKey($Currency)) { throw "Unsupported currency: $Currency" }
if (Test-Path -LiteralPath $RatesFile) {
    $data = Get-Content -LiteralPath $RatesFile -Raw | ConvertFrom-Json -AsHashtable
} else {
    $data = [ordered]@{ base = 'USD'; date = $catalog.fallbackRates.date; rates = $catalog.fallbackRates.rates }
}
if ($data.base -ne 'USD' -or $data.rates.USD -ne 1) { throw 'The existing file must use USD as its base with USD=1.' }
foreach ($code in $catalog.fallbackRates.rates.Keys) {
    if (-not $data.rates.ContainsKey($code) -or [decimal]$data.rates[$code] -le 0 -or [decimal]$data.rates[$code] -gt 1000000000000) {
        throw "Missing or invalid rate: $code"
    }
}
if (-not $Date) { $Date = $data.date }
$parsedDate = [DateOnly]::ParseExact($Date, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
$data.date = $parsedDate.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
$data.rates[$Currency] = $Rate
$directory = Split-Path -Parent $RatesFile
[IO.Directory]::CreateDirectory($directory) | Out-Null
$json = $data | ConvertTo-Json -Depth 5
# Exclusive write prevents readers from accepting a partially written document.
$stream = [IO.File]::Open($RatesFile, [IO.FileMode]::Create, [IO.FileAccess]::Write, [IO.FileShare]::None)
try {
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($json)
    $stream.Write($bytes, 0, $bytes.Length)
    $stream.Flush($true)
} finally { $stream.Dispose() }
[pscustomobject]@{ File = $RatesFile; Base = 'USD'; Currency = $Currency; Rate = $Rate; Date = $data.date; RestartRequired = $true }
