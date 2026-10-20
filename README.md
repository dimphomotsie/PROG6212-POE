# PROG6212-POE
RaceDay

System Description

RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. The system allows users to register and log in according to their assigned role, manage their profiles, view events and categories, enrol in events, and manage event results. The system provides separate functionality for Organisers and Participants through role-based access control.

User Roles

Organiser

Organisers are responsible for managing road running, walking, and cycling events. They can create, update, view, and delete events, define categories for their events, view participant enrolments for their events, and capture participant results. Organiser functionality is restricted to authenticated users with the Organiser role.

Participant

Participants are users who take part in events managed through RaceDay. They can view available events and categories, enrol in events by selecting a category, view and update their own profile, and view their own event results. Participant functionality is restricted to authenticated users with the Participant role.

CI/CD

<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/bea9862b-df57-41b2-8394-6e861e2e6c7e" />


YouTube Video Link: 
https://youtu.be/sxA4HMAIwnk  
https://youtu.be/p_mwQ1dPA_g

Video Demonstration Summary (Due to no sound in Video)

ERD Walkthrough
My ERD for the RaceDay system consists of seven entities, exceeding the minimum requirement of six. The system is built around two distinct user roles — Organisers and Participants — and this split drives the entire data model. The User entity serves as the base for anyone who registers on the platform, holding the email, password hash, full name, and role. From there, a one-to-one relationship exists between User and Organiser, where the Organiser entity stores organiser-specific data such as the organisation name. This cardinality makes sense because not every user is an organiser; if I had placed organisation details directly on the Users table, those columns would be null for every Participant. The same logic applies to the one-to-one relationship between User and Participant, where the Participant entity holds date of birth and contact number — data only relevant to participants. Moving to the next relationship, an Organiser creates and manages many Events, while each Event belongs to exactly one Organiser. This one-to-many cardinality is essential for role-based access because each event carries a single OrganiserId foreign key, allowing the API to filter events by the logged-in organiser so they only see and manage their own events. Next, an Event offers many Categories, and each Category belongs to one Event. This one-to-many relationship is why categories cannot be a simple text field on the Event table — a single event like the Cape Town Cycle Tour has multiple categories such as Elite, Amateur 18 to 39, and Amateur 40 plus. A separate table gives each category its own identity and supports filtering, reporting, and the enrolment process. The most important relationship is between Participant and Event, which is a many-to-many relationship resolved through the Enrolments junction table. A Participant enrols in many Events, and an Event has many Participants enrolled. This is many-to-many because Sipho can enter both the Durban Marathon and the Pietermaritzburg Half Marathon, while Nomsa can also enter the Durban Marathon. The Enrolments table captures the ParticipantId, EventId, and critically, the CategoryId, because when you enter a race you must pick a category. I also added a unique constraint on Participant plus Event so a participant cannot enrol twice in the same event. This leads to the next relationship: each Enrolment selects one Category, and a Category is chosen for many Enrolments, which is a many-to-one cardinality. This links the participant's entry to the specific race category they signed up for and enforces that they can only pick a category that actually belongs to that event. Finally, an Enrolment may produce one Result, and a Result is recorded for one Enrolment, giving a one-to-zero-or-one cardinality. The zero-or-one is important because a participant might have enrolled but the event has not happened yet, so no result exists. Once the organiser captures a finish time and position, one Result row is created, and the unique constraint on EnrolmentId ensures you cannot record two results for one entry. Every table, key, and relationship in the ERD matches my SQL script exactly.

First Two API Endpoints
From my API endpoint plan, I will highlight the first two endpoints that anchor the authentication and user profile resource groups. The first is the register endpoint, POST /api/auth/register, which accepts email, password, full name, and role — either Organiser or Participant. This satisfies the Part 2 requirement that users must be able to register by providing personal details and selecting their role. It also feeds into the requirement that the password must be hashed and stored, never stored in plain text. The role required is None because you cannot be logged in before you register, and the expected response is 201 Created, or 400 Bad Request if the email already exists. The second endpoint is the user profile retrieval, GET /api/users/profile, which returns the currently authenticated user's profile details. The POE requires that both roles can view and update their own profile information, and this endpoint satisfies the view half, with a matching PUT endpoint handling updates. The role required is Any logged-in user, because both Organisers and Participants need it, and the expected response is 200 OK, or 401 Unauthorized if there is no valid session.

