# Database Relationships - How Tables Connect

## 🔗 Overview of All Relationships

The GRADES database has 6 tables connected through various relationships. Here's how they all fit together:

```
Subjects ←→ SubjectZones ←→ Zones ←→ ZonesQuestions ←→ Questions
                                                        ↑
                                                      Tests
```

## 📋 Relationship Types

### 1. **Subjects ↔ Zones** (Many-to-Many via SubjectZones)
- **One subject** can have **many zones**
- **One zone** can belong to **many subjects** (though rare in practice)
- **Junction table:** `SubjectZones`

### 2. **Zones ↔ Questions** (Many-to-Many via ZonesQuestions)  
- **One zone** can have **many questions**
- **One question** can belong to **many zones** (this is common!)
- **Junction table:** `ZonesQuestions`

### 3. **Tests → Questions** (One-to-Many)
- **One test** can have **many questions**
- **One question** belongs to **exactly one test**
- **Direct relationship:** `Questions.TestId → Tests.TestId`

## 🎯 Detailed Relationship Diagrams

### Relationship 1: Subjects ↔ Zones
```
┌─────────────┐     ┌─────────────────┐     ┌─────────────┐
│  Subjects   │────▶│  SubjectZones   │◀────│    Zones    │
├─────────────┤     ├─────────────────┤     ├─────────────┤
│ SnapshotId  │     │ SnapshotId      │     │ SnapshotId  │
│ SubjectId   │─────│ SubjectId       │     │ ZoneId      │
│ SubjectName │     │ ZoneId          │─────│ ZoneName    │
└─────────────┘     └─────────────────┘     │ IsRelevant  │
                                            └─────────────┘
```

**Example:**
```
Subject: "Mathematics" (ID: 2)
  ├── Zone: "Algebra" (ID: 10)
  ├── Zone: "Geometry" (ID: 11)  
  └── Zone: "Statistics" (ID: 12)

SubjectZones table:
SnapshotId | SubjectId | ZoneId
0          | 2         | 10      ← Math has Algebra
0          | 2         | 11      ← Math has Geometry  
0          | 2         | 12      ← Math has Statistics
```

### Relationship 2: Zones ↔ Questions
```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│    Zones    │────▶│ ZonesQuestions   │◀────│  Questions  │
├─────────────┤     ├──────────────────┤     ├─────────────┤
│ SnapshotId  │     │ SnapshotId       │     │ SnapshotId  │
│ ZoneId      │─────│ ZoneId           │     │ QuestionId  │
│ ZoneName    │     │ QuestionId       │─────│ QuestionText│
│ IsRelevant  │     └──────────────────┘     │ Score       │
└─────────────┘                              │ IsRelevant  │
                                             │ TestId      │
                                             └─────────────┘
```

**Example - Question Can Be in Multiple Zones:**
```
Question: "Solve: 2x + 3 = 7" (ID: 500)
  ├── Zone: "Algebra" (ID: 10)
  ├── Zone: "Basic Math" (ID: 13)
  └── Zone: "Problem Solving" (ID: 15)

ZonesQuestions table:
SnapshotId | ZoneId | QuestionId
0          | 10     | 500        ← Algebra has this question
0          | 13     | 500        ← Basic Math also has this question
0          | 15     | 500        ← Problem Solving also has this question
```

### Relationship 3: Tests → Questions
```
┌─────────────┐     ┌─────────────┐
│    Tests    │────▶│  Questions  │
├─────────────┤     ├─────────────┤
│ TestId      │─────│ TestId      │
│ TestName    │     │ SnapshotId  │
│ IsATest     │     │ QuestionId  │
└─────────────┘     │ QuestionText│
                    │ Score       │
                    │ IsRelevant  │
                    └─────────────┘
```

**Example:**
```
Test: "Algebra Midterm" (ID: 25, IsATest: 1)
  ├── Question: "What is x in x+5=10?" (ID: 501)
  ├── Question: "Solve: 2y-3=7" (ID: 502)
  └── Question: "Factor: x²-9" (ID: 503)

Questions table:
SnapshotId | QuestionId | QuestionText        | TestId
0          | 501        | "What is x+5=10?"   | 25
0          | 502        | "Solve: 2y-3=7"     | 25  
0          | 503        | "Factor: x²-9"      | 25
```

## 🎯 Complete Relationship Flow

### How Everything Connects Together:
```
Subject: "Mathematics"
    ↓ (via SubjectZones)
Zone: "Algebra" 
    ↓ (via ZonesQuestions)
Question: "What is x+5=10?"
    ↓ (belongs to)
Test: "Algebra Midterm" (National Test)
```

### Real Example with All Tables:
```
1. Subject "Mathematics" (SubjectId: 2)
2. Zone "Algebra" (ZoneId: 10) 
3. Question "What is x+5=10?" (QuestionId: 501)
4. Test "Algebra Midterm" (TestId: 25, IsATest: 1)

Table Data:
Subjects:       SnapshotId=0, SubjectId=2, SubjectName="Mathematics"
SubjectZones:   SnapshotId=0, SubjectId=2, ZoneId=10  
Zones:          SnapshotId=0, ZoneId=10, ZoneName="Algebra"
ZonesQuestions: SnapshotId=0, ZoneId=10, QuestionId=501
Questions:      SnapshotId=0, QuestionId=501, QuestionText="What is x+5=10?", TestId=25
Tests:          TestId=25, TestName="Algebra Midterm", IsATest=1
```

