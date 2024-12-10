source .env

dotnet ef migrations script \
    --idempotent \
    --output db_migration.sql