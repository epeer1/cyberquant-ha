Dear applicant,
In this home assignment,
You will find attached Grades DB, which contains the following tables: 
Subjects: The actual thing that is being tested (for example Math, Hebrew)
Zones:  A sub subject / module (for example algebra, geometry)
Questions: The actual question used to assess the skill (for example “1+1”).
Tests: Every question can only be a part of one test, and a test is the source of questions. 
Test has a special marker called A test (for national scoring)
The system allows teachers to assess their student’s skill level. 
The system follows snapshots principal approach:
-	New snapshot is created for each student assessment.
-	SnapshotId 0 will hold the catalog version of the assignment.
-	All the relationships that are stored in the table are duplicated (given a new SnapshotId).
Questions score scale between 0 and 100. 
A question that was not answered has the value NULL in the score column.
A question can be related to multiple zones.
After a new snapshot is created, the teacher can decide if the zone of questions is relevant for his future test. 
Your Goals:
1.	Create a new subject, multiple new zones and questions linked to a new test in the catalog snapshot. 
Name this script simple_migration_script.sql, it’s meant to be a static data migration.
  
Bonus: Create a procedure that automates the previous step.
Meaning that a user can supply the number of subjects, zones, questions, and tests, and all the new entities would be created with valid links, auto-generated names. Once in the catalog snapshot (snapshot 0), and once as a new snapshot with random question scores.
If supplied name this script data_generator.sql. 

2.	For a specific assessment (snapshot), calculate the score of a subject for national scoring tests and nonnational tests (using a stored procedure), the number of questions that are related to that subject (per type of test), and the number of answered questions (per type of test). 
Result query output columns: 
a.	SnpashotId 
b.	SubjectId
c.	SubjectName 
d.	NumNationalQuestions 
e.	NumNationalAnswerdQuestions
f.	NationalTestScores
g.	NumNonNationalQuestions
h.	NumNonNationalAnsweredQuestions
i.	NonNationalTestScores

The score of each subject is the average score of the relevant zones connected to it.
The score of each zone is the average score of the relevant questions connected to it.
Name this script calculate_score_per_snapshot.sql.

For the sections below please provide BE code implementation

3.	In this section you are required to create a REST API with the following functionality.
Basic CRUD ability against questions table for specific snapshot. 
•	Get all snapshot questions, 
•	Update question -  name, IsRelevant and score values (score can’t be negative) 
•	Remove a question by snapshot 
•	Create a new question for a snapshot.
4.	Generate “Student Report“ from an API endpoint 
For a given snapshot we want to generate a “Student report”. A student report holds: 
•	Top 3 zones with their calculated scores (section 2) 
•	Bottom 3 zones with their calculated 
•	All zones with a score lower than 60, 
•	Report creation date 
•	Report Title “Student report”.
Please return a JSON response containing all specified fields.
5.	Add “Principal Report” - for a given list of snapshot ids, the system should generate a report which holds 
•	Zone with the lowest average score
•	Report title “Principal Report” 
•	Report creation date 
Please return a JSON response containing all specified fields.

Guidance:
You are given an example of Grades DB (SQL express 2019), it’s highly recommended to restore the backup to the same sqlexpress database version using your favorite management tool.
Project should include 3 parts:
•	Server side (WebAPI .net version 4.8)
•	Database (SqlServer)

 
Please provide:
Link to a GitHub repo containing 
-	.sql files for sections 1 and 2
-	Solution for sections 3,4,5

GOOD LUCK 😊
