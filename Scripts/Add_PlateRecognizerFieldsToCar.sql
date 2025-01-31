

BEGIN TRANSACTION;
GO

ALTER TABLE [Cars] ADD [Matricula] nvarchar(max) NULL;
GO
ALTER TABLE [Cars] ADD [PrCount] int NULL;
GO
ALTER TABLE [Cars] ADD [PrPlate] nvarchar(max) NULL;
GO
ALTER TABLE [Cars] ADD [PrScore] float NULL;
GO

COMMIT;
GO


BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'PrCount');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Cars] ALTER COLUMN [PrCount] int NULL;
GO


COMMIT;
GO


