-- CTLite.Data.MicrosoftSqlServer - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

-- Permission is hereby granted, free of charge, to any person obtaining a copy of this software
-- and associated documentation files (the "Software"), to deal in the Software without restriction, 
-- including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
-- and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
-- subject to the following conditions:

-- The above copyright notice and this permission notice shall be included in all copies
-- or substantial portions of the Software.

-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
-- LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
-- NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
-- WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
-- SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

CREATE OR ALTER PROCEDURE dbo.CreateTable 
    @tableName NVARCHAR(MAX),
	@parentTableName NVARCHAR(MAX) = '',
	@baseTableName NVARCHAR(MAX) = ''
AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @dropReplicationSql NVARCHAR(MAX)

	SET @sql = 'IF NOT EXISTS (SELECT * FROM sys.tables WHERE Name = ''' + @tableName + ''')
		BEGIN
			CREATE TABLE "' + @tableName + '" ( [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY '
	IF @parentTableName = ''
		SET @sql = @sql + ')
			PRINT ''Created table: ' + @tableName + '''
		END'
	ELSE
		SET @sql = @sql + ', [' + @parentTableName + 'Id] BIGINT NOT NULL,
			CONSTRAINT [FK_' + @tableName + '_To_' + @parentTableName + '] FOREIGN KEY ([' + @parentTableName + 'Id]) REFERENCES ['  + @parentTableName + ']([Id]) ON DELETE CASCADE)
			PRINT ''Created table: ' + @tableName + '''
		END'


	PRINT @sql

	EXEC sp_executesql @sql