-- Table: public.paths

-- DROP TABLE IF EXISTS public.paths;

CREATE TABLE
IF NOT EXISTS public.paths
(
    path_key bytea NOT NULL,
    filesystem_name character varying
(255) COLLATE pg_catalog."default" NOT NULL,
    path character varying
(1024) COLLATE pg_catalog."default" NOT NULL,
    path_reversed character varying
(1024) COLLATE pg_catalog."default" GENERATED ALWAYS AS
(reverse
((path)::text)) STORED,
    created_on timestamp
with time zone,
    last_modified timestamp
with time zone,
    deleted_on timestamp
with time zone,
    etag character varying
(20) COLLATE pg_catalog."default",
    CONSTRAINT paths_pk PRIMARY KEY
(path_key)
)

WITH
(
    autovacuum_enabled = TRUE
)
TABLESPACE pg_default;

ALTER TABLE
IF EXISTS public.paths
    OWNER to myuser;
-- Index: filesystem_path_unique

-- DROP INDEX IF EXISTS public.filesystem_path_unique;

CREATE INDEX
IF NOT EXISTS filesystem_path_unique
    ON public.paths USING btree
(filesystem_name COLLATE pg_catalog."default" ASC NULLS LAST, path COLLATE pg_catalog."default" ASC NULLS LAST)
WITH
(deduplicate_items=True)
    TABLESPACE pg_default;

ALTER TABLE
IF EXISTS public.paths
    CLUSTER ON filesystem_path_unique;



-- Table: public.paths_metadata

-- DROP TABLE IF EXISTS public.paths_metadata;

CREATE TABLE
IF NOT EXISTS public.paths_metadata
(
    path_key bytea NOT NULL,
    metadata_json jsonb,
    CONSTRAINT paths_metadata_pk PRIMARY KEY
(path_key)
)

TABLESPACE pg_default;

ALTER TABLE
IF EXISTS public.paths_metadata
    OWNER to myuser;