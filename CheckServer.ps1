param(
    [string]$OutputFile = ""
)

$servers = @("(local)", ".\SQLEXPRESS")

foreach ($server in $servers) {

    Write-Host "Checking $server"

    $version = & sqlcmd -S $server -E -C -h -1 -W -b `
        -Q "SET NOCOUNT ON; SELECT SERVERPROPERTY('ProductMajorVersion')"

    if ($LASTEXITCODE -ne 0) {
        continue
    }

    $version = ($version | Out-String).Trim()

    if ($version -in @("15", "16")) {

        Write-Host "Found SQL Server: $server"
        Write-Host "Version: $version"

        if ($OutputFile -ne "") {
            Set-Content -LiteralPath $OutputFile `
                -Value $server -Encoding ASCII
        }

        exit 0
    }
}

Write-Host "SQL Server 2019 or 2022 not found"
exit 1