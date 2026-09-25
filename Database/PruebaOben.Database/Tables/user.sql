CREATE TABLE [dbo].[users]
(
    [id] INT IDENTITY(1,1) NOT NULL,
    [username] NVARCHAR(50) NOT NULL,
    [fullname] NVARCHAR(150) NOT NULL,
    [email] NVARCHAR(150) NOT NULL,
    [passwordHash] NVARCHAR(255) NOT NULL,
    [rol] NVARCHAR(50) NOT NULL,
    [active] BIT NOT NULL
        CONSTRAINT [DF_Users_Active] DEFAULT 1,
    [createdAt] DATETIME2 NOT NULL
        CONSTRAINT [DF_Users_CreatedAt] DEFAULT SYSDATETIME(),
    [updatedAt] DATETIME2 NULL,
    [deletedAt] DATETIME2 NULL,
    CONSTRAINT [PK_Users]
        PRIMARY KEY ([id]),

    CONSTRAINT [UQ_Users_Username]
        UNIQUE ([username]),

    CONSTRAINT [UQ_Users_Email]
        UNIQUE ([email])
);