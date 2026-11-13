$dumpFile   = "C:\Users\golam\Downloads\eimsdb_noble.dump"
$container  = "eims-postgres"
$port       = 5433
$password   = "12345AsD,./"
$dbName     = "eimsdb_noble"
$dbUser     = "ghp"
$volume     = "eims-postgres-data"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker not found. Install Docker Desktop first."
    exit 1
}

# --- Container setup ---
$exists = docker ps -a --filter "name=$container" --format "{{.Names}}" | Out-String
$exists = $exists.Trim()

if ($exists -eq $container) {
    $running = docker ps --filter "name=$container" --filter "status=running" --format "{{.Names}}" | Out-String
    if ($running.Trim() -ne $container) { docker start $container }
    Write-Host "Container '$container' ready."
}
else {
    Write-Host "Creating PostgreSQL container..."
    docker run -d --name $container -p ${port}:5432 `
        -e POSTGRES_PASSWORD=$password -v ${volume}:/var/lib/postgresql/data postgres:16
}

# --- Wait for readiness ---
Write-Host "Waiting for PostgreSQL..."
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    if ((docker exec $container pg_isready -U postgres) -match "accepting") { $ready = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $ready) { Write-Error "PostgreSQL not ready."; exit 1 }

# --- Create user ---
docker exec $container psql -U postgres -c "SELECT 'CREATE ROLE $dbUser LOGIN PASSWORD ''$password''' WHERE NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$dbUser')\gexec"

# --- Create database ---
docker exec $container psql -U postgres -c "SELECT 'CREATE DATABASE $dbName OWNER $dbUser' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$dbName')\gexec"

# --- Restore ---
Write-Host "Copying dump & restoring..."
docker cp $dumpFile "${container}:/tmp/eims_dump.dump"
docker exec $container pg_restore -U postgres -d $dbName -F c --clean --if-exists /tmp/eims_dump.dump

Write-Host "`nDone! Database '$dbName' ready at localhost:$port"
Write-Host "Conn: Host=127.0.0.1;Port=$port;Database=$dbName;Username=$dbUser;Password=$password"