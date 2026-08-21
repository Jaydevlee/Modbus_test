SELECT * FROM code_group;
SELECT * FROM common_code;

SELECT
	code,
	code_name
FROM common_code
WHERE group_code = 'equipment_status'
	AND is_active = TRUE
ORDER BY sort_order;