-- 1. Create database

CREATE DATABASE STGenetics;


-- 2. Create table 

CREATE TABLE Animal (
    AnimalId INT PRIMARY KEY IDENTITY,
    Name VARCHAR(50) NOT NULL,
    Breed VARCHAR(50) NOT NULL,
    BirthDate DATE NOT NULL,
    Sex VARCHAR(6) NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    Status VARCHAR(9) NOT NULL
);

-- 3. Insert

USE [STGenetics]
GO

INSERT INTO [dbo].[Animal]
           ([Name]
           ,[Breed]
           ,[BirthDate]
           ,[Sex]
           ,[Price]
           ,[Status])
     VALUES
           ('Dog'
           ,'German Shepherd 2'
           ,'2023-01-05'
           ,'Male'
           ,10000.1
           ,'Active')
GO

-- 3. Update

USE [STGenetics]
GO

UPDATE [dbo].[Animal]
   SET [Name] = 'Dog'
      ,[Breed] = 'German Shepherd'
      ,[BirthDate] = '2023-01-05'
      ,[Sex] = 'Male'
      ,[Price] = 20000.1
      ,[Status] = 'Acitve'
 WHERE [AnimalId] = 1
GO


USE [STGenetics]
GO

-- 3. Delete

DELETE FROM [dbo].[Animal]
      WHERE [AnimalId] = 1
GO


-- 4. Script to insert 5,000 animals in the Animal table

-- Create a variable to hold the counter
DECLARE @counter INT = 1

-- Loop through and insert 5,000 animals
WHILE @counter <= 5000
BEGIN
    -- Generate random values for each column
    DECLARE @Name VARCHAR(50) = 'Animal' + CAST(@counter AS VARCHAR(10))
    DECLARE @Breed VARCHAR(50) = 'Breed' + CAST(ABS(CHECKSUM(NEWID())) % 10 + 1 AS VARCHAR(10))
    DECLARE @BirthDate DATE = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 365, GETDATE())
    DECLARE @Sex VARCHAR(6) = CASE ABS(CHECKSUM(NEWID())) % 2 WHEN 0 THEN 'Male' ELSE 'Female' END
    DECLARE @Price DECIMAL(10, 2) = ABS(CHECKSUM(NEWID())) % 1000
    DECLARE @Status VARCHAR(9) = CASE ABS(CHECKSUM(NEWID())) % 2 WHEN 0 THEN 'Active' ELSE 'Inactive' END
    
    -- Insert the animal into the table
    INSERT INTO Animal (Name, Breed, BirthDate, Sex, Price, Status)
    VALUES (@Name, @Breed, @BirthDate, @Sex, @Price, @Status)
    
    -- Increment the counter
    SET @counter = @counter + 1
END


-- 5 Script to list animals older than 2 years and female, sorted by name.

SELECT AnimalId, Name, Breed, BirthDate, Sex, Price, Status
FROM Animal
WHERE BirthDate <= DATEADD(YEAR, -2, GETDATE()) AND Sex = 'Female'
ORDER BY Name;

-- 6 Script to list the quantity of animals by sex.

SELECT Sex, COUNT(*) AS Quantity
FROM Animal
GROUP BY Sex;

-- Order tables

CREATE TABLE [Order] (
    OrderId INT PRIMARY KEY IDENTITY,
    Price DECIMAL(10, 2) NOT NULL,
    DiscountPercentage TINYINT NOT NULL,
    TotalPrice DECIMAL(10, 2) NOT NULL
);

CREATE TABLE [OrderAnimals] (
    OrderId INT NOT NULL,
    AnimalId INT NOT NULL
);

ALTER TABLE [dbo].[OrderAnimals]
ADD CONSTRAINT FK_OrderAnimals_Order FOREIGN KEY (OrderId)
      REFERENCES [Order](OrderId);

ALTER TABLE [dbo].[OrderAnimals]
ADD CONSTRAINT FK_OrderAnimals_Animal FOREIGN KEY (AnimalId)
      REFERENCES Animal(AnimalId);
