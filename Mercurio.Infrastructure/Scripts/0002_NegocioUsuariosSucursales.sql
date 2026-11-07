CREATE TABLE IF NOT EXISTS Negocios (
    Id TEXT PRIMARY KEY,
    Nombre TEXT NOT NULL,
    RazonSocial TEXT NOT NULL DEFAULT '',
    Ruc TEXT NOT NULL DEFAULT '',
    Direccion TEXT NOT NULL DEFAULT '',
    Telefono TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Moneda TEXT NOT NULL DEFAULT 'PEN',
    ZonaHoraria TEXT NOT NULL DEFAULT 'America/Lima',
    Activo INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Sucursales (
    Id TEXT PRIMARY KEY,
    NegocioId TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    Codigo TEXT NOT NULL DEFAULT '',
    Direccion TEXT NOT NULL DEFAULT '',
    Telefono TEXT NOT NULL DEFAULT '',
    Activa INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Usuarios (
    Id TEXT PRIMARY KEY,
    NegocioId TEXT NOT NULL,
    SucursalId TEXT NULL,
    NombreUsuario TEXT NOT NULL,
    NombreCompleto TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    PasswordHash TEXT NOT NULL DEFAULT '',
    PasswordSalt TEXT NOT NULL DEFAULT '',
    Rol INTEGER NOT NULL DEFAULT 3,
    Activo INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL,
    UltimoAcceso TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Usuarios_NegocioUsuario ON Usuarios (NegocioId, NombreUsuario);
CREATE INDEX IF NOT EXISTS IX_Sucursales_Negocio ON Sucursales (NegocioId);
