CREATE TABLE Persons (
    ID int NOT NULL,
    LastName varchar(255) NOT NULL,
    FirstName varchar(255),
    Age int,
    CONSTRAINT PK_Person PRIMARY KEY (ID,LastName)
);



----
ALTER TABLE Persons
ADD PRIMARY KEY (ID);
---
ALTER TABLE Persons
ADD CONSTRAINT PK_Person PRIMARY KEY (ID,LastName);
--
--Create a nonclustered index on a table or view
CREATE INDEX index1 ON schema1.table1 (column1);
--
Create a clustered index on a table and use a 3-part name for the table
CREATE CLUSTERED INDEX index1 ON database1.schema1.table1 (column1);
--
--Create a nonclustered index with a unique constraint and specify the sort order
CREATE UNIQUE INDEX index1 ON schema1.table1 (column1 DESC, column2 ASC, column3 DESC);