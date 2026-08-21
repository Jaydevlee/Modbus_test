CREATE TABLE IF NOT EXISTS equipment_metric (
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

CREATE INDEX IF NOT EXISTS ix_metric_equip_name_time
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

CREATE TABLE IF NOT EXISTS equip_downtime (
	downtime_id   text        PRIMARY KEY,
	equip_id      text        NOT NULL,
	reason_code   text        NOT NULL,
	started_at    timestamptz NOT NULL DEFAULT now(),
	ended_at      timestamptz,
	note          text
);

CREATE INDEX IF NOT EXISTS ix_equip_downtime_equip_id
	ON equip_downtime(equip_id);

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

CREATE TABLE IF NOT EXISTS quality_defect (
	defect_id     text        PRIMARY KEY,
	result_id     text        NOT NULL,
	defect_code   text        NOT NULL,
	defect_type   text,
	detected_at   timestamptz NOT NULL DEFAULT now(),
	note          text
);

CREATE INDEX IF NOT EXISTS ix_quality_defect_result_id
	ON quality_defect(result_id);

CREATE TABLE IF NOT EXISTS authority (
	authority_id   text    PRIMARY KEY,
	name           text    NOT NULL,
	description    text
);

CREATE TABLE IF NOT EXISTS users (
	user_id        text        PRIMARY KEY,
	username       text        NOT NULL UNIQUE,
	password_hash  text        NOT NULL,
	full_name      text,
	authority_id   text        NOT NULL,
	is_active      boolean     NOT NULL DEFAULT TRUE,
	created_at     timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_users_authority_id
	ON users(authority_id);

CREATE TABLE IF NOT EXISTS primary_sequence (
	table_name	 text	 NOT NULL,
	prefix 		 text	 NOT NULL,
	years        text    DEFAULT TO_CHAR(CURRENT_DATE, 'YY'),
	current_val	 bigint  NOT NULL DEFAULT 0,
	CONSTRAINT pk_primary_seq PRIMARY KEY(table_name, years)
);

CREATE TABLE IF NOT EXISTS code_group(
	group_code		text		PRIMARY KEY,
	group_name		text		NOT NULL,
	description		text,
	is_active		boolean		NOT NULL DEFAULT TRUE,
	created_at		timestamptz  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS common_code(
	group_code		text		NOT NULL,
	code			text		NOT NULL,
	code_name		text		NOT NULL,
	description		text,
	sort_order		int			NOT NULL DEFAULT 0,
	is_active		boolean 	NOT NULL DEFAULT TRUE,
	created_at		timestamptz NOT NULL DEFAULT now(),
	PRIMARY KEY(group_code, code)
);

CREATE INDEX IF NOT EXISTS ix_common_code_group
	ON common_code (group_code, sort_order);

SELECT * FROM equipment_metric
ORDER BY TIME DESC;
SELECT * FROM PRIMARY_SEQUENCE;

SELECT * FROM PRODUCT;
SHOW timezone;

SELECT * FROM product;
