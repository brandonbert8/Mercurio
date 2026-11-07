CREATE TABLE IF NOT EXISTS Almacenes (
    Id TEXT PRIMARY KEY,
    SucursalId TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    Codigo TEXT NOT NULL DEFAULT '',
    Direccion TEXT NOT NULL DEFAULT '',
    Activo INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS StockAlmacen (
    Id TEXT PRIMARY KEY,
    AlmacenId TEXT NOT NULL,
    ProductoId TEXT NOT NULL,
    Cantidad INTEGER NOT NULL DEFAULT 0,
    StockMinimo INTEGER NOT NULL DEFAULT 0,
    FechaActualizacion TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_StockAlmacen_AlmacenProducto ON StockAlmacen (AlmacenId, ProductoId);
CREATE INDEX IF NOT EXISTS IX_Almacenes_Sucursal ON Almacenes (SucursalId);

CREATE TABLE IF NOT EXISTS MovimientosInventario (
    Id TEXT PRIMARY KEY,
    AlmacenId TEXT NOT NULL,
    AlmacenDestinoId TEXT NULL,
    ProductoId TEXT NOT NULL,
    Tipo INTEGER NOT NULL,
    Cantidad INTEGER NOT NULL,
    CostoUnitario REAL NOT NULL DEFAULT 0,
    Referencia TEXT NOT NULL DEFAULT '',
    Observacion TEXT NOT NULL DEFAULT '',
    UsuarioId TEXT NULL,
    Fecha TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_MovimientosInventario_AlmacenFecha ON MovimientosInventario (AlmacenId, Fecha);
CREATE INDEX IF NOT EXISTS IX_MovimientosInventario_Producto ON MovimientosInventario (ProductoId);
