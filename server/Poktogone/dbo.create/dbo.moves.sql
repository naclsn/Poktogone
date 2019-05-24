CREATE TABLE [dbo].[moves] (
    [id]       INT          NOT NULL,
    [name]     VARCHAR (50) NOT NULL,
    [type]     INT          NOT NULL,
    [sps]      INT          NOT NULL,
    [accuracy] INT          NOT NULL,
    [power]    INT          NOT NULL,
    [pp]       INT          DEFAULT ((42)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Type] FOREIGN KEY ([type]) REFERENCES [dbo].[types] ([id])
);

