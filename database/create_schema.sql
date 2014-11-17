
    if exists (select * from dbo.sysobjects where id = object_id(N'PersonWithExplicitGeneratorProperties') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PersonWithExplicitGeneratorProperties

    if exists (select * from dbo.sysobjects where id = object_id(N'PersonWithBlockSizeDefault') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PersonWithBlockSizeDefault

    if exists (select * from dbo.sysobjects where id = object_id(N'PersonWithBlockSizeZero') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PersonWithBlockSizeZero

    if exists (select * from dbo.sysobjects where id = object_id(N'PersonWithBlockSizeOne') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PersonWithBlockSizeOne

    if exists (select * from dbo.sysobjects where id = object_id(N'PersonWithBlockSizeTwo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PersonWithBlockSizeTwo

    if exists (select * from dbo.sysobjects where id = object_id(N'HibernateUniqueKey') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table HibernateUniqueKey

    if exists (select * from dbo.sysobjects where id = object_id(N'hibernate_unique_key') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table hibernate_unique_key

    create table PersonWithExplicitGeneratorProperties (
        Id bigint not null,
       FirstName NVARCHAR(20) not null,
       LastName NVARCHAR(20) not null,
       primary key (Id)
    )

    create table PersonWithBlockSizeDefault (
        Id bigint not null,
       FirstName NVARCHAR(20) not null,
       LastName NVARCHAR(20) not null,
       primary key (Id)
    )

    create table PersonWithBlockSizeZero (
        Id bigint not null,
       FirstName NVARCHAR(20) not null,
       LastName NVARCHAR(20) not null,
       primary key (Id)
    )

    create table PersonWithBlockSizeOne (
        Id bigint not null,
       FirstName NVARCHAR(20) not null,
       LastName NVARCHAR(20) not null,
       primary key (Id)
    )

    create table PersonWithBlockSizeTwo (
        Id bigint not null,
       FirstName NVARCHAR(20) not null,
       LastName NVARCHAR(20) not null,
       primary key (Id)
    )

    create table HibernateUniqueKey (
         NextId BIGINT 
    )

    insert into HibernateUniqueKey values ( 1 )

    create table hibernate_unique_key (
         next_id BIGINT 
    )

    insert into hibernate_unique_key values ( 1 )
