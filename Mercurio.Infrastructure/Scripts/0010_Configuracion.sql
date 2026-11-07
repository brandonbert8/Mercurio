CREATE TABLE IF NOT EXISTS Configuraciones (
    Id TEXT PRIMARY KEY,
    NegocioId TEXT NOT NULL,
    Clave TEXT NOT NULL,
    Valor TEXT NOT NULL DEFAULT '',
    Descripcion TEXT NOT NULL DEFAULT '',
    Grupo TEXT NOT NULL DEFAULT 'General',
    FechaActualizacion TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Configuraciones_NegocioClave ON Configuraciones (NegocioId, Clave);
CREATE INDEX IF NOT EXISTS IX_Configuraciones_Grupo ON Configuraciones (NegocioId, Grupo);
