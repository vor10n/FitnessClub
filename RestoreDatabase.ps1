param(
    [string]$Server = "(local)",
    [string]$Backup = "C:\DBBackup\FitnessClub.bak"
)

$ErrorActionPreference = "Stop"

# Проверяем SQL Server
$version = & sqlcmd -S $Server -E -C -h -1 -W -b `
    -Q "SET NOCOUNT ON; SELECT SERVERPROPERTY('ProductMajorVersion')"

if ($LASTEXITCODE -ne 0) {
    Write-Host "SQL Server not found"
    exit 1
}

$version = ($version | Out-String).Trim()

if ($version -notin @("15", "16")) {
    Write-Host "Only SQL Server 2019 and 2022 are supported"
    exit 1
}

Write-Host "SQL Server version: $version"

# Проверяем базу
$query = "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'FitnessClub'"

$result = & sqlcmd -S $Server -E -C -h -1 -W -b -Q $query

if ($LASTEXITCODE -ne 0) {
    Write-Host "Database check failed"
    exit 1
}

if (($result | Out-String).Trim() -eq "1") {
    Write-Host "Database FitnessClub already exists"
    exit 0
}

# Проверяем резервную копию
if (-not (Test-Path -LiteralPath $Backup)) {
    Write-Host "Backup file not found"
    exit 1
}

# Получаем папки SQL Server
$paths = & sqlcmd -S $Server -E -C -h -1 -W -s "|" -b `
    -Q "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000)), CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000))"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Cannot determine SQL Server paths"
    exit 1
}

$parts = ($paths | Out-String).Trim() -split '\|'

if ($parts.Count -ne 2 -or
    [string]::IsNullOrWhiteSpace($parts[0]) -or
    [string]::IsNullOrWhiteSpace($parts[1])) {
    Write-Host "Invalid SQL Server paths"
    exit 1
}

$dataPath = Join-Path $parts[0].Trim() "FitnessClub.mdf"
$logPath = Join-Path $parts[1].Trim() "FitnessClub_log.ldf"

$b = $Backup.Replace("'", "''")
$d = $dataPath.Replace("'", "''")
$l = $logPath.Replace("'", "''")

# Восстанавливаем базу
$sql = @"
IF DB_ID(N'FitnessClub') IS NULL
BEGIN
    RESTORE DATABASE [FitnessClub]
    FROM DISK = N'$b'
    WITH
        MOVE N'FitnessClub' TO N'$d',
        MOVE N'FitnessClub_log' TO N'$l',
        RECOVERY;
END
"@

& sqlcmd -S $Server -d master -E -C -b -Q $sql

if ($LASTEXITCODE -ne 0) {
    Write-Host "Database restore failed"
    exit 1
}

Write-Host "Database FitnessClub restored successfully"
exit 0