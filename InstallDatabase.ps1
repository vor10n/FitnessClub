param(
    [string]$Server = "(local)",
    [string]$SqlFile = "C:\DBBackup\FitnessClub.sql",
    [string]$ServerFile = ""
)

$serverFile = $ServerFile

if (Test-Path $serverFile) {
    $Server = (Get-Content $serverFile -Raw).Trim()
}
else {
    Write-Host "Server configuration not found"
    exit 1
}

# Проверяем подключение и существование базы
$query = "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'FitnessClub') IS NULL THEN 0 ELSE 1 END"

$result = & sqlcmd -S $Server -E -C -h -1 -W -b -Q $query

if ($LASTEXITCODE -ne 0) {
    Write-Host "SQL Server connection failed"
    exit 1
}

$exists = ($result | Out-String).Trim()

if ($exists -eq "1") {
    Write-Host "Database FitnessClub already exists"
    exit 0
}

if ($exists -ne "0") {
    Write-Host "Unexpected database check result"
    exit 1
}

# Проверяем SQL-файл
if (-not (Test-Path -LiteralPath $SqlFile)) {
    Write-Host "SQL file not found"
    exit 1
}

# Создаём базу, таблицы и данные
Write-Host "Installing FitnessClub database..."

& sqlcmd -S $Server -d master -E -C -b -i $SqlFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Database installation failed"
    exit 1
}

Write-Host "Database FitnessClub installed successfully"
exit 0