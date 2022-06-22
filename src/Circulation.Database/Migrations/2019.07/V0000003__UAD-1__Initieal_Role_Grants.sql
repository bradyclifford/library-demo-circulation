
ALTER ROLE [db_datareader] ADD MEMBER [Bff_Role]
ALTER ROLE [db_datawriter] ADD MEMBER [Bff_Role]

GRANT UPDATE ON [dbo].[PublicationCopyId] TO [Bff_Role]

ALTER ROLE [db_datareader] ADD MEMBER [Service_Role]
ALTER ROLE [db_datawriter] ADD MEMBER [Service_Role]

GRANT UPDATE ON [dbo].[PublicationCopyId] TO [Service_Role]
