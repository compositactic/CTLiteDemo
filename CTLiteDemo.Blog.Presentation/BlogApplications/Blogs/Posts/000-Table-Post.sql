EXEC CreateTable 'Post', 'Blog'
EXEC CreateOrModifyColumn 'Post', 'Title', 'nvarchar(50)'
EXEC CreateOrModifyColumn 'Post', 'Text', 'nvarchar(2000)'