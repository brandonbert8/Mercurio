ALTER TABLE Negocios ADD COLUMN ImpuestoPorcentaje REAL NOT NULL DEFAULT 13;
ALTER TABLE Negocios ADD COLUMN PrecioIncluyeImpuesto INTEGER NOT NULL DEFAULT 1;

UPDATE Negocios SET Moneda = 'BOB' WHERE Moneda = 'PEN';
UPDATE Negocios SET ZonaHoraria = 'America/La_Paz' WHERE ZonaHoraria = 'America/Lima';
