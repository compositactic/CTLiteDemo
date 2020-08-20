INSERT INTO Comment (PostId, Text, UserId) VALUES ((SELECT ID FROM Post WHERE Title = 'Anim id est laborum'), 'feugiat euismod lacinia at', (SELECT Id FROM "User" WHERE Name = 'alice'))
INSERT INTO Comment (PostId, Text, UserId) VALUES ((SELECT ID FROM Post WHERE Title = 'Anim id est laborum'), 'quam elementum', (SELECT Id FROM "User" WHERE Name = 'bob'))
