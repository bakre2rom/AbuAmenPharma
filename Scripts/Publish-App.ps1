param(
    [switch]$WebDeploy,
    [string]$PublishDir = "$env:USERPROFILE\Desktop\Abuamna"
)

$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectFile = Join-Path $projectRoot "AbuAmenPharma.csproj"

Push-Location $projectRoot
try {
    Write-Host "Building Release output..."
    dotnet build $projectFile -c Release
    if ($LASTEXITCODE -ne 0) { throw "Release build failed." }

    Write-Host "Applying pending EF Core migrations..."
    dotnet ef database update --project $projectFile --startup-project $projectFile --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) { throw "Database update failed." }

    if ($WebDeploy) {
        Write-Host "Publishing with Web Deploy profile..."
        dotnet publish $projectFile -c Release --no-build /p:PublishProfile=site61074-WebDeploy
    }
    else {
        $resolvedPublishDir = [System.IO.Path]::GetFullPath($PublishDir)
        if (-not (Test-Path $resolvedPublishDir)) {
            New-Item -ItemType Directory -Path $resolvedPublishDir | Out-Null
        }

        Write-Host "Publishing to $resolvedPublishDir ..."
        dotnet publish $projectFile -c Release --no-build /p:PublishProfile=FolderProfile /p:PublishDir="$resolvedPublishDir\"
    }

    if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
}
finally {
    Pop-Location
}
