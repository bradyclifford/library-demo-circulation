DROP TABLE PublicationCopy;


CREATE TABLE dbo.Publication
(
	isbn varchar(13) not null,
	constraint [PK_Publication] primary key clustered
	(
		isbn
	)
)

CREATE TABLE dbo.PublicationCopy
(
	isbn varchar(13) not null,
	publicationCopyId int not null,
	location varchar(50) not null,
	expectedReturnDate date null,
	constraint [PK_PublicationCopy] primary key clustered
	(
		isbn,
		publicationCopyId
	),
	constraint [FK_Publication_PublicationCopy] foreign key (isbn) references dbo.Publication (isbn)
);


CREATE SEQUENCE [dbo].[PublicationCopyId] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
;

