[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ApiBaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$UserName,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [string]$Email = "",

    [string]$DisplayName = "Backlogr Seed Bot",

    [string]$CatalogPath = "",

    [switch]$RegisterIfMissing,

    [switch]$PreviewOnly
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CatalogPath)) {
    $CatalogPath = Join-Path $PSScriptRoot "starter-catalog-titles.json"
}

if ([string]::IsNullOrWhiteSpace($Email)) {
    $Email = "$UserName@backlogr.local"
}

$baseUrl = $ApiBaseUrl.TrimEnd('/')

function Normalize-SeedTitle {
    param(
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    return (($Value.ToLowerInvariant()) -replace "[^a-z0-9]+", "").Trim()
}

function Invoke-BacklogrApi {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("GET", "POST")]
        [string]$Method,

        [Parameter(Mandatory = $true)]
        [string]$Uri,

        [object]$Body,

        [hashtable]$Headers
    )

    $params = @{
        Method = $Method
        Uri    = $Uri
    }

    if ($null -ne $Headers -and $Headers.Count -gt 0) {
        $params["Headers"] = $Headers
    }

    if ($PSBoundParameters.ContainsKey("Body")) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
        $params["ContentType"] = "application/json"
    }

    return Invoke-RestMethod @params
}

function Try-Login {
    param(
        [string]$LoginName,
        [string]$LoginPassword
    )

    $body = @{
        emailOrUserName = $LoginName
        password        = $LoginPassword
    }

    try {
        return Invoke-BacklogrApi `
            -Method POST `
            -Uri "$baseUrl/api/auth/login" `
            -Body $body
    }
    catch {
        return $null
    }
}

function Register-SeedUser {
    $body = @{
        userName    = $UserName
        displayName = $DisplayName
        email       = $Email
        password    = $Password
    }

    Write-Host "Registering seed user '$UserName'..." -ForegroundColor Yellow

    return Invoke-BacklogrApi `
        -Method POST `
        -Uri "$baseUrl/api/auth/register" `
        -Body $body
}

function Get-AuthResponse {
    $authResponse = Try-Login -LoginName $UserName -LoginPassword $Password

    if ($null -eq $authResponse -and -not [string]::IsNullOrWhiteSpace($Email)) {
        $authResponse = Try-Login -LoginName $Email -LoginPassword $Password
    }

    if ($null -ne $authResponse) {
        return $authResponse
    }

    if (-not $RegisterIfMissing) {
        throw "Unable to log in with the supplied credentials. Use -RegisterIfMissing to create the seed user first."
    }

    $registerResponse = Register-SeedUser

    if ($null -ne $registerResponse -and -not [string]::IsNullOrWhiteSpace($registerResponse.token)) {
        return $registerResponse
    }

    $authResponse = Try-Login -LoginName $UserName -LoginPassword $Password

    if ($null -eq $authResponse -and -not [string]::IsNullOrWhiteSpace($Email)) {
        $authResponse = Try-Login -LoginName $Email -LoginPassword $Password
    }

    if ($null -eq $authResponse) {
        throw "Seed user registration succeeded, but login still failed."
    }

    return $authResponse
}

function Resolve-SeedMatch {
    param(
        [object[]]$Results,
        [string]$Query,
        [string]$ExpectedTitle
    )

    $searchResults = @($Results)

    if ($searchResults.Count -eq 1 -and $searchResults[0] -is [System.Array]) {
        $searchResults = @($searchResults[0])
    }

    if ($searchResults.Count -eq 0) {
        return $null
    }

    $expectedNorm = Normalize-SeedTitle -Value $ExpectedTitle
    $queryNorm = Normalize-SeedTitle -Value $Query

    $exactExpected = @(
        $searchResults | Where-Object {
            (Normalize-SeedTitle -Value ([string]$_.title)) -eq $expectedNorm
        }
    )

    if ($exactExpected.Count -gt 0) {
        return $exactExpected[0]
    }

    $exactQuery = @(
        $searchResults | Where-Object {
            (Normalize-SeedTitle -Value ([string]$_.title)) -eq $queryNorm
        }
    )

    if ($exactQuery.Count -gt 0) {
        return $exactQuery[0]
    }

    $partialExpected = @(
        $searchResults | Where-Object {
            $resultNorm = Normalize-SeedTitle -Value ([string]$_.title)
            $resultNorm.Contains($expectedNorm) -or $expectedNorm.Contains($resultNorm)
        }
    )

    if ($partialExpected.Count -gt 0) {
        return $partialExpected[0]
    }

    return $searchResults[0]
}

if (-not (Test-Path -Path $CatalogPath)) {
    throw "Catalog file not found: $CatalogPath"
}

$catalog = @((Get-Content -Path $CatalogPath -Raw) | ConvertFrom-Json)

if ($catalog.Count -eq 0) {
    throw "Catalog file is empty."
}

$authResponse = Get-AuthResponse

if ([string]::IsNullOrWhiteSpace($authResponse.token)) {
    throw "Authentication succeeded, but no JWT token was returned."
}

$authHeaders = @{
    Authorization = "Bearer $($authResponse.token)"
}

$summary = [ordered]@{
    Previewed = 0
    Ensured   = 0
    Failed    = 0
}

Write-Host ""
Write-Host "Starting starter catalog seed against $baseUrl" -ForegroundColor Cyan
Write-Host "Catalog entries: $($catalog.Count)" -ForegroundColor Cyan
Write-Host ""

foreach ($entry in $catalog) {
    $query = [string]$entry.query
    $expectedTitle = [string]$entry.expectedTitle

    if ([string]::IsNullOrWhiteSpace($query)) {
        continue
    }

    if ([string]::IsNullOrWhiteSpace($expectedTitle)) {
        $expectedTitle = $query
    }

    Write-Host "Searching IGDB for: $query" -ForegroundColor White

    try {
        $encodedQuery = [System.Uri]::EscapeDataString($query)

        $searchResults = Invoke-BacklogrApi `
            -Method GET `
            -Uri "$baseUrl/api/igdb/search?query=$encodedQuery&take=10" `
            -Headers $authHeaders

        $searchResults = @($searchResults)

        if ($searchResults.Count -eq 1 -and $searchResults[0] -is [System.Array]) {
            $searchResults = @($searchResults[0])
        }

        if ($searchResults.Count -eq 0) {
            Write-Warning "No IGDB matches found for '$query'."
            $summary["Failed"]++
            Start-Sleep -Milliseconds 300
            continue
        }

        $selected = Resolve-SeedMatch `
            -Results $searchResults `
            -Query $query `
            -ExpectedTitle $expectedTitle

        if ($null -eq $selected) {
            Write-Warning "Could not resolve a match for '$query'."
            $summary["Failed"]++
            Start-Sleep -Milliseconds 300
            continue
        }

        Write-Host (" -> Selected: {0} (IGDB {1})" -f ([string]$selected.title), ([string]$selected.igdbId)) -ForegroundColor DarkGray

        if ($PreviewOnly) {
            $summary["Previewed"]++
            Start-Sleep -Milliseconds 300
            continue
        }

        $importResult = Invoke-BacklogrApi `
            -Method POST `
            -Uri "$baseUrl/api/igdb/import/$($selected.igdbId)" `
            -Headers $authHeaders

        Write-Host ("    Ensured local game: {0} ({1})" -f ([string]$importResult.title), ([string]$importResult.gameId)) -ForegroundColor Green
        $summary["Ensured"]++
    }
    catch {
        Write-Warning ("Failed for '{0}': {1}" -f $query, $_.Exception.Message)
        $summary["Failed"]++
    }

    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "Seed summary" -ForegroundColor Cyan
$summary.GetEnumerator() | ForEach-Object {
    Write-Host (" - {0}: {1}" -f $_.Key, $_.Value)
}
Write-Host ""