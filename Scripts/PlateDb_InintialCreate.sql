
BEGIN TRANSACTION;
GO

CREATE TABLE [PlateResults] (
    [Id] int NOT NULL IDENTITY,
    [DatabaseTable] nvarchar(max) NULL,
    [ImageId] int NOT NULL,
    [HighScore] float NULL,
    [HighPlate] nvarchar(max) NULL,
    [Area] float NULL,
    [Resposta] nvarchar(max) NULL,
    CONSTRAINT [PK_PlateResults] PRIMARY KEY ([Id])
);
GO

COMMIT;
GO

