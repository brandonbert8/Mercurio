CREATE TABLE IF NOT EXISTS Proveedores (
    Id TEXT PRIMARY KEY,
    NegocioId TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    Ruc TEXT NOT NULL DEFAULT '',
    Contacto TEXT NOT NULL DEFAULT '',
    Telefono TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Direccion TEXT NOT NULL DEFAULT '',
    Activo INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Compras (
    Id TEXT PRIMARY KEY,
    SucursalId TEXT NOT NULL,
    ProveedorId TEXT NOT NULL,
    AlmacenId TEXT NOT NULL,
    UsuarioId TEXT NOT NULL,
    NumeroDocumento TEXT NOT NULL DEFAULT '',
    TipoDocumento INTEGER NOT NULL DEFAULT 5,
    Fecha TEXT NOT NULL,
    SubTotal REAL NOT NULL DEFAULT 0,
    Impuesto REAL NOT NULL DEFAULT 0,
    Total REAL NOT NULL DEFAULT 0,
    MontoPagado REAL NOT NULL DEFAULT 0,
    MetodoPago INTEGER NOT NULL DEFAULT 1,
    Estado INTEGER NOT NULL DEFAULT 1,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS CompraDetalles (
    Id TEXT PRIMARY KEY,
    CompraId TEXT NOT NULL,
    ProductoId TEXT NOT NULL,
    ProductoNombre TEXT NOT NULL DEFAULT '',
    Cantidad INTEGER NOT NULL,
    CostoUnitario REAL NOT NULL DEFAULT 0,
    Descuento REAL NOT NULL DEFAULT 0,
    ImpuestoPorcentaje REAL NOT NULL DEFAULT 0,
    SubTotal REAL NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS IX_Proveedores_Negocio ON Proveedores (NegocioId);
CREATE INDEX IF NOT EXISTS IX_Compras_SucursalFecha ON Compras (SucursalId, Fecha);
CREATE INDEX IF NOT EXISTS IX_CompraDetalles_Compra ON CompraDetalles (CompraId);