## 📊 Snapshot Duplication Pattern

**Key Insight:** All relationships are duplicated across snapshots!

### Catalog (SnapshotId = 0):
```
SubjectZones:   0 | 2 | 10    ← Math → Algebra  
ZonesQuestions: 0 | 10| 501   ← Algebra → Question
Questions:      0 | 501| ... | NULL | TestId=25
```

### Student Assessment (SnapshotId = 1001190):
```
SubjectZones:   1001190 | 2 | 10    ← SAME relationship copied
ZonesQuestions: 1001190 | 10| 501   ← SAME relationship copied  
Questions:      1001190 | 501| ... | 85 | TestId=25  ← Now has score!
```

## 🔍 Why These Relationships Matter for the Assignment

### For Score Calculation (Part 2):
To calculate a subject score for a snapshot, you need to follow this path:
```sql
Subjects → SubjectZones → Zones → ZonesQuestions → Questions → Tests
                                                      ↓
                                    Get scores, group by IsATest
```

### For Questions API (Part 3):
To get questions for a snapshot:
```sql
Questions WHERE SnapshotId = X
```

To delete a question safely:
```sql
DELETE FROM ZonesQuestions WHERE SnapshotId = X AND QuestionId = Y
DELETE FROM Questions WHERE SnapshotId = X AND QuestionId = Y
```

### For Reports (Parts 4 & 5):
To calculate zone scores:
```sql
Zones → ZonesQuestions → Questions
        ↓
    AVG(Score) GROUP BY ZoneId
```

## 💡 Key Relationship Rules

1. **Every table has SnapshotId** (except Tests)
2. **Questions MUST have a valid TestId**  
3. **Questions can be in multiple zones** (via ZonesQuestions)
4. **Zones can be in multiple subjects** (via SubjectZones) 
5. **All relationships are snapshot-specific**

This relationship structure allows maximum flexibility while maintaining data integrity!

## 🎯 Real Data Example from Your Database

Based on the actual data we found in your GRADES database, here's how the relationships work with real records:

### The Complete Chain:
```
Subject: "Default-Subject" (ID: 1)
    ↓ (via SubjectZones)
Zones: "zone number:1" (ID: 447), "zone number:2" (ID: 448), etc.
    ↓ (via ZonesQuestions) 
Questions: "question number1" (ID: 308), "question number2" (ID: 309), etc.
    ↓ (belongs to)
Test: "test number2" (ID: 6, IsATest: 0 - Regular Test)
```

### Actual Table Records:

**Subjects Table:**
```
SnapshotId | SubjectId | SubjectName
0          | 1         | "Default-Subject"
```

**Zones Table:**
```
SnapshotId | ZoneId | ZoneName        | IsRelevant
0          | 447    | "zone number:1" | 1
0          | 448    | "zone number:2" | 1  
0          | 449    | "zone number:3" | 1
```

**Questions Table:**
```
SnapshotId | QuestionId | QuestionText      | Score | IsRelevant | TestId
0          | 308        | "question number1"| NULL  | 1          | 6
0          | 309        | "question number2"| NULL  | 1          | 6
0          | 310        | "question number3"| NULL  | 1          | 6
```

**Tests Table:**
```
TestId | TestName       | IsATest
6      | "test number2" | 0        ← Regular Test (not national)
```

**SubjectZones Table (Junction):**
```
SnapshotId | SubjectId | ZoneId
0          | 1         | 447     ← "Default-Subject" contains "zone number:1"
0          | 1         | 448     ← "Default-Subject" contains "zone number:2"  
0          | 1         | 449     ← "Default-Subject" contains "zone number:3"
```

**ZonesQuestions Table (Junction):**
```
SnapshotId | ZoneId | QuestionId
0          | 447    | 308        ← "zone number:1" contains "question number1"
0          | 447    | 309        ← "zone number:1" ALSO contains "question number2"
0          | 448    | 310        ← "zone number:2" contains "question number3"
```

### Key Insights from Real Data:

1. **One Subject, Multiple Zones:** "Default-Subject" has 10 zones (447-456)
2. **Questions Span Multiple Zones:** Question 308 and 309 are both in zone 447
3. **All in Catalog:** Everything shown is SnapshotId = 0 (catalog/template)
4. **Regular Test:** Test 6 is a regular test (IsATest = 0), not national scoring
5. **No Scores Yet:** All questions have NULL scores (this is the catalog)

### What Happens in Student Assessment:

When a student takes an assessment (e.g., SnapshotId = 1001190):
```
Questions Table:
SnapshotId | QuestionId | QuestionText      | Score | IsRelevant | TestId
1001190    | 308        | "question number1"| 85    | 1          | 6      ← Now has score!
1001190    | 309        | "question number2"| 72    | 1          | 6      ← Now has score!
1001190    | 310        | "question number3"| NULL  | 0          | 6      ← Not answered, marked irrelevant
```

All the junction table relationships get copied too, but with the new SnapshotId! 