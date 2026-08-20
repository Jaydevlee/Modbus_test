UPDATE  product
	SET is_active = false
WHERE name = 'test3';

UPDATE primary_sequence
	SET current_val = 0
WHERE table_name = 'product';