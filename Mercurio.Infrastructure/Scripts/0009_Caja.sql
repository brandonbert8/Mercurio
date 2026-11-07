CREATE TABLE IF NOT EXISTS Cajas (
    Id TEXT PRIMARY KEY,
    SucursalId TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    Activa INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS CajaSesiones (
    Id TEXT PRIMARY KEY,
    CajaId TEXT NOT NULL,
    UsuarioId TEXT NOT NULL,
    FechaApertura TEXT NOT NULL,
    FechaCierre TEXT NULL,
    MontoInicial REAL NOT NULL DEFAULT 0,
    MontoEsperado REAL NOT NULL DEFAULT 0,
    MontoDeclarado REAL NOT NULL DEFAULT 0,
    Diferencia REAL NOT NULL DEFAULT 0,
    Estado INTEGER NOT NULL DEFAULT 1,
    Observacion TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS CajaMovimientos (
    Id TEXT PRIMARY KEY,
    CajaSesionId TEXT NOT NULL,
    Tipo INTEGER NOT NULL,
    Concepto TEXT NOT NULL DEFAULT '',
    Monto REAL NOT NULL DEFAULT 0,
    Fecha TEXT NOT NULL,
    UsuarioId TEXT NULL,
    Referencia TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS IX_Cajas_Sucursal ON Cajas (SucursalId);
CREATE INDEX IF NOT EXISTS IX_CajaSesiones_Caja ON CajaSesiones (CajaId, Estado);
CREATE INDEX IF NOT EXISTS IX_CajaSesiones_Fecha ON CajaSesiones (FechaApertura);
CREATE INDEX IF NOT EXISTS IX_CajaMovimientos_Sesion ON CajaMovimientos (CajaSesionId, Fecha);
