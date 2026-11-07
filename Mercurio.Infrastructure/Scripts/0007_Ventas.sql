CREATE TABLE IF NOT EXISTS Ventas (
    Id TEXT PRIMARY KEY,
    SucursalId TEXT NOT NULL,
    AlmacenId TEXT NOT NULL,
    UsuarioId TEXT NOT NULL,
    ClienteId TEXT NULL,
    CajaSesionId TEXT NULL,
    NumeroDocumento TEXT NOT NULL DEFAULT '',
    TipoDocumento INTEGER NOT NULL DEFAULT 1,
    Fecha TEXT NOT NULL,
    SubTotal REAL NOT NULL DEFAULT 0,
    Descuento REAL NOT NULL DEFAULT 0,
    Impuesto REAL NOT NULL DEFAULT 0,
    Total REAL NOT NULL DEFAULT 0,
    Estado INTEGER NOT NULL DEFAULT 1,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS VentaDetalles (
    Id TEXT PRIMARY KEY,
    VentaId TEXT NOT NULL,
    ProductoId TEXT NOT NULL,
    ProductoNombre TEXT NOT NULL DEFAULT '',
    Cantidad INTEGER NOT NULL,
    PrecioUnitario REAL NOT NULL DEFAULT 0,
    Descuento REAL NOT NULL DEFAULT 0,
    ImpuestoPorcentaje REAL NOT NULL DEFAULT 0,
    SubTotal REAL NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS IX_Ventas_SucursalFecha ON Ventas (SucursalId, Fecha);
CREATE INDEX IF NOT EXISTS IX_Ventas_Cliente ON Ventas (ClienteId);
CREATE INDEX IF NOT EXISTS IX_VentaDetalles_Venta ON VentaDetalles (VentaId);
CREATE INDEX IF NOT EXISTS IX_VentaDetalles_Producto ON VentaDetalles (ProductoId);
