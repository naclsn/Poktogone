CREATE TABLE [dbo].[items] (
    [id]   INT           NOT NULL,
    [name] VARCHAR (50)  NOT NULL,
    [desc] VARCHAR (500) NOT NULL,
    [uniq] TINYINT       NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

