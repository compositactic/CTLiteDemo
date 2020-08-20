EXEC CreateTable 'Comment', 'Post'
EXEC CreateOrModifyColumn 'Comment', 'Text', 'nvarchar(150)'
EXEC CreateOrModifyColumn 'Comment', 'UserId', 'int'
EXEC CreateForeignKeyConstraint 'Comment', 'User', 'UserId', 'Id'