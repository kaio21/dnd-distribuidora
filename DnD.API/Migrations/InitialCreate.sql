-- Script de criação manual do banco (alternativa às migrations EF)
-- Execute no SQL Server Management Studio ou Azure Data Studio

CREATE DATABASE DnDDistribuidora;
GO

USE DnDDistribuidora;
GO

CREATE TABLE Sellers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CPF NVARCHAR(14) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Sellers_CPF UNIQUE (CPF),
    CONSTRAINT UQ_Sellers_Email UNIQUE (Email)
);

CREATE TABLE Buyers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    StoreName NVARCHAR(150) NOT NULL,
    CPF NVARCHAR(14) NOT NULL,
    CNPJ NVARCHAR(18) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Buyers_CPF UNIQUE (CPF),
    CONSTRAINT UQ_Buyers_CNPJ UNIQUE (CNPJ),
    CONSTRAINT UQ_Buyers_Email UNIQUE (Email)
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL DEFAULT '',
    CostPrice DECIMAL(10,2) NOT NULL,
    SalePrice DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BuyerId INT NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    TotalProfit DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pendente',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Orders_Buyers FOREIGN KEY (BuyerId) REFERENCES Buyers(Id)
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    UnitCost DECIMAL(10,2) NOT NULL,
    Profit DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Messages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SenderId INT NOT NULL,
    SenderType NVARCHAR(20) NOT NULL,
    ReceiverId INT NOT NULL,
    ReceiverType NVARCHAR(20) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    BuyerId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Messages_Buyers FOREIGN KEY (BuyerId) REFERENCES Buyers(Id)
);

PRINT 'Banco de dados DnDDistribuidora criado com sucesso!';