SQL Demo
To prove the database script works, I opened SQL Server Management Studio and connected to my local instance. I first showed that the database either did not exist or would be dropped and recreated by the script, ensuring a clean run every time. I then executed the full script by pressing F5 and waited for it to complete. The script ran with no errors. I scrolled down to show the verification output, where the summary listed Users, Organisers, Participants, Events, Categories, Enrolments, and Results with their record counts — four users comprising two Organisers and two Participants, three events, ten categories across those events, four enrolments, and two results for the completed Pietermaritzburg Half Marathon. I then expanded the Tables folder in Object Explorer to show all seven tables were created. I right-clicked the Users table and selected Top 1000 Rows to display the seeded data, showing the four users with their roles and BCrypt password hashes rather than plain text, which satisfies the password hashing requirement at the database level. I then displayed the Events table, showing the three events — the Durban City Marathon, the Pietermaritzburg Half Marathon, and the Cape Town Cycle Tour — with the correct event types of Run, Run, and Cycle. Finally, I displayed the Enrolments table, showing Sipho and Nomsa linked to their events and chosen categories with statuses of Confirmed and Pending, which proves the many-to-many relationship is working correctly through the junction table. This demonstrated that the script runs cleanly on a fresh instance, creates all tables with full constraints, and seeds realistic South African sample data.

## PROG6212 Part 2 

RaceDay – Event Management System

Overview
RaceDay is a full-stack web-based event management system for the South African
road running, walking and cycling community. It allows Event Organisers to
create and manage events, categories, and results, while Participants can
browse and enter events and track their performance history.

Roles
- Organiser – create, edit, and delete events; manage categories;
  capture participant results; view all enrolments for their events.
- Participant – browse events; enter an event by selecting a category;
  view their own enrolments and personal results.

Tech Stack
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core (Code-First) with SQL Server
- Session-based authentication with BCrypt password hashing
- Swagger / OpenAPI
- xUnit integration tests + FluentAssertions
- GitHub Actions CI/CD

Project Structure
docs/                 Part 1 planning documents (ERD, endpoint plan, SQL script)
RaceDayAPI/           Part 2 Web API
RaceDayAPI.Tests/     Integration tests
.github/workflows/    CI/CD


Setup & Run Instructions

Prerequisites
- .NET 8 SDK
- SQL Server Management (LocalDB)
- Visual Studio 2022 (or `dotnet` CLI)

Steps

1. Clone the repository. Open a terminal and run git clone <your-repo-url>, replacing <your-repo-url> with the actual GitHub URL of your repository. Then move into the project root folder by running cd RaceDay.

2. Set the connection string. Open the file RaceDayAPI/appsettings.json in a text editor. Locate the ConnectionStrings section and update the DefaultConnection entry to point at your SQL Server instance. The default uses LocalDB, so if you are using LocalDB no change is required. If you are using SQL Server Express, use Server=localhost\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;. If you are using SQL Server with a username and password, use Server=localhost;Database=RaceDayDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;. Save the file after making any changes.

3. Create the database. Navigate into the API project folder by running cd RaceDayAPI, then create the database by running dotnet ef database update. This applies the Entity Framework Core migration and creates the RaceDayDB database with all seven tables and their constraints. If the command fails with dotnet ef: command not found, first install the EF Core tools globally by running dotnet tool install --global dotnet-ef, then retry the command.

4. Run the API. Still inside the RaceDayAPI folder, run the API by executing dotnet run. The terminal will display the URLs the API is listening on, such as https://localhost:7209 and http://localhost:5093. Make a note of the HTTPS URL, as you will need it in the next step. To stop the API, press Ctrl+C.

5. Open Swagger. Open your browser and navigate to https://localhost:7209/swagger. If the browser warns about an invalid certificate because it is a self-signed development certificate, click Advanced and then Proceed to localhost. The Swagger UI will show every endpoint grouped by controller, and you can test any endpoint directly by expanding it and clicking Try it out.

6. Run the tests. Open a new terminal, navigate back to the solution root by running cd .., then run the test suite with the command dotnet test. You should see output confirming that all ten integration tests pass, such as Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10.


CI/CD

<img width="1920" height="1020" alt="Screenshot 2026-10-20 215646" src="https://github.com/user-attachments/assets/307c3597-6184-4803-aac7-f91fc937d2b7" />


Unit Testing

<img width="1920" height="1020" alt="Screenshot 2026-10-20 180706" src="https://github.com/user-attachments/assets/adbef441-3d6d-46c5-bcb1-34bad001ee18" />


Video Demonstration

https://youtu.be/dmirEh1LSQ0


Video Demonstration Text (Due to no audio)

