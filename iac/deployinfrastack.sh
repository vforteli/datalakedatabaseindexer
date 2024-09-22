az stack group create \
  --name 'databaseindexer' \
  --resource-group 'database-indexer-rg' \
  --template-file 'main.bicep' \
  --deny-settings-mode 'none' \
  --action-on-unmanage 'deleteResources'