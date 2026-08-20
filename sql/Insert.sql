INSERT INTO PRIMARY_SEQUENCE
	(table_name, prefix, current_val)
VALUES
	('product', 'PD', 0),
	('equipment', 'EQ', 0),
	('work_order', 'WO', 0),
	('lot', 'LOT', 0),
	('production_result', 'RS', 0),
	('equip_downtime', 'DT', 0),
	('quality_defect', 'QD', 0),
	('users', 'US', 0)
ON CONFLICT (table_name, years) DO NOTHING;