This is my Part 2 submission for PROG6212, the RaceDay RESTful API. In this video I'll explain the structure of my API, show how authentication and role-based access work, walk through the endpoints in Swagger. Let's start with the project structure.

The solution has two projects. RaceDay_API is the Web API, and RaceDay_API.Tests is the test project. Inside the API project, there are six controllers; Auth, Users, Events, Categories, Enrolments, and Results. These map directly to the endpoint plan I submitted in Part 1. The Models/Entities folder contains the seven entities matching my Part 1 ERD; User, Organiser, Participant, Event, Category, Enrolment, and Result. Data/RaceDayDbContext configures all the relationships, unique constraints, and cascade rules. Filters/AuthorizeRoleAttribute is my custom role-based access filter, the core of access control. DTOs define the request and response shapes so entities are never exposed directly. Extensions/SessionExtensions provides helper methods for reading UserId and Role from the session. Now let me show you Swagger.

Swagger lists every endpoint grouped by controller, and each one shows its description, the request body it expects, and the response codes it returns. These descriptions come from XML comments in the code and ProducesResponseType attributes on each action. Let me explain the authentication flow. The register endpoint accepts email, password, full name, and role, either Organiser or Participant. If the role is Organiser, it also creates an Organiser record with an organisation name. If the role is Participant, it creates a Participant record with date of birth and contact number. The password is hashed with BCrypt before storage, never stored in plain text. The login endpoint accepts email and password, verifies the BCrypt hash, and then stores UserId, Role, and Email in the server-side session. From that point, every request carries a session cookie, so the API knows who the caller is and what role they have. The logout endpoint clears the session entirely. These three endpoints satisfy the POE requirement for authentication and session management.

Here's how role-based access is enforced. My custom AuthorizeRole attribute is applied to controllers and actions. On every request, it runs before the action method executes. It reads UserId and Role from the session. If there's no UserId, it returns 401 Unauthorized. If the caller's role isn't in the allowed list for that endpoint, it returns 403 Forbidden. This means access control is centralized. It's a cross-cutting concern handled in one place. The result is that Participants cannot call Organiser endpoints and vice versa. Both roles get 403 if they try.

The Users controller handles the profile endpoints. Both Organisers and Participants can view and update their own profile. GET /api/users/profile returns the logged-in user's details. For Organisers, that includes the organisation name. For Participants, it includes date of birth and contact number. The endpoint reads UserId from the session, so a user can only view their own profile, never another user's. PUT /api/users/profile allows updating those same fields. Only fields relevant to the caller's role are updated. Both endpoints require any logged-in user, and return 200 OK on success or 401 Unauthorized if not logged in.

The Events controller has five endpoints. GET /api/events returns all events, sorted by date, with organiser name, category count, and enrolment count. Available to both roles. GET /api/events/{id} returns a single event with its categories. POST /api/events creates a new event. It's restricted to Organisers only. The event is linked to the logged-in Organiser, the organiser's ID is read from the session, not from the request body, so a malicious caller can't forge ownership. PUT /api/events/{id} and DELETE /api/events/{id} do two layers of checking. The AuthorizeRole filter ensures only Organisers can call them. Then inside the action, it is verified that the Organiser making the request actually owns the event. If Organiser A tries to edit Organiser B's event, they get 403 Forbidden even though they're an Organiser. That's the ownership check, resource-level access on top of role-level access.

Categories are nested under events. POST /api/events/{id}/categories adds a category to a specific event, and the same ownership check applies, only the owning Organiser can add categories. Enrolments are where Participants interact with events. POST /api/events/{id}/enrolments allows a Participant to enrol by selecting a category. The endpoint reads ParticipantId from the session, validates that the category belongs to the event, and prevents duplicate enrolments. Participants get 201 Created on success, or 400 if already enrolled. GET /api/enrolments is interesting, it returns different data based on role. Participants see only their own enrolments. Organisers see all enrolments across their events, with participant names shown. Same endpoint, role-appropriate response. Results complete the flow. POST /api/enrolments/{id}/results lets an Organiser record a finish time and position for a specific enrolment — again with the ownership check. GET /api/participants/results lets a Participant view their personal race history with event name, date, category, finish time, and finishing position. Together, these six resource groups cover every functional requirement in the POE.

To summarise, the RaceDay API implements all planned endpoints from Part 1, uses session-based authentication with BCrypt password hashing, enforces role-based access through a custom filter with ownership checks on top, documents every endpoint in Swagger with clear descriptions and response codes, and passes all unit tests locally and in CI. Thank you for watching.
