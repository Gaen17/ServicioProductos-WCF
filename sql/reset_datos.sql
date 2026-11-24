/*
    Restablece los datos de prueba de DesavaPA.
    Borra devoluciones, detalles y ventas, restaura el stock inicial
    y reinicia las identidades. Es seguro ejecutarlo varias veces.
    Ejecutar con: sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i sql\reset_datos.sql
*/

USE DesavaPA;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM dbo.DetalleDevolucion;
    DELETE FROM dbo.Devolucion;
    DELETE FROM dbo.DetalleVenta;
    DELETE FROM dbo.Ventas;
    DELETE FROM dbo.Productos;
    DELETE FROM dbo.Categorias;

    -- Tablas sin datos iniciales: el siguiente Id debe ser 1.
    DBCC CHECKIDENT ('dbo.DetalleDevolucion', RESEED, 1) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Devolucion', RESEED, 1) WITH NO_INFOMSGS;

    -- Categorias: Id 1..4
    SET IDENTITY_INSERT dbo.Categorias ON;
    INSERT INTO dbo.Categorias (Id, Nombre)
    VALUES (1, 'Electronicos'),
           (2, 'Ropa de hombre'),
           (3, 'Ropa de mujer'),
           (4, 'Hogar');
    SET IDENTITY_INSERT dbo.Categorias OFF;

    -- Productos: Id 1..8. El stock es el stock actual de prueba.
    SET IDENTITY_INSERT dbo.Productos ON;
    INSERT INTO dbo.Productos (Id, Nombre, Descripcion, Precio, Stock, IdCategoria, UrlImagen)
    VALUES (1, 'Laptop Lenovo',    'Laptop de 15 pulgadas, 16 GB de RAM', 3500.00, 10, 1, 'https://example.com/img/laptop.jpg'),
           (2, 'Mouse Logitech',   'Mouse inalambrico USB',                120.00, 20, 1, 'https://example.com/img/mouse.jpg'),
           (3, 'Teclado Redragon', 'Teclado mecanico con retroiluminacion', 250.00, 15, 1, 'https://example.com/img/teclado.jpg'),
           (4, 'Monitor Samsung',  'Monitor LED de 24 pulgadas',           1800.00,  8, 1, 'https://example.com/img/monitor.jpg'),
           (5, 'Auriculares Sony', 'Auriculares Bluetooth con cancelacion', 450.00, 12, 1, 'https://example.com/img/auriculares.jpg'),
           (6, 'Polo basico',      'Polo de algodon manga corta',            80.00, 30, 2, 'https://example.com/img/polo.jpg'),
           (7, 'Blusa floral',     'Blusa de verano estampada',              95.00, 25, 3, 'https://example.com/img/blusa.jpg'),
           (8, 'Lampara de mesa',  'Lampara LED de escritorio',              60.00, 18, 4, 'https://example.com/img/lampara.jpg');
    SET IDENTITY_INSERT dbo.Productos OFF;

    -- Ventas: Id 1..3
    SET IDENTITY_INSERT dbo.Ventas ON;
    INSERT INTO dbo.Ventas (Id, Fecha, Total)
    VALUES (1, '2026-10-01T10:30:00', 0),
           (2, '2026-10-02T16:15:00', 0),
           (3, '2026-10-03T09:45:00', 0);
    SET IDENTITY_INSERT dbo.Ventas OFF;

    -- DetalleVenta: Id 1..6
    SET IDENTITY_INSERT dbo.DetalleVenta ON;
    INSERT INTO dbo.DetalleVenta (Id, IdVenta, IdProducto, Cantidad, Precio)
    VALUES (1, 1, 1, 2, 3500.00),
           (2, 1, 2, 3,  120.00),
           (3, 2, 3, 1,  250.00),
           (4, 2, 4, 1, 1800.00),
           (5, 3, 2, 5,  120.00),
           (6, 3, 5, 2,  450.00);
    SET IDENTITY_INSERT dbo.DetalleVenta OFF;

    -- Totales coherentes con el detalle.
    UPDATE v
    SET Total = x.Total
    FROM dbo.Ventas AS v
    INNER JOIN
    (
        SELECT IdVenta, SUM(Cantidad * Precio) AS Total
        FROM dbo.DetalleVenta
        GROUP BY IdVenta
    ) AS x ON x.IdVenta = v.Id;

    -- Sincroniza el valor actual de las identidades con el maximo insertado.
    -- Se indica el valor de forma explicita para que tambien baje si quedo alto.
    DBCC CHECKIDENT ('dbo.Categorias', RESEED, 4) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Productos', RESEED, 8) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Ventas', RESEED, 3) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.DetalleVenta', RESEED, 6) WITH NO_INFOMSGS;

    COMMIT TRANSACTION;
    PRINT 'Datos de prueba restablecidos correctamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Error al restablecer los datos: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
