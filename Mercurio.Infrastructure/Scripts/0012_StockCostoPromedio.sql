ALTER TABLE StockAlmacen ADD COLUMN CostoPromedio REAL NOT NULL DEFAULT 0;

CREATE INDEX IF NOT EXISTS IX_StockAlmacen_Producto ON StockAlmacen (ProductoId);

-- StockAlmacen es la fuente de verdad: reconstruir el stock denormalizado del producto.
UPDATE Productos
SET StockActual = COALESCE((
    SELECT SUM(s.Cantidad)
    FROM StockAlmacen s
    WHERE s.ProductoId = Productos.Id
), 0);

CREATE TRIGGER IF NOT EXISTS TR_StockAlmacen_NoNegativo_Insert
BEFORE INSERT ON StockAlmacen
WHEN NEW.Cantidad < 0
BEGIN
    SELECT RAISE(ABORT, 'El stock no puede ser negativo.');
END;

CREATE TRIGGER IF NOT EXISTS TR_StockAlmacen_NoNegativo_Update
BEFORE UPDATE OF Cantidad ON StockAlmacen
WHEN NEW.Cantidad < 0
BEGIN
    SELECT RAISE(ABORT, 'El stock no puede ser negativo.');
END;

CREATE TRIGGER IF NOT EXISTS TR_StockAlmacen_SyncStock_Insert
AFTER INSERT ON StockAlmacen
BEGIN
    UPDATE Productos
    SET StockActual = StockActual + NEW.Cantidad
    WHERE Id = NEW.ProductoId;
END;

CREATE TRIGGER IF NOT EXISTS TR_StockAlmacen_SyncStock_Update
AFTER UPDATE OF Cantidad ON StockAlmacen
BEGIN
    UPDATE Productos
    SET StockActual = StockActual - OLD.Cantidad + NEW.Cantidad
    WHERE Id = NEW.ProductoId;
END;

CREATE TRIGGER IF NOT EXISTS TR_StockAlmacen_SyncStock_Delete
AFTER DELETE ON StockAlmacen
BEGIN
    UPDATE Productos
    SET StockActual = StockActual - OLD.Cantidad
    WHERE Id = OLD.ProductoId;
END;
