/*
    Procedimientos almacenados del servicio.
    Requiere SQL Server 2016 SP1 o superior para CREATE OR ALTER.
    Ejecutar con: sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i sql\procedimientos.sql
*/

USE DesavaPA;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.Id,
           p.Nombre,
           p.Descripcion,
           p.Precio,
           p.Stock,
           p.IdCategoria,
           c.Nombre AS CategoriaNombre,
           p.UrlImagen
    FROM dbo.Productos AS p
    INNER JOIN dbo.Categorias AS c ON c.Id = p.IdCategoria
    ORDER BY p.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Obtener
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.Id,
           p.Nombre,
           p.Descripcion,
           p.Precio,
           p.Stock,
           p.IdCategoria,
           c.Nombre AS CategoriaNombre,
           p.UrlImagen
    FROM dbo.Productos AS p
    INNER JOIN dbo.Categorias AS c ON c.Id = p.IdCategoria
    WHERE p.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Insertar
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @IdCategoria INT,
    @UrlImagen   VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Productos (Nombre, Descripcion, Precio, Stock, IdCategoria, UrlImagen)
    VALUES (@Nombre, @Descripcion, @Precio, @Stock, @IdCategoria, @UrlImagen);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Actualizar
    @Id          INT,
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @IdCategoria INT,
    @UrlImagen   VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Productos
    SET Nombre      = @Nombre,
        Descripcion = @Descripcion,
        Precio      = @Precio,
        Stock       = @Stock,
        IdCategoria = @IdCategoria,
        UrlImagen   = @UrlImagen
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Productos
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_SumarStock
    @IdProducto INT,
    @Cantidad   INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Productos
    SET Stock = Stock + @Cantidad
    WHERE Id = @IdProducto;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Devolucion_Insertar
    @IdVenta INT,
    @Motivo  VARCHAR(255),
    @Tipo    VARCHAR(20),
    @Monto   DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Devolucion (IdVenta, Fecha, MotivoDevolucion, TipoDevolucion, MontoTotalDevuelto)
    VALUES (@IdVenta, GETDATE(), @Motivo, @Tipo, @Monto);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetalleDevolucion_Insertar
    @IdDevolucion INT,
    @IdProducto   INT,
    @Cantidad     INT,
    @PrecioVenta  DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DetalleDevolucion (IdDevolucion, IdProducto, Cantidad, PrecioVenta)
    VALUES (@IdDevolucion, @IdProducto, @Cantidad, @PrecioVenta);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Venta_ObtenerPendientes
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH VentasPorProducto AS
    (
        SELECT IdProducto,
               SUM(Cantidad) AS CantidadVendida,
               MAX(Precio)   AS PrecioVenta
        FROM dbo.DetalleVenta
        WHERE IdVenta = @IdVenta
        GROUP BY IdProducto
    ),
    DevolucionesPorProducto AS
    (
        SELECT dd.IdProducto,
               SUM(dd.Cantidad) AS CantidadDevuelta
        FROM dbo.DetalleDevolucion AS dd
        INNER JOIN dbo.Devolucion AS d ON d.Id = dd.IdDevolucion
        WHERE d.IdVenta = @IdVenta
        GROUP BY dd.IdProducto
    )
    SELECT v.IdProducto,
           v.CantidadVendida,
           ISNULL(d.CantidadDevuelta, 0) AS CantidadDevuelta,
           v.CantidadVendida - ISNULL(d.CantidadDevuelta, 0) AS CantidadPendiente,
           v.PrecioVenta
    FROM VentasPorProducto AS v
    LEFT JOIN DevolucionesPorProducto AS d ON d.IdProducto = v.IdProducto
    ORDER BY v.IdProducto;
END;
GO

-- Auxiliares de validacion. No exponen datos al cliente: solo responden 1 o 0.
CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Existe
    @IdCategoria INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Categorias WHERE Id = @IdCategoria)
                THEN 1 ELSE 0 END AS Existe;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Venta_Existe
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Ventas WHERE Id = @IdVenta)
                THEN 1 ELSE 0 END AS Existe;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_TieneVentas
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.DetalleVenta WHERE IdProducto = @IdProducto)
                THEN 1 ELSE 0 END AS TieneVentas;
END;
GO
