CREATE TABLE IF NOT EXISTS SecuenciasDocumento (
    Id TEXT PRIMARY KEY,
    SucursalId TEXT NOT NULL,
    TipoDocumento INTEGER NOT NULL,
    Anio INTEGER NOT NULL,
    UltimoNumero INTEGER NOT NULL DEFAULT 0,
    FechaActualizacion TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_SecuenciasDocumento_Clave
ON SecuenciasDocumento (SucursalId, TipoDocumento, Anio);
