source .env

dotnet ef dbcontext scaffold \
    "Data Source=localhost;Initial Catalog=datalakeindexer;User ID=sa;Password=$dbsecret;TrustServerCertificate=true" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir GeneratedDatabase \
    --project DatabaseIndexerDatabase \
    --force \
    --no-onconfiguring