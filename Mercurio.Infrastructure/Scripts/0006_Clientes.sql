CREATE TABLE IF NOT EXISTS Clientes (
    Id TEXT PRIMARY KEY,
    NegocioId TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    TipoDocumento TEXT NOT NULL DEFAULT 'DNI',
    NumeroDocumento TEXT NOT NULL DEFAULT '',
    Telefono TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Direccion TEXT NOT NULL DEFAULT '',
    LimiteCredito REAL NOT NULL DEFAULT 0,
    Activo INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_Clientes_Negocio ON Clientes (NegocioId);
CREATE INDEX IF NOT EXISTS IX_Clientes_Documento ON Clientes (NegocioId, NumeroDocumento);
