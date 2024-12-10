source .env

dotnet ef dbcontext scaffold \
    "$connectionstring" \
    Npgsql.EntityFrameworkCore.PostgreSQL \
    --output-dir GeneratedDatabase \
    --force \
    --no-onconfiguring \
    --no-pluralize \
    --use-database-names \
    --context PgIndexerContext