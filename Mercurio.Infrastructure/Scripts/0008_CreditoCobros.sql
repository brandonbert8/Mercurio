CREATE TABLE IF NOT EXISTS Pagos (
    Id TEXT PRIMARY KEY,
    VentaId TEXT NULL,
    CompraId TEXT NULL,
    CuentaPorCobrarId TEXT NULL,
    ClienteId TEXT NULL,
    CajaSesionId TEXT NULL,
    Tipo INTEGER NOT NULL DEFAULT 1,
    MetodoPago INTEGER NOT NULL DEFAULT 1,
    Monto REAL NOT NULL DEFAULT 0,
    Referencia TEXT NOT NULL DEFAULT '',
    UsuarioId TEXT NULL,
    Fecha TEXT NOT NULL,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS CuentasPorCobrar (
    Id TEXT PRIMARY KEY,
    ClienteId TEXT NOT NULL,
    VentaId TEXT NOT NULL,
    FechaEmision TEXT NOT NULL,
    FechaVencimiento TEXT NOT NULL,
    MontoOriginal REAL NOT NULL DEFAULT 0,
    Saldo REAL NOT NULL DEFAULT 0,
    Estado INTEGER NOT NULL DEFAULT 1,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS AbonosCobro (
    Id TEXT PRIMARY KEY,
    CuentaPorCobrarId TEXT NOT NULL,
    ClienteId TEXT NOT NULL,
    Fecha TEXT NOT NULL,
    Monto REAL NOT NULL DEFAULT 0,
    MetodoPago INTEGER NOT NULL DEFAULT 1,
    Referencia TEXT NOT NULL DEFAULT '',
    UsuarioId TEXT NULL,
    CajaSesionId TEXT NULL,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS IX_Pagos_Venta ON Pagos (VentaId);
CREATE INDEX IF NOT EXISTS IX_Pagos_Compra ON Pagos (CompraId);
CREATE INDEX IF NOT EXISTS IX_CuentasPorCobrar_Cliente ON CuentasPorCobrar (ClienteId);
CREATE INDEX IF NOT EXISTS IX_CuentasPorCobrar_Venta ON CuentasPorCobrar (VentaId);
CREATE INDEX IF NOT EXISTS IX_AbonosCobro_Cuenta ON AbonosCobro (CuentaPorCobrarId);
