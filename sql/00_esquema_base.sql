/*
    Esquema base de la base de datos DesavaPA.
    Nota: el archivo pa2_pa3_esquema.sql no estaba disponible en el equipo.
    Este script reconstruye el esquema a partir de los enunciados PA2/PA3
    y del resumen entregado, manteniendo los nombres de tablas y columnas.
*/

IF DB_ID(N'DesavaPA') IS NULL
BEGIN
    CREATE DATABASE DesavaPA;
END;
GO

USE DesavaPA;
GO

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias
    (
        Id     INT IDENTITY(1,1) NOT NULL,
        Nombre VARCHAR(100)      NOT NULL,
        CONSTRAINT PK_Categorias PRIMARY KEY (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Productos
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        Nombre      VARCHAR(100)      NOT NULL,
        Descripcion VARCHAR(255)      NULL,
        Precio      DECIMAL(10,2)     NOT NULL,
        Stock       INT               NOT NULL,
        IdCategoria INT               NOT NULL,
        UrlImagen   VARCHAR(500)      NULL,
        CONSTRAINT PK_Productos PRIMARY KEY (Id),
        CONSTRAINT FK_Productos_Categorias
            FOREIGN KEY (IdCategoria) REFERENCES dbo.Categorias (Id),
        CONSTRAINT CK_Productos_Precio CHECK (Precio > 0),
        CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Ventas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ventas
    (
        Id    INT IDENTITY(1,1) NOT NULL,
        Fecha DATETIME          NOT NULL CONSTRAINT DF_Ventas_Fecha DEFAULT (GETDATE()),
        Total DECIMAL(10,2)     NOT NULL,
        CONSTRAINT PK_Ventas PRIMARY KEY (Id),
        CONSTRAINT CK_Ventas_Total CHECK (Total >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetalleVenta
    (
        Id         INT IDENTITY(1,1) NOT NULL,
        IdVenta    INT               NOT NULL,
        IdProducto INT               NOT NULL,
        Cantidad   INT               NOT NULL,
        Precio     DECIMAL(10,2)     NOT NULL,
        CONSTRAINT PK_DetalleVenta PRIMARY KEY (Id),
        CONSTRAINT FK_DetalleVenta_Ventas
            FOREIGN KEY (IdVenta) REFERENCES dbo.Ventas (Id),
        CONSTRAINT FK_DetalleVenta_Productos
            FOREIGN KEY (IdProducto) REFERENCES dbo.Productos (Id),
        CONSTRAINT CK_DetalleVenta_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetalleVenta_Precio CHECK (Precio >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Devolucion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Devolucion
    (
        Id                 INT IDENTITY(1,1) NOT NULL,
        IdVenta            INT               NOT NULL,
        Fecha              DATETIME          NOT NULL CONSTRAINT DF_Devolucion_Fecha DEFAULT (GETDATE()),
        MotivoDevolucion   VARCHAR(255)      NOT NULL,
        TipoDevolucion     VARCHAR(20)       NOT NULL,
        MontoTotalDevuelto DECIMAL(10,2)     NOT NULL,
        CONSTRAINT PK_Devolucion PRIMARY KEY (Id),
        CONSTRAINT FK_Devolucion_Ventas
            FOREIGN KEY (IdVenta) REFERENCES dbo.Ventas (Id),
        CONSTRAINT CK_Devolucion_Tipo
            CHECK (TipoDevolucion IN ('Parcial', 'Completa')),
        CONSTRAINT CK_Devolucion_Monto CHECK (MontoTotalDevuelto >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetalleDevolucion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetalleDevolucion
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        IdDevolucion  INT               NOT NULL,
        IdProducto    INT               NOT NULL,
        Cantidad      INT               NOT NULL,
        PrecioVenta   DECIMAL(10,2)     NOT NULL,
        CONSTRAINT PK_DetalleDevolucion PRIMARY KEY (Id),
        CONSTRAINT FK_DetalleDevolucion_Devolucion
            FOREIGN KEY (IdDevolucion) REFERENCES dbo.Devolucion (Id),
        CONSTRAINT FK_DetalleDevolucion_Productos
            FOREIGN KEY (IdProducto) REFERENCES dbo.Productos (Id),
        CONSTRAINT CK_DetalleDevolucion_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetalleDevolucion_Precio CHECK (PrecioVenta >= 0)
    );
END;
GO
