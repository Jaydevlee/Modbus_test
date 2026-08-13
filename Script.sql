CREATE TABLE equipment_metric (
    time          timestamptz      NOT NULL,
    equip_id      text             NOT NULL,
    address       text             NOT NULL,
    metric_name   text             NOT NULL,
    metric_value  double precision NOT NULL,
    unit          text,
    quality       smallint         NOT NULL DEFAULT 192,
    source_time   timestamptz,
    collected_at  timestamptz      NOT NULL DEFAULT now()
);

SELECT create_hypertable(
    'equipment_metric',
    by_range('time'),
    if_not_exists => TRUE
);

CREATE INDEX ix_metric_equip_name_time
    ON equipment_metric (equip_id, metric_name, time DESC);

SELECT
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'equipment_metric'
ORDER BY ordinal_position;


select * from equipment_metric;

CREATE TABLE IF NOT EXISTS product  (
	id	                int       PRIMARY KEY,
	name                text	  NOT NULL,
	recipe_version      text      NOT NULL 
);

CREATE TABLE IF NOT EXISTS equipment (

)
