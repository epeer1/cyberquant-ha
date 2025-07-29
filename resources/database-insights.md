# Database Insights - Simple Overview

## What We Found in the GRADES Database

### 📋 Tables in the Database
1. **Questions** - The actual questions students answer
2. **Subjects** - Main topics like Math, Hebrew  
3. **Zones** - Sub-topics like Algebra, Geometry
4. **Tests** - Collections of questions
5. **SubjectZones** - Links subjects to zones
6. **ZonesQuestions** - Links zones to questions

### 🔑 Key Discovery: The Snapshot System

**SnapshotId = 0** → This is the "catalog" (like a template)
- Has 125,477 questions
- All scores are NULL (no actual scores yet)

**SnapshotId = 1001190** → Student assessment #1  
- Has 544 questions
- Has actual scores (some NULL for unanswered)

**SnapshotId = 1001191** → Student assessment #2
- Has 1,055 questions  
- Has actual scores (some NULL for unanswered)

### 🎯 National vs Regular Tests

**Tests table has `IsATest` column:**
- `IsATest = 1` → National test (for official scoring)
- `IsATest = 0` → Regular test (internal assessment)

This is **super important** for the assignment because we need to calculate scores separately!

### 📊 How Scoring Works

**Question Level:** Each question has a score 0-100 (or NULL if not answered)

**Zone Level:** Average of all question scores in that zone

**Subject Level:** Average of all zone scores in that subject

### 📁 Current Data Examples

**Subjects:** Only "Default-Subject" exists right now

**Zones:** "zone number:1", "zone number:2", etc. (10 zones total)

**Tests:** Mix like "test number1", "test number2" with different IsATest values

**Questions:** "question number1", "question number2", etc.

### 🎯 What This Means for the Assignment

**Part 1 (SQL Scripts):** We need to add NEW subjects, zones, questions in SnapshotId = 0

**Part 2 (Score Calculation):** We calculate scores for a specific SnapshotId, separating National vs Regular tests

**Part 3-5 (API):** We work with Questions table, always filtering by SnapshotId

### 💡 The Big Picture

When a student takes an assessment:
1. Copy relevant data from catalog (SnapshotId = 0) to new SnapshotId
2. Student answers questions → scores get filled in
3. Teacher can mark zones/questions as relevant or not
4. System calculates zone and subject scores based on the hierarchy

That's it! The database is designed to keep "before" and "after" versions of assessments using SnapshotId. 