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

INSERT INTO code_group
	(group_code, group_name)
VALUES
	('equipment_status', '설비 상태'),
	('lot_status', 'lot 상태'),
	('production_result', '생산 상태'),
	('work_order_status', '작업지시 상태'),
	('defect_code', '불량 코드'),
	('downtime_reason', '중단 원인')
ON CONFLICT (group_code) DO NOTHING
;

INSERT INTO common_code
	(group_code, code, code_name, sort_order)
VALUES
	('equipment_status', 'idle', '대기중', 0),
	('equipment_status', 'run', '작동중', 1),
	('equipment_status', 'complete', '완료', 2),
	('equipment_status', 'error', '문제발생', 3)
ON CONFLICT (group_code, code) DO NOTHING
;

INSERT INTO equipment
	(equip_id, name, location, status, is_active)
VALUES
	('EQ26000001', '사출성형기 1호기', '1공장 A라인', 'run', TRUE),
	('EQ26000002', '사출성형기 2호기', '1공장 A라인', 'idle', TRUE),
	('EQ26000003', '비전검사기 1호기', '1공장 A라인', 'run', TRUE),
	('EQ26000004', '컨베이어 1호기', '1공장 A라인', 'run', TRUE),
	('EQ26000005', '포장기 1호기', '1공장 B라인', 'idle', TRUE),
	('EQ26000006', '사출성형기 3호기', '2공장 A라인', 'error', TRUE),
	('EQ26000007', '구형 프레스기', '2공장 C라인', 'idle', FALSE)
ON CONFLICT (equip_id) DO NOTHING;

UPDATE primary_sequence
	SET current_val = 7
WHERE table_name = 'equipment'
	AND years = TO_CHAR(CURRENT_DATE, 'YY');
