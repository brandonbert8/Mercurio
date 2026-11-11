CREATE TABLE IF NOT EXISTS CuentasPorPagar (
    Id TEXT PRIMARY KEY,
    CompraId TEXT NOT NULL,
    ProveedorId TEXT NOT NULL,
    FechaEmision TEXT NOT NULL,
    FechaVencimiento TEXT NOT NULL,
    MontoOriginal REAL NOT NULL DEFAULT 0,
    Saldo REAL NOT NULL DEFAULT 0,
    Estado INTEGER NOT NULL DEFAULT 1,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS AbonosPago (
    Id TEXT PRIMARY KEY,
    CuentaPorPagarId TEXT NOT NULL,
    ProveedorId TEXT NOT NULL,
    Fecha TEXT NOT NULL,
    Monto REAL NOT NULL DEFAULT 0,
    MetodoPago INTEGER NOT NULL DEFAULT 1,
    Referencia TEXT NOT NULL DEFAULT '',
    UsuarioId TEXT NULL,
    CajaSesionId TEXT NULL,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS IX_CuentasPorPagar_Compra ON CuentasPorPagar (CompraId);
CREATE INDEX IF NOT EXISTS IX_CuentasPorPagar_Proveedor ON CuentasPorPagar (ProveedorId);
CREATE INDEX IF NOT EXISTS IX_AbonosPago_Cuenta ON AbonosPago (CuentaPorPagarId);
