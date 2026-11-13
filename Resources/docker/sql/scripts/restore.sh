#!/bin/bash

DB_NAME="EIMS"
BACKUP_DIR="/var/backups"

echo "⏳ Waiting for SQL Server to be ready..."

# Wait until SQL is actually ready (better than fixed sleep)
for i in {1..30}; do
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -Q "SELECT 1" &> /dev/null
  if [ $? -eq 0 ]; then
    echo "✅ SQL Server is ready!"
    break
  fi
  echo "Waiting... ($i)"
  sleep 2
done

# Find latest .bak file
LATEST_BAK=$(ls -t $BACKUP_DIR/*.bak 2>/dev/null | head -n 1)

if [ -z "$LATEST_BAK" ]; then
  echo "❌ No .bak file found in $BACKUP_DIR"
  exit 1
fi

echo "📦 Latest backup found: $LATEST_BAK"

echo "🔄 Restoring database $DB_NAME..."

# Drop DB if exists
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" \
-Q "IF DB_ID('$DB_NAME') IS NOT NULL BEGIN ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$DB_NAME]; END"

# Get logical names dynamically
LOGICAL_NAMES=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" \
-Q "RESTORE FILELISTONLY FROM DISK='$LATEST_BAK'" -s "," -W | tail -n +3)

DATA_NAME=$(echo "$LOGICAL_NAMES" | head -n 1 | cut -d',' -f1)
LOG_NAME=$(echo "$LOGICAL_NAMES" | tail -n 1 | cut -d',' -f1)

echo "📄 Data file: $DATA_NAME"
echo "📄 Log file: $LOG_NAME"

# Restore DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" \
-Q "RESTORE DATABASE [$DB_NAME] FROM DISK='$LATEST_BAK' WITH REPLACE, MOVE '$DATA_NAME' TO '/var/opt/mssql/data/${DB_NAME}.mdf', MOVE '$LOG_NAME' TO '/var/opt/mssql/data/${DB_NAME}_log.ldf'"

echo "🎉 Database restored successfully!"