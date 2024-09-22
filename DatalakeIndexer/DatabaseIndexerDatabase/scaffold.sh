source .env

dotnet ef dbcontext scaffold \
    "Data Source=localhost,1434;Initial Catalog=testdb;User ID=sa;Password=$dbsecret;TrustServerCertificate=true" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir GeneratedDatabase \
    --force \
    --no-onconfiguring \
    --no-pluralize \
    --use-database-names \
    --context DatalakeindexerContext