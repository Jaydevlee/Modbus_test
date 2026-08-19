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

CREATE TABLE IF NOT EXISTS product  (
	product_id	        text    PRIMARY KEY,
	name                text	  NOT NULL,
	recipe_version      text      NOT NULL,
	is_active			boolean	  NOT NULL DEFAULT TRUE 
);

CREATE TABLE IF NOT EXISTS equipment (
	equip_id    text      PRIMARY KEY,
	name		text	  NOT NULL,
	location	text	  NOT NULL,
	status      text      NOT  NULL,
	is_active   boolean	  NOT NULL DEFAULT TRUE 
);

CREATE TABLE IF NOT EXISTS work_order(
	work_order_id     text    	PRIMARY KEY,
	product_id 	      text 		NOT NULL,
	target_quantity   int	 	NOT NULL,
	status 			  text	 	NOT NULL,
	planned_at		  timestamptz,
	started_at		  timestamptz,
	completed_at      timestamptz,
	created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_work_order_product_id
	ON work_order(product_id);

CREATE TABLE IF NOT EXISTS lot (
	lot_id		   text			PRIMARY KEY,
	work_order_id  text  		NOT NULL,
	equip_id       text   		NOT NULL,
	status		   text			NOT NULL,
	start_at       timestamptz  NOT NULL DEFAULT NOW(),
	end_at         timestamptz  
);

CREATE INDEX IF NOT EXISTS ix_lot_work_order_id
    ON lot(work_order_id);
CREATE INDEX IF NOT EXISTS ix_lot_equip_id
    ON lot(equip_id);
CREATE INDEX IF NOT EXISTS ix_lot_work_order_equip_id
	ON lot(work_order_id, equip_id);

CREATE TABLE IF NOT EXISTS production_result (
	result_id	text		PRIMARY KEY,
	lot_id      text		NOT NULL,
	result      text		NOT NULL,
	cycle_time  double PRECISION,
	produced_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_production_result_lot_id
	ON production_result(lot_id);

CREATE TABLE IF NOT EXISTS primary_sequence (
	table_name	 text	 NOT NULL,
	prefix 		 text	 NOT NULL,
	years        text    DEFAULT TO_CHAR(CURRENT_DATE, 'YY'),
	current_val	 bigint  NOT NULL DEFAULT 0,
	CONSTRAINT pk_primary_seq PRIMARY KEY(table_name, years)
);

SELECT * FROM equipment_metric;
SELECT * FROM PRIMARY_SEQUENCE;
TRUNCATE table primary_sequence;
INSERT INTO PRIMARY_SEQUENCE 
	(table_name, prefix, current_val)
VALUES
	('product', 'PD', 0),
	('equipment', 'EQ', 0),
	('work_order', 'WO', 0),
	('lot', 'LOT', 0),
	('production_result', 'RS', 0)
ON CONFLICT (table_name, years) DO NOTHING;
SELECT * FROM PRODUCT;
SHOW timezone;