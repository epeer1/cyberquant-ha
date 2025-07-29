-- ==================================================================
-- simple_migration_script.sql
-- 
-- Assignment Part 1: Create new subject, zones, questions and tests
-- Target: Catalog snapshot (SnapshotId = 0)
-- ==================================================================

USE [Grades]
GO

-- Variables for new IDs (get next available IDs)
DECLARE @NewSubjectId INT = (SELECT ISNULL(MAX(SubjectId), 0) + 1 FROM Subjects)
DECLARE @NewZoneId1 INT = (SELECT ISNULL(MAX(ZoneId), 0) + 1 FROM Zones)
DECLARE @NewZoneId2 INT = @NewZoneId1 + 1
DECLARE @NewZoneId3 INT = @NewZoneId1 + 2
DECLARE @NewTestId INT = (SELECT ISNULL(MAX(TestId), 0) + 1 FROM Tests)
DECLARE @NewQuestionId INT = (SELECT ISNULL(MAX(QuestionId), 0) + 1 FROM Questions)

PRINT 'Starting migration...'
PRINT 'New SubjectId: ' + CAST(@NewSubjectId AS VARCHAR(10))
PRINT 'New ZoneIds: ' + CAST(@NewZoneId1 AS VARCHAR(10)) + ', ' + CAST(@NewZoneId2 AS VARCHAR(10)) + ', ' + CAST(@NewZoneId3 AS VARCHAR(10))
PRINT 'New TestId: ' + CAST(@NewTestId AS VARCHAR(10))
PRINT 'Starting QuestionId: ' + CAST(@NewQuestionId AS VARCHAR(10))

-- ==================================================================
-- 1. CREATE NEW SUBJECT
-- ==================================================================
INSERT INTO Subjects (SnapshotId, SubjectId, SubjectName)
VALUES (0, @NewSubjectId, 'Mathematics')

PRINT 'Created Subject: Mathematics (ID: ' + CAST(@NewSubjectId AS VARCHAR(10)) + ')'

-- ==================================================================
-- 2. CREATE NEW TEST (National Test)
-- ==================================================================
INSERT INTO Tests (TestId, TestName, IsATest)
VALUES (@NewTestId, 'Mathematics Assessment', 1) -- IsATest = 1 (National)

PRINT 'Created Test: Mathematics Assessment (ID: ' + CAST(@NewTestId AS VARCHAR(10)) + ', National)'

-- ==================================================================
-- 3. CREATE NEW ZONES
-- ==================================================================
INSERT INTO Zones (SnapshotId, ZoneId, ZoneName, IsRelevant)
VALUES 
    (0, @NewZoneId1, 'Algebra', 1),
    (0, @NewZoneId2, 'Geometry', 1),
    (0, @NewZoneId3, 'Statistics', 1)

PRINT 'Created 3 Zones: Algebra, Geometry, Statistics'

-- ==================================================================
-- 4. LINK ZONES TO SUBJECT
-- ==================================================================
INSERT INTO SubjectZones (SnapshotId, SubjectId, ZoneId)
VALUES 
    (0, @NewSubjectId, @NewZoneId1), -- Math -> Algebra
    (0, @NewSubjectId, @NewZoneId2), -- Math -> Geometry  
    (0, @NewSubjectId, @NewZoneId3)  -- Math -> Statistics

PRINT 'Linked all zones to Mathematics subject'

-- ==================================================================
-- 5. CREATE NEW QUESTIONS
-- ==================================================================
INSERT INTO Questions (SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId)
VALUES 
    -- Algebra Questions
    (0, @NewQuestionId + 0, 'Solve for x: 2x + 5 = 13', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 1, 'What is the value of y in: 3y - 7 = 8?', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 2, 'Factor: x² - 9', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 3, 'Solve: 4(x + 2) = 20', NULL, 1, @NewTestId),
    
    -- Geometry Questions  
    (0, @NewQuestionId + 4, 'What is the area of a rectangle with length 8 and width 5?', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 5, 'Calculate the circumference of a circle with radius 6', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 6, 'Find the perimeter of a triangle with sides 3, 4, and 5', NULL, 1, @NewTestId),
    
    -- Statistics Questions
    (0, @NewQuestionId + 7, 'What is the mean of: 10, 15, 20, 25, 30?', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 8, 'Find the median of: 7, 3, 9, 1, 5, 8, 2', NULL, 1, @NewTestId),
    (0, @NewQuestionId + 9, 'Calculate the range of: 12, 8, 15, 3, 10', NULL, 1, @NewTestId)

PRINT 'Created 10 questions (Algebra: 4, Geometry: 3, Statistics: 3)'

-- ==================================================================
-- 6. LINK QUESTIONS TO ZONES
-- ==================================================================
INSERT INTO ZonesQuestions (SnapshotId, ZoneId, QuestionId)
VALUES 
    -- Algebra Zone Questions
    (0, @NewZoneId1, @NewQuestionId + 0),
    (0, @NewZoneId1, @NewQuestionId + 1),
    (0, @NewZoneId1, @NewQuestionId + 2),
    (0, @NewZoneId1, @NewQuestionId + 3),
    
    -- Geometry Zone Questions
    (0, @NewZoneId2, @NewQuestionId + 4),
    (0, @NewZoneId2, @NewQuestionId + 5),
    (0, @NewZoneId2, @NewQuestionId + 6),
    
    -- Statistics Zone Questions  
    (0, @NewZoneId3, @NewQuestionId + 7),
    (0, @NewZoneId3, @NewQuestionId + 8),
    (0, @NewZoneId3, @NewQuestionId + 9),
    
    -- Cross-zone questions (some questions belong to multiple zones)
    (0, @NewZoneId1, @NewQuestionId + 4), -- Rectangle area also involves algebra
    (0, @NewZoneId3, @NewQuestionId + 1)  -- Mean calculation involves algebra

PRINT 'Linked questions to zones (including cross-zone relationships)'

-- ==================================================================
-- 7. VERIFICATION QUERIES
-- ==================================================================
PRINT 'Migration completed! Verification:'

SELECT 'New Subject' as EntityType, SubjectId as ID, SubjectName as Name
FROM Subjects 
WHERE SubjectId = @NewSubjectId

UNION ALL

SELECT 'New Test', TestId, TestName 
FROM Tests 
WHERE TestId = @NewTestId

UNION ALL

SELECT 'New Zone', ZoneId, ZoneName
FROM Zones 
WHERE ZoneId IN (@NewZoneId1, @NewZoneId2, @NewZoneId3)

-- Count new questions
SELECT 'Question Count' as Info, COUNT(*) as Value
FROM Questions 
WHERE QuestionId BETWEEN @NewQuestionId AND @NewQuestionId + 9

-- Count new relationships
SELECT 'SubjectZone Links' as Info, COUNT(*) as Value  
FROM SubjectZones 
WHERE SubjectId = @NewSubjectId

SELECT 'ZoneQuestion Links' as Info, COUNT(*) as Value
FROM ZonesQuestions 
WHERE ZoneId IN (@NewZoneId1, @NewZoneId2, @NewZoneId3)

PRINT 'Migration script completed successfully!'
GO 