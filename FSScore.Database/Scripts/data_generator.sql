-- ==================================================================
-- data_generator.sql (BONUS)
-- 
-- Assignment Part 1 Bonus: Automated data generation procedure
-- Parameters: Number of subjects, zones, questions, tests
-- Creates: Catalog entities (SnapshotId=0) + New assessment snapshot with random scores
-- ==================================================================

USE [Grades]
GO

-- ==================================================================
-- STORED PROCEDURE: Generate Test Data
-- ==================================================================
CREATE OR ALTER PROCEDURE [dbo].[GenerateTestData]
    @NumSubjects INT = 2,
    @NumZones INT = 5,
    @NumQuestions INT = 20,
    @NumTests INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input parameters
    IF @NumSubjects <= 0 OR @NumZones <= 0 OR @NumQuestions <= 0 OR @NumTests <= 0
    BEGIN
        PRINT 'ERROR: All parameters must be greater than 0'
        RETURN
    END
    
    IF @NumSubjects > 10 OR @NumZones > 50 OR @NumQuestions > 500 OR @NumTests > 20
    BEGIN
        PRINT 'ERROR: Parameters too large. Max: Subjects=10, Zones=50, Questions=500, Tests=20'
        RETURN
    END
    
    PRINT 'Starting data generation...'
    PRINT 'Parameters: Subjects=' + CAST(@NumSubjects AS VARCHAR(10)) + 
          ', Zones=' + CAST(@NumZones AS VARCHAR(10)) + 
          ', Questions=' + CAST(@NumQuestions AS VARCHAR(10)) + 
          ', Tests=' + CAST(@NumTests AS VARCHAR(10))
    
    -- ==================================================================
    -- PHASE 1: CREATE CATALOG DATA (SnapshotId = 0)
    -- ==================================================================
    
    -- Get starting IDs
    DECLARE @StartSubjectId INT = (SELECT ISNULL(MAX(SubjectId), 0) + 1 FROM Subjects)
    DECLARE @StartZoneId INT = (SELECT ISNULL(MAX(ZoneId), 0) + 1 FROM Zones)
    DECLARE @StartTestId INT = (SELECT ISNULL(MAX(TestId), 0) + 1 FROM Tests)
    DECLARE @StartQuestionId INT = (SELECT ISNULL(MAX(QuestionId), 0) + 1 FROM Questions)
    
    PRINT 'Creating catalog data (SnapshotId = 0)...'
    
    -- Create subjects
    DECLARE @i INT = 0
    WHILE @i < @NumSubjects
    BEGIN
        INSERT INTO Subjects (SnapshotId, SubjectId, SubjectName)
        VALUES (0, @StartSubjectId + @i, 'Generated Subject ' + CAST(@StartSubjectId + @i AS VARCHAR(10)))
        SET @i = @i + 1
    END
    PRINT 'Created ' + CAST(@NumSubjects AS VARCHAR(10)) + ' subjects'
    
    -- Create tests (mix of national and regular)
    SET @i = 0
    WHILE @i < @NumTests
    BEGIN
        DECLARE @IsNational BIT = CASE WHEN @i % 2 = 0 THEN 1 ELSE 0 END -- Alternate national/regular
        INSERT INTO Tests (TestId, TestName, IsATest)
        VALUES (@StartTestId + @i, 
                'Generated Test ' + CAST(@StartTestId + @i AS VARCHAR(10)) + 
                CASE WHEN @IsNational = 1 THEN ' (National)' ELSE ' (Regular)' END,
                @IsNational)
        SET @i = @i + 1
    END
    PRINT 'Created ' + CAST(@NumTests AS VARCHAR(10)) + ' tests'
    
    -- Create zones and link to subjects
    SET @i = 0
    DECLARE @ZonesPerSubject INT = @NumZones / @NumSubjects
    DECLARE @CurrentSubject INT = 0
    
    WHILE @i < @NumZones
    BEGIN
        -- Calculate which subject this zone belongs to
        SET @CurrentSubject = @i / @ZonesPerSubject
        IF @CurrentSubject >= @NumSubjects SET @CurrentSubject = @NumSubjects - 1
        
        -- Create zone
        INSERT INTO Zones (SnapshotId, ZoneId, ZoneName, IsRelevant)
        VALUES (0, @StartZoneId + @i, 'Generated Zone ' + CAST(@StartZoneId + @i AS VARCHAR(10)), 1)
        
        -- Link zone to subject
        INSERT INTO SubjectZones (SnapshotId, SubjectId, ZoneId)
        VALUES (0, @StartSubjectId + @CurrentSubject, @StartZoneId + @i)
        
        SET @i = @i + 1
    END
    PRINT 'Created ' + CAST(@NumZones AS VARCHAR(10)) + ' zones and linked to subjects'
    
    -- Create questions and link to zones
    SET @i = 0
    DECLARE @QuestionsPerZone INT = @NumQuestions / @NumZones
    DECLARE @CurrentZone INT = 0
    DECLARE @CurrentTest INT = 0
    
    WHILE @i < @NumQuestions
    BEGIN
        -- Calculate which zone and test this question belongs to
        SET @CurrentZone = @i / @QuestionsPerZone
        IF @CurrentZone >= @NumZones SET @CurrentZone = @NumZones - 1
        
        SET @CurrentTest = @i % @NumTests -- Distribute questions across tests
        
        -- Create question
        INSERT INTO Questions (SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId)
        VALUES (0, @StartQuestionId + @i, 
                'Generated Question ' + CAST(@StartQuestionId + @i AS VARCHAR(10)) + 
                ': What is the answer to question number ' + CAST(@StartQuestionId + @i AS VARCHAR(10)) + '?',
                NULL, -- Catalog has no scores
                1, 
                @StartTestId + @CurrentTest)
        
        -- Link question to zone
        INSERT INTO ZonesQuestions (SnapshotId, ZoneId, QuestionId)
        VALUES (0, @StartZoneId + @CurrentZone, @StartQuestionId + @i)
        
        -- Add some cross-zone relationships (10% of questions)
        IF @i % 10 = 0 AND @CurrentZone > 0
        BEGIN
            INSERT INTO ZonesQuestions (SnapshotId, ZoneId, QuestionId)
            VALUES (0, @StartZoneId + @CurrentZone - 1, @StartQuestionId + @i)
        END
        
        SET @i = @i + 1
    END
    PRINT 'Created ' + CAST(@NumQuestions AS VARCHAR(10)) + ' questions and linked to zones'
    
    -- ==================================================================
    -- PHASE 2: CREATE ASSESSMENT SNAPSHOT WITH RANDOM SCORES
    -- ==================================================================
    
    -- Generate new SnapshotId
    DECLARE @NewSnapshotId INT = (SELECT ISNULL(MAX(SnapshotId), 0) + 1 FROM Questions)
    
    PRINT 'Creating assessment snapshot (SnapshotId = ' + CAST(@NewSnapshotId AS VARCHAR(10)) + ') with random scores...'
    
    -- Copy subjects
    INSERT INTO Subjects (SnapshotId, SubjectId, SubjectName)
    SELECT @NewSnapshotId, SubjectId, SubjectName
    FROM Subjects 
    WHERE SnapshotId = 0 AND SubjectId BETWEEN @StartSubjectId AND @StartSubjectId + @NumSubjects - 1
    
    -- Copy zones
    INSERT INTO Zones (SnapshotId, ZoneId, ZoneName, IsRelevant)
    SELECT @NewSnapshotId, ZoneId, ZoneName, 
           CASE WHEN ABS(CHECKSUM(NEWID())) % 10 = 0 THEN 0 ELSE 1 END -- 10% chance irrelevant
    FROM Zones 
    WHERE SnapshotId = 0 AND ZoneId BETWEEN @StartZoneId AND @StartZoneId + @NumZones - 1
    
    -- Copy SubjectZones relationships
    INSERT INTO SubjectZones (SnapshotId, SubjectId, ZoneId)
    SELECT @NewSnapshotId, SubjectId, ZoneId
    FROM SubjectZones 
    WHERE SnapshotId = 0 
      AND SubjectId BETWEEN @StartSubjectId AND @StartSubjectId + @NumSubjects - 1
      AND ZoneId BETWEEN @StartZoneId AND @StartZoneId + @NumZones - 1
    
    -- Copy questions with random scores
    INSERT INTO Questions (SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId)
    SELECT @NewSnapshotId, QuestionId, QuestionText,
           CASE 
               WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN NULL -- 20% unanswered
               WHEN ABS(CHECKSUM(NEWID())) % 10 < 2 THEN ABS(CHECKSUM(NEWID())) % 41 + 30 -- 20% low scores (30-70)
               ELSE ABS(CHECKSUM(NEWID())) % 31 + 70 -- 60% high scores (70-100)
           END as Score,
           CASE WHEN ABS(CHECKSUM(NEWID())) % 20 = 0 THEN 0 ELSE 1 END, -- 5% chance irrelevant
           TestId
    FROM Questions 
    WHERE SnapshotId = 0 AND QuestionId BETWEEN @StartQuestionId AND @StartQuestionId + @NumQuestions - 1
    
    -- Copy ZonesQuestions relationships
    INSERT INTO ZonesQuestions (SnapshotId, ZoneId, QuestionId)
    SELECT @NewSnapshotId, ZoneId, QuestionId
    FROM ZonesQuestions 
    WHERE SnapshotId = 0 
      AND ZoneId BETWEEN @StartZoneId AND @StartZoneId + @NumZones - 1
      AND QuestionId BETWEEN @StartQuestionId AND @StartQuestionId + @NumQuestions - 1
    
    PRINT 'Assessment snapshot created with random scores!'
    
    -- ==================================================================
    -- VERIFICATION AND SUMMARY
    -- ==================================================================
    
    PRINT ''
    PRINT '=== GENERATION SUMMARY ==='
    
    -- Catalog summary
    SELECT 'Catalog Entities' as Category, 
           COUNT(CASE WHEN SnapshotId = 0 THEN 1 END) as Count
    FROM (
        SELECT SnapshotId FROM Subjects WHERE SubjectId BETWEEN @StartSubjectId AND @StartSubjectId + @NumSubjects - 1
        UNION ALL
        SELECT 0 FROM Tests WHERE TestId BETWEEN @StartTestId AND @StartTestId + @NumTests - 1
        UNION ALL  
        SELECT SnapshotId FROM Zones WHERE SnapshotId = 0 AND ZoneId BETWEEN @StartZoneId AND @StartZoneId + @NumZones - 1
        UNION ALL
        SELECT SnapshotId FROM Questions WHERE SnapshotId = 0 AND QuestionId BETWEEN @StartQuestionId AND @StartQuestionId + @NumQuestions - 1
    ) entities
    
    -- Assessment summary
    SELECT 'Assessment Snapshot ' + CAST(@NewSnapshotId AS VARCHAR(10)) as Category,
           COUNT(*) as Count
    FROM Questions 
    WHERE SnapshotId = @NewSnapshotId
    
    -- Score distribution in assessment
    SELECT 'Score Distribution' as Info,
           COUNT(CASE WHEN Score IS NULL THEN 1 END) as Unanswered,
           COUNT(CASE WHEN Score BETWEEN 0 AND 59 THEN 1 END) as Low,
           COUNT(CASE WHEN Score BETWEEN 60 AND 79 THEN 1 END) as Medium,
           COUNT(CASE WHEN Score BETWEEN 80 AND 100 THEN 1 END) as High
    FROM Questions 
    WHERE SnapshotId = @NewSnapshotId
    
    PRINT ''
    PRINT 'Data generation completed successfully!'
    PRINT 'Catalog SnapshotId: 0'
    PRINT 'Assessment SnapshotId: ' + CAST(@NewSnapshotId AS VARCHAR(10))
    
    -- Return the new SnapshotId for further testing
    SELECT @NewSnapshotId as NewSnapshotId
END
GO

-- ==================================================================
-- Example usage and testing
-- ==================================================================
PRINT 'Testing the data generator with small dataset...'

-- Test with small parameters
EXEC GenerateTestData 
    @NumSubjects = 2,
    @NumZones = 4, 
    @NumQuestions = 12,
    @NumTests = 3

PRINT ''
PRINT 'Data generator testing completed!'
GO 