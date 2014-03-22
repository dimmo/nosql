CREATE DATABASE twitter
GO

USE twitter
GO

CREATE TABLE messages (
    id          UNIQUEIDENTIFIER NOT NULL,
    userName    VARCHAR(100),
    text        VARCHAR(1000),
    createDate  DATETIME NOT NULL,
    version     RowVersion
)
ALTER TABLE messages ADD CONSTRAINT messages_pk PRIMARY KEY (id)
CREATE INDEX messages_createDate_idx ON messages(createDate)
GO

CREATE TABLE likes (
    userName   VARCHAR(100) NOT NULL,
    messageId  UNIQUEIDENTIFIER NOT NULL,
    createDate DATETIME NOT NULL
)
ALTER TABLE likes ADD CONSTRAINT likes_pk PRIMARY KEY (userName, messageId)
ALTER TABLE likes ADD CONSTRAINT likes_messageId_fk FOREIGN KEY (messageId) REFERENCES messages(id)
CREATE INDEX likes_messageId_idx ON likes(messageId)
GO
