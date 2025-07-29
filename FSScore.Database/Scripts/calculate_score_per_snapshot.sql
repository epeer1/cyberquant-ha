-- ==================================================================
-- calculate_score_per_snapshot.sql
-- 
-- Assignment Part 2: Calculate subject scores for a specific snapshot
-- Separates National tests (IsATest=1) from Regular tests (IsATest=0)
-- Output: Subject scores with question counts and averages by test type
-- ==================================================================

USE [Grades]
GO

-- ==================================================================
-- STORED PROCEDURE: Calculate Score Per Snapshot
-- ==================================================================
CREATE OR ALTER PROCEDURE [dbo].[CalculateScorePerSnapshot]
    @SnapshotId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    PRINT 'Calculating scores for SnapshotId: ' + CAST(@SnapshotId AS VARCHAR(10))
    
    -- Validate that snapshot exists
    IF NOT EXISTS (SELECT 1 FROM Questions WHERE SnapshotId = @SnapshotId)
    BEGIN
        PRINT 'ERROR: SnapshotId ' + CAST(@SnapshotId AS VARCHAR(10)) + ' not found in Questions table'
        RETURN
    END
    
    -- ==================================================================
    -- Main calculation query using CTEs
    -- ==================================================================
    ;WITH QuestionScores AS (
        -- Get all questions with their test type and scores
        SELECT 
            s.SnapshotId,
            s.SubjectId,
            s.SubjectName,
            z.ZoneId,
            q.QuestionId,
            q.Score,
            t.IsATest,
            CASE WHEN q.Score IS NOT NULL THEN 1 ELSE 0 END as IsAnswered
        FROM Subjects s
        INNER JOIN SubjectZones sz ON s.SnapshotId = sz.SnapshotId AND s.SubjectId = sz.SubjectId
        INNER JOIN Zones z ON sz.SnapshotId = z.SnapshotId AND sz.ZoneId = z.ZoneId
        INNER JOIN ZonesQuestions zq ON z.SnapshotId = zq.SnapshotId AND z.ZoneId = zq.ZoneId
        INNER JOIN Questions q ON zq.SnapshotId = q.SnapshotId AND zq.QuestionId = q.QuestionId
        INNER JOIN Tests t ON q.TestId = t.TestId
        WHERE s.SnapshotId = @SnapshotId
        AND z.IsRelevant = 1    -- Only include relevant zones
        AND q.IsRelevant = 1    -- Only include relevant questions
    ),
    ZoneScores AS (
        -- Calculate zone-level scores by test type
        SELECT 
            SnapshotId, 
            SubjectId, 
            SubjectName, 
            ZoneId, 
            IsATest,
            COUNT(QuestionId) as TotalQuestions,
            SUM(IsAnswered) as AnsweredQuestions,
            AVG(CASE WHEN Score IS NOT NULL THEN CAST(Score AS FLOAT) END) as ZoneScore
        FROM QuestionScores
        GROUP BY SnapshotId, SubjectId, SubjectName, ZoneId, IsATest
    ),
    SubjectScores AS (
        -- Calculate subject-level scores by test type
        SELECT 
            SnapshotId, 
            SubjectId, 
            SubjectName, 
            IsATest,
            SUM(TotalQuestions) as TotalQuestions,
            SUM(AnsweredQuestions) as AnsweredQuestions,
            AVG(ZoneScore) as SubjectScore
        FROM ZoneScores
        WHERE ZoneScore IS NOT NULL  -- Only include zones that have answered questions
        GROUP BY SnapshotId, SubjectId, SubjectName, IsATest
    )
    -- ==================================================================
    -- Final result: Join National and Non-National scores
    -- ==================================================================
    SELECT 
        ISNULL(n.SnapshotId, r.SnapshotId) as SnapshotId,
        ISNULL(n.SubjectId, r.SubjectId) as SubjectId,
        ISNULL(n.SubjectName, r.SubjectName) as SubjectName,
        
        -- National Test Columns
        ISNULL(n.TotalQuestions, 0) as NumNationalQuestions,
        ISNULL(n.AnsweredQuestions, 0) as NumNationalAnsweredQuestions,
        ROUND(n.SubjectScore, 2) as NationalTestScores,
        
        -- Non-National Test Columns  
        ISNULL(r.TotalQuestions, 0) as NumNonNationalQuestions,
        ISNULL(r.AnsweredQuestions, 0) as NumNonNationalAnsweredQuestions,
        ROUND(r.SubjectScore, 2) as NonNationalTestScores
        
    FROM (SELECT * FROM SubjectScores WHERE IsATest = 1) n  -- National tests
    FULL OUTER JOIN (SELECT * FROM SubjectScores WHERE IsATest = 0) r  -- Regular tests
        ON n.SnapshotId = r.SnapshotId AND n.SubjectId = r.SubjectId
    ORDER BY ISNULL(n.SubjectId, r.SubjectId)
    
    PRINT 'Score calculation completed successfully'
END
GO

-- ==================================================================
-- Example usage and testing
-- ==================================================================
PRINT 'Testing the stored procedure with existing snapshots...'

-- Test with student assessment snapshot 1001190
PRINT ''
PRINT '=== Testing with SnapshotId 1001190 ==='
EXEC CalculateScorePerSnapshot @SnapshotId = 1001190

-- Test with student assessment snapshot 1001191  
PRINT ''
PRINT '=== Testing with SnapshotId 1001191 ==='
EXEC CalculateScorePerSnapshot @SnapshotId = 1001191

-- Test with catalog (should show 0 scores since all NULL)
PRINT ''
PRINT '=== Testing with Catalog (SnapshotId 0) ==='
EXEC CalculateScorePerSnapshot @SnapshotId = 0

-- Test with non-existent snapshot
PRINT ''
PRINT '=== Testing with Non-existent SnapshotId ==='
EXEC CalculateScorePerSnapshot @SnapshotId = 999999

PRINT ''
PRINT 'All tests completed!'
GO 