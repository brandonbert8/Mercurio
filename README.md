# Mercurio

> ERP de escritorio gratuito y de código abierto para negocios locales: punto de venta, inventario y administración en una sola aplicación.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows&logoColor=white)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Dapper](https://img.shields.io/badge/Dapper-2.1-0B4F6C)](https://github.com/DapperLib/Dapper)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/brandonbert8/Mercurio/pulls)
[![Status](https://img.shields.io/badge/status-en%20desarrollo-orange)](https://github.com/brandonbert8/Mercurio)

---

## Acerca del proyecto

**Mercurio** nace con una idea sencilla: que cualquier negocio local —una tienda de barrio, una papelería, una cafetería o un pequeño almacén— pueda digitalizar su operación sin pagar licencias ni depender de la nube.

Es un **ERP de escritorio** ligero, rápido y sin costos ocultos. Toda la información vive localmente en una base de datos **SQLite**, por lo que funciona sin conexión a internet y sin configurar servidores. Está construido sobre **.NET 10 con WPF** siguiendo los principios de **Clean Architecture**, para que sea fácil de mantener, extender y auditar.

### ¿Por qué Mercurio?

- **Gratis y open source** — Licencia MIT, úsalo, modifícalo y distribúyelo libremente.
- **Local y privado** — Tus datos son tuyos; no salen de tu computadora.
- **Offline first** — Funciona sin internet.
- **Ligero** — Un solo ejecutable para Windows, sin infraestructura compleja.
- **Extensible** — Arquitectura por capas pensada para crecer.

---

## Características

### Disponibles / en desarrollo

| Módulo | Descripción | Estado |
|---|---|---|
| Punto de venta | Registro de ventas y catálogo de productos | En desarrollo |
| Inventario | Alta de productos con precio de compra, venta y stock | En desarrollo |
| Gestión de productos | Código de barras, nombre, precios y estado activo | En desarrollo |
| Persistencia local | Base de datos SQLite con migraciones automáticas | Listo |
| Registro de eventos | Logs diarios con Serilog | Listo |

### En el roadmap

- [x] Arquitectura en capas (Domain / Application / Infrastructure / WPF)
- [x] Persistencia con Dapper + SQLite
- [x] Migraciones de base de datos con DbUp
- [x] Inyección de dependencias y host genérico
- [ ] Módulo completo de ventas y tickets
- [ ] Generación de comprobantes en PDF (QuestPDF)
- [ ] Gestión de clientes y proveedores
- [ ] Control de caja y arqueo
- [ ] Reportes de ventas e inventario
- [ ] Autenticación y roles de usuario
- [ ] Impresión de tickets
- [ ] Soporte para múltiples sucursales

---

## Stack tecnológico

| Tecnología | Uso |
|---|---|
| [.NET 10](https://dotnet.microsoft.com/) | Plataforma de ejecución |
| [WPF](https://learn.microsoft.com/dotnet/desktop/wpf/) | Interfaz de escritorio para Windows |
| [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) | Patrón MVVM con generadores de código |
| [Dapper](https://github.com/DapperLib/Dapper) | Micro ORM para acceso a datos |
| [SQLite](https://www.sqlite.org/) | Base de datos embebida |
| [DbUp](https://dbup.readthedocs.io/) | Migraciones versionadas de la base de datos |
| [Serilog](https://serilog.net/) | Logging estructurado a archivo |
| [QuestPDF](https://www.questpdf.com/) | Generación de documentos PDF |
| [Microsoft.Extensions.Hosting](https://learn.microsoft.com/dotnet/core/extensions/generic-host) | Host, configuración y DI |

---

## Arquitectura

Mercurio sigue **Clean Architecture**, separando responsabilidades en cuatro proyectos y manteniendo las dependencias apuntando hacia el dominio.

```
Mercurio.slnx
├── Mercurio.Domain          # Entidades e interfaces (núcleo, sin dependencias)
│   ├── Entities/            # Product, etc.
│   └── Interfaces/          # IProductRepository
├── Mercurio.Application     # Casos de uso y contratos de servicio
│   ├── Interfaces/          # ISaleService
│   └── Services/            # SaleService
├── Mercurio.Infrastructure  # Implementaciones concretas
│   ├── Repositories/        # ProductRepository (Dapper)
│   ├── Scripts/             # Migraciones SQL embebidas (DbUp)
│   └── DatabaseInitializer.cs
└── Mercurio.WPF             # Capa de presentación
    ├── ViewModels/          # MainViewModel
    ├── App.xaml.cs          # Composición, DI, logging y arranque
    └── MainWindow.xaml
```

**Flujo de dependencias:** `WPF → Application → Domain` · `Infrastructure → Application/Domain`

---

## Requisitos

- **Windows 10/11**
- [**.NET 10 SDK**](https://dotnet.microsoft.com/download) (para compilar)
- [**Visual Studio 2022+**](https://visualstudio.microsoft.com/) con la carga de trabajo *.NET desktop development* (recomendado)

---

## Instalación y uso

### 1. Clonar el repositorio

```bash
git clone https://github.com/brandonbert8/Mercurio.git
cd Mercurio
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Compilar y ejecutar

```bash
dotnet run --project Mercurio.WPF
```

También puedes abrir `Mercurio.slnx` en Visual Studio, establecer **Mercurio.WPF** como proyecto de inicio y presionar `F5`.

### 4. Base de datos

No requiere configuración manual. Al iniciar por primera vez, Mercurio ejecuta las migraciones embebidas (DbUp) y crea el archivo `mercurio.db` automáticamente. La cadena de conexión se define en `Mercurio.WPF/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=mercurio.db;Cache=Shared"
  }
}
```

Los registros de la aplicación se guardan en `logs/mercurio-log-YYYYMMDD.txt`.

---

## Uso básico

1. Abre la aplicación.
2. Escribe el **nombre** y el **precio de venta** de un producto.
3. Presiona **Agregar** para guardarlo en el inventario.
4. Usa **Refrescar** para recargar el catálogo desde la base de datos.

---

## Contribuir

¡Las contribuciones son bienvenidas! Si quieres mejorar Mercurio:

1. Haz un **fork** del repositorio.
2. Crea una rama para tu cambio: `git checkout -b feat/mi-mejora`.
3. Realiza tus cambios y añade pruebas cuando aplique.
4. Haz commit con un mensaje claro: `git commit -m "feat: agrega módulo de clientes"`.
5. Abre un **Pull Request** describiendo el cambio.

Antes de enviar tu PR, verifica que el proyecto compile:

```bash
dotnet build
```

### Convención de commits

Se recomienda [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.

### Ideas para empezar

- Completar el flujo de ventas.
- Agregar validaciones y manejo de errores en los repositorios.
- Ampliar la cobertura de pruebas.
- Mejorar la interfaz y la experiencia de usuario.

---

## Licencia

Distribuido bajo la **Licencia MIT**. Esto significa que puedes usar, copiar, modificar, fusionar, publicar, distribuir, sublicenciar y vender el software libremente, siempre que se incluya el aviso de copyright y la licencia original.

Consulta el archivo [`LICENSE`](LICENSE) para más detalles.

---

## Autor

**Brandon Bert** — [@brandonbert8](https://github.com/brandonbert8)

> Hecho con cariño para los negocios locales.
