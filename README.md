# Datalake indexer stuff

## todo diagram

### rescan

- list paths and get files with etag
- upsert paths into paths table and figure out which are new or have a modified etag
- fetch metadata for files returned in previous step
- upsert metadata for new or modified files
- update etag for new or modified files

### event

- receive blob created events in batch
- fetch metadata for paths received
- upsert paths and metadata
- update etag

## Projects

### DatabaseIndexer

Contains the project for upserting paths and metadata into the database

### DatabaseIndexerDatabase

EntityFramework migrations and database definition

### IndexerRunner

Console application for testing the indexer with various sources

### IndexerWorker

Console application for receiving blob created events and.. do stuff
