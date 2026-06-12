# PRM System — UML Diagrams

> All diagrams derived from [PROJECT_CONTEXT.md](file:///c:/Users/jayesh.s.lodha/OneDrive%20-%20InTimeTec%20Visionsoft%20Pvt.%20Ltd.,/Documents/project-resource-manager/PROJECT_CONTEXT.md) and [PRM_BRD_V4.md](file:///c:/Users/jayesh.s.lodha/OneDrive%20-%20InTimeTec%20Visionsoft%20Pvt.%20Ltd.,/Documents/project-resource-manager/PRM_BRD_V4.md).

---

## 1. Class Diagram

Shows the domain entities (PRM.Core), service/repository interfaces, and their relationships across Clean Architecture layers.

```mermaid
classDiagram
    direction TB

    %% ─────────────── ENUMS ───────────────
    class EmployeeStatus {
        <<enumeration>>
        Allocated
        Bench
    }

    class ProjectStatus {
        <<enumeration>>
        Planned
        Active
        OnHold
        Completed
    }

    class MilestoneStatus {
        <<enumeration>>
        NotStarted
        InProgress
        Done
    }

    class HealthStatus {
        <<enumeration>>
        OnTrack
        Attention
        AtRisk
    }

    class TimesheetStatus {
        <<enumeration>>
        Submitted
        Missed
    }

    class SkillCategory {
        <<enumeration>>
        Backend
        Frontend
        DevOps
        QA
        Other
    }

    class ProficiencyLevel {
        <<enumeration>>
        Beginner
        Intermediate
        Advanced
    }

    %% ─────────────── DOMAIN ENTITIES ───────────────
    class Permission {
        +int Id
        +string Name
        +string Description
    }

    class RolePermission {
        +int RoleId
        +int PermissionId
        +Role Role
        +Permission Permission
    }

    class Role {
        +int Id
        +string Name
        +string Description
        +List~User~ Users
        +List~RolePermission~ RolePermissions
    }

    class User {
        +int Id
        +string Username
        +string Email
        +string PasswordHash
        +int RoleId
        +Role Role
        +string FullName
        +string Department
        +int ManagerId
        +bool IsActive
        +bool ForcePasswordChange
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +User Manager
        +List~User~ DirectReports
        +List~UserSkill~ UserSkills
        +List~Allocation~ Allocations
        +List~Timesheet~ Timesheets
    }

    class Skill {
        +int Id
        +string Name
        +SkillCategory Category
    }

    class UserSkill {
        +int UserId
        +int SkillId
        +ProficiencyLevel ProficiencyLevel
        +User User
        +Skill Skill
    }

    class Project {
        +int Id
        +string Name
        +string Description
        +DateTime StartDate
        +DateTime EndDate
        +ProjectStatus Status
        +int ManagerId
        +int TotalStoryPoints
        +HealthStatus HealthStatus
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +User Manager
        +List~Milestone~ Milestones
        +List~Allocation~ Allocations
    }

    class Milestone {
        +int Id
        +int ProjectId
        +string Title
        +DateTime DueDate
        +int StoryPoints
        +MilestoneStatus Status
        +Project Project
    }

    class Allocation {
        +int Id
        +int UserId
        +int ProjectId
        +int UtilisationPercent
        +DateTime FromDate
        +DateTime ToDate
        +User User
        +Project Project
    }

    class Timesheet {
        +int Id
        +int UserId
        +DateTime WeekStartDate
        +TimesheetStatus Status
        +DateTime SubmittedAt
        +User User
        +List~TimesheetEntry~ Entries
    }

    class TimesheetEntry {
        +int Id
        +int TimesheetId
        +int ProjectId
        +decimal HoursWorked
        +string ActivityTags
        +Timesheet Timesheet
        +Project Project
    }

    class SystemConfig {
        +string Key
        +string Value
    }

    %% ─────────────── RELATIONSHIPS ───────────────
    Role "1" -- "0..*" RolePermission : has
    Permission "1" -- "0..*" RolePermission : granted via
    Role "1" -- "0..*" User : assigned to
    User "0..1" -- "0..*" User : manages
    User "1" -- "0..*" UserSkill : possesses
    Skill "1" -- "0..*" UserSkill : assigned to
    User "1" -- "0..*" Allocation : allocated via
    Project "1" -- "0..*" Allocation : resources
    Project "1" -- "0..*" Milestone : broken into
    User "1" -- "0..*" Timesheet : submits
    Timesheet "1" -- "1..*" TimesheetEntry : contains
    TimesheetEntry "0..*" -- "1" Project : logs hours on
    Project "0..*" -- "1" User : managed by

    %% ─────────────── SERVICE INTERFACES ───────────────
    class IUserRepository {
        <<interface>>
        +GetByIdAsync(id) User
        +GetAllAsync(filter) List~User~
        +AddAsync(user) void
        +UpdateAsync(user) void
        +DeactivateAsync(id) void
    }

    class IProjectRepository {
        <<interface>>
        +GetByIdAsync(id) Project
        +GetAllAsync() List~Project~
        +GetByManagerIdAsync(managerId) List~Project~
        +AddAsync(project) void
        +UpdateAsync(project) void
    }

    class IAllocationRepository {
        <<interface>>
        +GetByEmployeeIdAsync(employeeId) List~Allocation~
        +GetByProjectIdAsync(projectId) List~Allocation~
        +GetOverlapping(employeeId, from, to) List~Allocation~
        +AddAsync(allocation) void
        +EndAllocationAsync(id, endDate) void
    }

    class ITimesheetRepository {
        <<interface>>
        +GetByEmployeeAndWeekAsync(employeeId, weekStart) Timesheet
        +GetByEmployeeIdAsync(employeeId) List~Timesheet~
        +AddAsync(timesheet) void
    }

    class ILlmProvider {
        <<interface>>
        +GetSkillMatchAsync(requirement, candidates) SkillMatchResult
        +GetRiskSummaryAsync(projectData) string
    }

    class IUserService {
        <<interface>>
        +GetAllUsersAsync(filter) List~UserDto~
        +UpdateUserAsync(dto) void
        +DeactivateUserAsync(id) void
        +ManageSkillsAsync(userId, skillDto) void
        +AssignManagerAsync(userId, managerId) void
    }

    class IProjectService {
        <<interface>>
        +CreateProjectAsync(dto) ProjectDto
        +UpdateProjectAsync(dto) void
        +GetProjectHealthAsync(projectId) ProjectHealthDto
        +ManageMilestonesAsync(projectId, milestoneDto) void
    }

    class IAllocationService {
        <<interface>>
        +AllocateResourceAsync(dto) AllocationDto
        +EndAllocationAsync(allocationId) void
        +ValidateAllocationAsync(dto) ValidationResult
    }

    class ITimesheetService {
        <<interface>>
        +SubmitTimesheetAsync(dto) TimesheetDto
        +GetTimesheetsAsync(employeeId) List~TimesheetDto~
        +GetTeamTimesheetsAsync(managerId, week) List~TimesheetDto~
    }

    class IAiService {
        <<interface>>
        +FindSkillMatchAsync(requirement, projectId) List~MatchResultDto~
        +GenerateRiskSummaryAsync(projectId) string
    }

    class IAuthService {
        <<interface>>
        +LoginAsync(username, password) AuthResult
        +ChangePasswordAsync(userId, newPassword) void
        +ResetPasswordAsync(userId, tempPassword) void
    }

    %% ─────────────── LAYER CONNECTIONS ───────────────
    IUserService ..> IUserRepository : uses
    IProjectService ..> IProjectRepository : uses
    IAllocationService ..> IAllocationRepository : uses
    ITimesheetService ..> ITimesheetRepository : uses
    IAiService ..> ILlmProvider : delegates to
    IAllocationService ..> IUserRepository : checks capacity
```

---

## 2. ER Diagram

Full database schema with tables, columns, types, primary keys, foreign keys, and cardinalities.

```mermaid
erDiagram
    PERMISSIONS {
        int permission_id PK
        varchar name UK "NOT NULL, UNIQUE"
        varchar description "NULL"
    }

    ROLE_PERMISSIONS {
        int role_id PK, FK "-> ROLES"
        int permission_id PK, FK "-> PERMISSIONS"
    }

    ROLES {
        int role_id PK
        varchar name UK "NOT NULL, UNIQUE"
        varchar description "NULL"
    }

    USERS {
        int user_id PK
        varchar username UK "NOT NULL, UNIQUE"
        varchar email UK "NOT NULL, UNIQUE"
        varchar password_hash "NOT NULL"
        int role_id FK "NOT NULL -> ROLES"
        varchar full_name "NOT NULL"
        varchar department "NULL"
        int manager_id FK "NULL -> USERS self-ref"
        bit is_active "DEFAULT 1"
        bit force_password_change "DEFAULT 1"
        datetime created_at "NOT NULL"
        datetime updated_at "NULL"
    }

    SKILLS {
        int skill_id PK
        varchar name UK "NOT NULL, UNIQUE"
        varchar category "NOT NULL (Backend/Frontend/DevOps/QA/Other)"
    }

    USER_SKILLS {
        int user_id PK, FK "-> USERS"
        int skill_id PK, FK "-> SKILLS"
        varchar proficiency_level "NOT NULL (Beginner/Intermediate/Advanced)"
    }

    PROJECTS {
        int project_id PK
        varchar name "NOT NULL"
        text description "NULL"
        date start_date "NOT NULL"
        date end_date "NOT NULL"
        varchar status "NOT NULL (Planned/Active/OnHold/Completed)"
        int manager_id FK "NULL -> USERS"
        int total_story_points "NOT NULL"
        varchar health_status "DEFAULT OnTrack"
        datetime created_at "NOT NULL"
        datetime updated_at "NULL"
    }

    MILESTONES {
        int milestone_id PK
        int project_id FK "NOT NULL -> PROJECTS"
        varchar title "NOT NULL"
        date due_date "NOT NULL"
        int story_points "NOT NULL"
        varchar status "NOT NULL (NotStarted/InProgress/Done)"
    }

    ALLOCATIONS {
        int allocation_id PK
        int user_id FK "NOT NULL -> USERS"
        int project_id FK "NOT NULL -> PROJECTS"
        int utilisation_percent "NOT NULL 1-100"
        date from_date "NOT NULL"
        date to_date "NOT NULL"
    }

    TIMESHEETS {
        int timesheet_id PK
        int user_id FK "NOT NULL -> USERS"
        date week_start_date "NOT NULL"
        varchar status "NOT NULL (Submitted/Missed)"
        datetime submitted_at "NULL"
    }

    TIMESHEET_ENTRIES {
        int entry_id PK
        int timesheet_id FK "NOT NULL -> TIMESHEETS"
        int project_id FK "NOT NULL -> PROJECTS"
        decimal hours_worked "NOT NULL"
        varchar activity_tags "NULL"
    }

    SYSTEM_CONFIG {
        varchar config_key PK
        varchar config_value "NOT NULL"
    }

    %% ─────────────── RELATIONSHIPS ───────────────
    ROLES ||--o{ ROLE_PERMISSIONS : "has"
    PERMISSIONS ||--o{ ROLE_PERMISSIONS : "granted via"
    ROLES ||--o{ USERS : "assigned to"
    USERS ||--o{ USERS : "manages self-ref"
    USERS ||--o{ USER_SKILLS : "possesses"
    SKILLS ||--o{ USER_SKILLS : "assigned to"
    USERS ||--o{ ALLOCATIONS : "allocated via"
    PROJECTS ||--o{ ALLOCATIONS : "resourced by"
    PROJECTS ||--o{ MILESTONES : "broken into"
    USERS ||--o{ TIMESHEETS : "submits"
    TIMESHEETS ||--|{ TIMESHEET_ENTRIES : "contains"
    PROJECTS ||--o{ TIMESHEET_ENTRIES : "hours logged on"
    USERS ||--o{ PROJECTS : "manages"
```

---

## 3. Use Case Diagram

All use cases grouped by actor — Admin, Manager, Employee, Background Scheduler, and LLM Provider.

```mermaid
flowchart TB
    subgraph System["PRM System"]
        direction TB

        subgraph Auth["Authentication"]
            UC_LOGIN["Login"]
            UC_CHANGE_PWD["Change Password - forced on first login"]
            UC_LOGOUT["Logout"]
        end

        subgraph AdminUC["Admin Use Cases"]
            UC_CREATE_USER["Create User Account"]
            UC_VIEW_USERS["View All Users"]
            UC_RESET_PWD["Reset User Password"]
            UC_DEACTIVATE_USER["Deactivate User"]
            UC_REACTIVATE_USER["Reactivate User"]
            UC_VIEW_EMPLOYEES["View All Employees"]
            UC_UPDATE_EMPLOYEE["Update Employee"]
            UC_DEACTIVATE_EMPLOYEE["Deactivate Employee"]
            UC_MANAGE_SKILLS["Manage Employee Skills"]
            UC_ASSIGN_MANAGER["Assign Manager"]
            UC_CREATE_PROJECT["Create Project"]
            UC_VIEW_PROJECTS["View All Projects"]
            UC_UPDATE_PROJECT["Update Project Details"]
            UC_MANAGE_MILESTONES["Manage Milestones"]
            UC_VIEW_ALLOCATIONS_ADMIN["View All Allocations"]
            UC_SYS_CONFIG["System Configuration"]
        end

        subgraph ManagerUC["Manager Use Cases"]
            UC_RESOURCE_DASHBOARD["Resource Dashboard"]
            UC_DRILL_EMPLOYEE["Drill into Employee Details"]
            UC_AI_SKILL_MATCH["AI Skill Match"]
            UC_DIRECT_ALLOCATE["Direct Allocation"]
            UC_END_ALLOCATION["End Allocation"]
            UC_MY_PROJECTS["My Projects"]
            UC_PROJECT_HEALTH["View Project Health"]
            UC_AI_RISK_SUMMARY["AI Risk Summary"]
            UC_VIEW_TEAM_TIMESHEETS["View Team Timesheets"]
        end

        subgraph EmployeeUC["Employee Use Cases"]
            UC_SUBMIT_TIMESHEET["Submit Timesheet"]
            UC_VIEW_MY_TIMESHEETS["View My Timesheets"]
            UC_VIEW_MY_ALLOCATIONS["View My Allocations"]
        end

        subgraph BackgroundUC["Background Scheduler"]
            UC_COMPUTE_UTILISATION["Compute Utilisation"]
            UC_FLAG_HEALTH["Flag Project Health"]
            UC_DETECT_MISSED_TS["Detect Missed Timesheets"]
        end
    end

    ADMIN(("Admin"))
    MANAGER(("Manager"))
    EMPLOYEE(("Employee"))
    SCHEDULER(("Scheduler"))
    LLM(("LLM Provider"))

    %% Auth connections
    ADMIN --> UC_LOGIN
    ADMIN --> UC_CHANGE_PWD
    ADMIN --> UC_LOGOUT
    MANAGER --> UC_LOGIN
    MANAGER --> UC_CHANGE_PWD
    MANAGER --> UC_LOGOUT
    EMPLOYEE --> UC_LOGIN
    EMPLOYEE --> UC_CHANGE_PWD
    EMPLOYEE --> UC_LOGOUT

    %% Admin use cases
    ADMIN --> UC_CREATE_USER
    ADMIN --> UC_VIEW_USERS
    ADMIN --> UC_RESET_PWD
    ADMIN --> UC_DEACTIVATE_USER
    ADMIN --> UC_REACTIVATE_USER
    ADMIN --> UC_VIEW_EMPLOYEES
    ADMIN --> UC_UPDATE_EMPLOYEE
    ADMIN --> UC_DEACTIVATE_EMPLOYEE
    ADMIN --> UC_MANAGE_SKILLS
    ADMIN --> UC_ASSIGN_MANAGER
    ADMIN --> UC_CREATE_PROJECT
    ADMIN --> UC_VIEW_PROJECTS
    ADMIN --> UC_UPDATE_PROJECT
    ADMIN --> UC_MANAGE_MILESTONES
    ADMIN --> UC_VIEW_ALLOCATIONS_ADMIN
    ADMIN --> UC_SYS_CONFIG

    %% Manager use cases
    MANAGER --> UC_RESOURCE_DASHBOARD
    UC_RESOURCE_DASHBOARD --> UC_DRILL_EMPLOYEE
    MANAGER --> UC_AI_SKILL_MATCH
    MANAGER --> UC_DIRECT_ALLOCATE
    MANAGER --> UC_END_ALLOCATION
    MANAGER --> UC_MY_PROJECTS
    UC_MY_PROJECTS --> UC_PROJECT_HEALTH
    MANAGER --> UC_AI_RISK_SUMMARY
    MANAGER --> UC_VIEW_TEAM_TIMESHEETS

    %% Employee use cases
    EMPLOYEE --> UC_SUBMIT_TIMESHEET
    EMPLOYEE --> UC_VIEW_MY_TIMESHEETS
    EMPLOYEE --> UC_VIEW_MY_ALLOCATIONS

    %% Background scheduler
    SCHEDULER --> UC_COMPUTE_UTILISATION
    SCHEDULER --> UC_FLAG_HEALTH
    SCHEDULER --> UC_DETECT_MISSED_TS

    %% LLM connections
    UC_AI_SKILL_MATCH --> LLM
    UC_AI_RISK_SUMMARY --> LLM
```

---

## 4. Sequence Diagrams

### 4.1 Login and Authentication Flow

```mermaid
sequenceDiagram
    autonumber
    actor U as User - Console
    participant CS as Console Screen
    participant API as PRM.API Controller
    participant Auth as AuthService
    participant DB as Database

    U->>CS: Select Login
    CS->>U: Prompt username and password
    U->>CS: Enter credentials

    CS->>API: POST /api/auth/login
    API->>Auth: LoginAsync(username, password)
    Auth->>DB: SELECT user WHERE username
    DB-->>Auth: User record

    alt User not found or inactive
        Auth-->>API: 401 Unauthorized
        API-->>CS: 401 Invalid credentials
        CS-->>U: Error - Invalid username or password
    else Password mismatch
        Auth-->>API: 401 Unauthorized
        API-->>CS: 401 Invalid credentials
        CS-->>U: Error - Invalid username or password
    else Valid credentials
        Auth->>Auth: Generate JWT with userId and role claims
        Auth-->>API: AuthResult with token, role, forcePasswordChange
        API-->>CS: 200 OK with token

        alt forcePasswordChange is true
            CS-->>U: Show Change Password screen
            U->>CS: Enter new password and confirm
            CS->>API: POST /api/auth/change-password
            API->>Auth: ChangePasswordAsync(userId, newPassword)
            Auth->>DB: UPDATE password_hash, force_password_change = false
            DB-->>Auth: Success
            Auth-->>API: 200 OK
            API-->>CS: Password updated
            CS-->>U: Password updated - Welcome!
        end

        CS->>CS: Store JWT in memory
        CS-->>U: Navigate to role-specific menu
    end
```

### 4.2 AI-Assisted Resource Allocation

```mermaid
sequenceDiagram
    autonumber
    actor M as Manager - Console
    participant CS as Console Screen
    participant API as PRM.API Controller
    participant AllocSvc as AllocationService
    participant AISvc as AiService
    participant LLM as LLM Provider
    participant EmpRepo as EmployeeRepository
    participant AllocRepo as AllocationRepository
    participant DB as Database

    M->>CS: Select Allocate Resource then Find resource using AI
    CS->>M: Prompt for project selection
    M->>CS: Enter Project ID
    CS->>M: Prompt requirement description
    M->>CS: I need a backend developer with Java...

    CS->>API: POST /api/allocations/ai-match
    API->>AISvc: FindSkillMatchAsync(requirement, projectId)

    AISvc->>EmpRepo: GetTeamEmployeesAsync(managerId)
    EmpRepo->>DB: SELECT employees WHERE manager_id
    DB-->>EmpRepo: Employee list with skills
    EmpRepo-->>AISvc: Candidate employees

    AISvc->>AllocRepo: GetActiveAllocationsAsync(employeeIds)
    AllocRepo->>DB: SELECT active allocations
    DB-->>AllocRepo: Allocation records
    AllocRepo-->>AISvc: Current allocations per employee

    AISvc->>AISvc: Filter candidates with available capacity
    AISvc->>AISvc: Build prompt with requirement and candidate summaries

    AISvc->>LLM: POST /v1/chat/completions
    LLM-->>AISvc: Ranked matches with reasons

    AISvc->>AISvc: Parse LLM response
    AISvc-->>API: List of MatchResultDto
    API-->>CS: 200 OK with matches

    CS-->>M: Display AI-matched results table
    M->>CS: Select employee number 1
    CS->>M: Prompt allocation details
    M->>CS: Enter 50%, 01-Jun-2026, 30-Sep-2026

    CS->>API: POST /api/allocations
    API->>AllocSvc: AllocateResourceAsync(dto)

    AllocSvc->>AllocRepo: GetOverlapping(employeeId, from, to)
    AllocRepo->>DB: SELECT overlapping allocations
    DB-->>AllocRepo: Existing allocations
    AllocRepo-->>AllocSvc: Overlapping list

    AllocSvc->>AllocSvc: Validate total is 100% or less

    alt Validation fails - over 100%
        AllocSvc-->>API: 400 Exceeds 100% utilisation
        API-->>CS: 400 Error
        CS-->>M: Error - Allocation would exceed 100%
    else Validation passes
        AllocSvc->>AllocRepo: AddAsync(allocation)
        AllocRepo->>DB: INSERT INTO allocations
        DB-->>AllocRepo: Success

        AllocSvc->>EmpRepo: UpdateStatus(employeeId, Allocated)
        EmpRepo->>DB: UPDATE employee SET status Allocated
        DB-->>EmpRepo: Success

        AllocSvc-->>API: AllocationDto
        API-->>CS: 201 Created
        CS-->>M: Allocation saved successfully
    end
```

### 4.3 Timesheet Submission

```mermaid
sequenceDiagram
    autonumber
    actor E as Employee - Console
    participant CS as Console Screen
    participant API as PRM.API Controller
    participant TSSvc as TimesheetService
    participant AllocRepo as AllocationRepository
    participant TSRepo as TimesheetRepository
    participant DB as Database

    E->>CS: Select Submit Timesheet
    CS->>E: Prompt week start date
    E->>CS: Enter 12-05-2026

    CS->>API: GET /api/allocations/my?weekOf=12-05-2026
    API->>AllocRepo: GetActiveForWeek(employeeId, weekStart)
    AllocRepo->>DB: SELECT active allocations for week
    DB-->>AllocRepo: Allocations
    AllocRepo-->>API: List of active allocations
    API-->>CS: 200 OK with allocations list

    CS-->>E: Display Project 1 of 2 - Alpha Portal 50% 20 hrs max
    E->>CS: Hours 18, Tags Microservices and WebSocket
    CS-->>E: Display Project 2 of 2 - Beta CRM 50% 20 hrs max
    E->>CS: Hours 20, Tags Backend API and Bug Fixing

    CS-->>E: Show summary Total 38 of 40 hrs
    E->>CS: Press Submit

    CS->>API: POST /api/timesheets
    API->>TSSvc: SubmitTimesheetAsync(dto)

    TSSvc->>TSRepo: GetByEmployeeAndWeek(employeeId, weekStart)
    TSRepo->>DB: SELECT timesheet WHERE employee and week
    DB-->>TSRepo: NULL no duplicate
    TSRepo-->>TSSvc: No existing timesheet

    TSSvc->>TSSvc: Validate no future week
    TSSvc->>AllocRepo: GetActiveForWeek(employeeId, weekStart)
    AllocRepo->>DB: SELECT allocations
    DB-->>AllocRepo: Active allocations
    AllocRepo-->>TSSvc: Allocation list

    TSSvc->>TSSvc: Validate per-project hours within allocation limit
    TSSvc->>TSSvc: Validate total hours within weekly limit

    alt Validation fails
        TSSvc-->>API: 400 Validation errors
        API-->>CS: 400 Error
        CS-->>E: Validation error message
    else All validations pass
        TSSvc->>TSRepo: AddAsync(timesheet and entries)
        TSRepo->>DB: INSERT timesheet and entries
        DB-->>TSRepo: Success
        TSRepo-->>TSSvc: Saved timesheet
        TSSvc-->>API: TimesheetDto
        API-->>CS: 201 Created
        CS-->>E: Timesheet submitted successfully
    end
```

### 4.4 AI Risk Summary

```mermaid
sequenceDiagram
    autonumber
    actor M as Manager - Console
    participant CS as Console Screen
    participant API as PRM.API Controller
    participant AISvc as AiService
    participant ProjRepo as ProjectRepository
    participant TSRepo as TimesheetRepository
    participant LLM as LLM Provider
    participant DB as Database

    M->>CS: Select AI Assistant then Risk Summary
    CS->>API: GET /api/projects/my
    API->>ProjRepo: GetByManagerIdAsync(managerId)
    ProjRepo->>DB: SELECT projects WHERE manager_id
    DB-->>ProjRepo: Manager projects
    ProjRepo-->>API: Project list
    API-->>CS: 200 OK with projects and health status
    CS-->>M: Display project list with health indicators

    M->>CS: Select Alpha Portal
    CS->>API: GET /api/ai/risk-summary/201
    API->>AISvc: GenerateRiskSummaryAsync(projectId 201)

    AISvc->>ProjRepo: GetByIdWithMilestonesAsync(201)
    ProjRepo->>DB: SELECT project with milestones
    DB-->>ProjRepo: Project with milestones
    ProjRepo-->>AISvc: Project data

    AISvc->>TSRepo: GetRecentByProjectAsync(201, weeks 4)
    TSRepo->>DB: SELECT recent timesheets for project
    DB-->>TSRepo: Timesheet records
    TSRepo-->>AISvc: Recent timesheet data

    AISvc->>AISvc: Build structured prompt with facts
    Note over AISvc: Milestones status and overdue flags,<br/>Allocated resources,<br/>Expected vs actual hours,<br/>Logged activity tags

    AISvc->>LLM: POST /v1/chat/completions
    LLM-->>AISvc: Plain-English risk paragraph

    AISvc-->>API: RiskSummaryDto with summary
    API-->>CS: 200 OK with summary
    CS-->>M: Display AI risk summary with disclaimer
```

---

## 5. Activity / Sequence Flow Diagrams

### 5.1 Allocation Validation Flow

Decision-flow for validating a resource allocation request.

```mermaid
flowchart TD
    A([Manager submits allocation request]) --> B{Project exists?}
    B -- No --> B1[Return: Project not found]
    B -- Yes --> C{Project status is<br/>ACTIVE or PLANNED?}
    C -- No --> C1[Return: Project not in allocatable status]
    C -- Yes --> D{Employee exists<br/>and is active?}
    D -- No --> D1[Return: Employee not found or inactive]
    D -- Yes --> E{Employee is in<br/>Managers team?}
    E -- No --> E1[Return: Not authorized for this employee]
    E -- Yes --> F{FromDate before ToDate?}
    F -- No --> F1[Return: Invalid date range]
    F -- Yes --> G[Fetch overlapping allocations<br/>for employee in date range]
    G --> H{Sum of existing utilisation<br/>plus new percent is 100 or less?}
    H -- No --> H1[Return: Total utilisation<br/>would exceed 100%]
    H -- Yes --> I[Validation passed]
    I --> J[Save allocation to database]
    J --> K{Employee has any<br/>active allocations?}
    K -- Yes --> L[Set employee status = ALLOCATED]
    K -- No --> M[Set employee status = BENCH]
    L --> N([Allocation confirmed])
    M --> N

    style A fill:#4CAF50,color:#fff
    style N fill:#4CAF50,color:#fff
    style B1 fill:#f44336,color:#fff
    style C1 fill:#f44336,color:#fff
    style D1 fill:#f44336,color:#fff
    style E1 fill:#f44336,color:#fff
    style F1 fill:#f44336,color:#fff
    style H1 fill:#f44336,color:#fff
    style I fill:#2196F3,color:#fff
```

### 5.2 Timesheet Submission Flow

```mermaid
flowchart TD
    A([Employee opens Submit Timesheet]) --> B[Enter or default week start date]
    B --> C{Week is in the past<br/>or current week?}
    C -- No future week --> C1[Cannot submit for future weeks]
    C -- Yes --> D[Fetch active allocations<br/>for this week]
    D --> E{Any active allocations<br/>for this week?}
    E -- No --> E1[No active allocations<br/>for selected week]
    E -- Yes --> F{Timesheet already<br/>exists for this week?}
    F -- Yes --> F1[Duplicate: already submitted]
    F -- No --> G[Display each project<br/>with allocation percent and max hours]
    G --> H[Employee enters hours<br/>and selects activity tags<br/>for each project]
    H --> I{Hours per project within<br/>allocation limit?}
    I -- No --> I1[Hours exceed allocation<br/>for project]
    I -- Yes --> J{Total hours across<br/>all projects within weekly limit?}
    J -- No --> J1[Total hours exceed<br/>weekly limit]
    J -- Yes --> K[Show summary to employee]
    K --> L{Employee confirms<br/>submission?}
    L -- No Back --> G
    L -- Yes Submit --> M[Save timesheet and entries<br/>Status = SUBMITTED]
    M --> N([Timesheet submitted successfully])

    style A fill:#4CAF50,color:#fff
    style N fill:#4CAF50,color:#fff
    style C1 fill:#f44336,color:#fff
    style E1 fill:#f44336,color:#fff
    style F1 fill:#f44336,color:#fff
    style I1 fill:#f44336,color:#fff
    style J1 fill:#f44336,color:#fff
```

### 5.3 Background Scheduler Job Flow

```mermaid
flowchart TD
    A([Scheduler triggers<br/>per configured interval]) --> B[Load all active projects]
    B --> C[For each project]

    C --> D[Fetch milestones]
    D --> E{Any milestone<br/>overdue and not DONE?}

    C --> F[Fetch allocations<br/>for project]
    F --> G[Fetch recent timesheets<br/>for allocated employees]
    G --> H{Logged hours<br/>significantly below<br/>expected hours?}

    E -- Yes --> FLAG_RISK
    H -- Yes --> FLAG_RISK

    E -- No --> CHECK_ATTENTION
    H -- No --> CHECK_ATTENTION

    FLAG_RISK[Set health_status = AT_RISK]
    CHECK_ATTENTION{Minor delays or<br/>slight hour gaps?}

    CHECK_ATTENTION -- Yes --> FLAG_ATTENTION[Set health_status = ATTENTION]
    CHECK_ATTENTION -- No --> FLAG_OK[Set health_status = ON_TRACK]

    FLAG_RISK --> SAVE
    FLAG_ATTENTION --> SAVE
    FLAG_OK --> SAVE

    SAVE[Update project health_status in DB]
    SAVE --> I[Move to next project]
    I --> C

    A --> J[Load all active employees<br/>with allocations in past week]
    J --> K[For each employee-week pair]
    K --> L{Timesheet submitted<br/>for last completed week?}
    L -- Yes --> MS[Status stays SUBMITTED]
    L -- No --> N[Create or flag timesheet<br/>with status = MISSED]
    MS --> O[Move to next employee]
    N --> O
    O --> K

    K --> P([Scheduler cycle complete<br/>Sleep until next interval])

    style A fill:#9C27B0,color:#fff
    style P fill:#9C27B0,color:#fff
    style FLAG_RISK fill:#f44336,color:#fff
    style FLAG_ATTENTION fill:#FF9800,color:#fff
    style FLAG_OK fill:#4CAF50,color:#fff
    style N fill:#FF9800,color:#fff
```

### 5.4 Employee Deactivation Flow

```mermaid
flowchart TD
    A([Admin selects<br/>Deactivate Employee]) --> B[Enter Employee ID]
    B --> C{Employee found?}
    C -- No --> C1[Employee not found]
    C -- Yes --> D{Employee already<br/>inactive?}
    D -- Yes --> D1[Employee is already deactivated]
    D -- No --> E[Display employee details]
    E --> F[Fetch active allocations]
    F --> G{Has active<br/>allocations?}
    G -- Yes --> H[Show warning:<br/>List affected projects<br/>and allocation percentages]
    G -- No --> I[Show confirmation prompt]
    H --> I

    I --> J{Admin confirms<br/>deactivation?}
    J -- No Cancel --> K([Cancelled - Return to menu])
    J -- Yes --> L[Set employee is_active = false]
    L --> MM[End all active allocations<br/>set to_date = today]
    MM --> N[Recompute affected employees<br/>status if needed]
    N --> O[Set linked user account<br/>is_active = false]
    O --> P[Block user login access]
    P --> Q([Employee deactivated<br/>Historical data preserved])

    style A fill:#4CAF50,color:#fff
    style Q fill:#4CAF50,color:#fff
    style K fill:#607D8B,color:#fff
    style C1 fill:#f44336,color:#fff
    style D1 fill:#2196F3,color:#fff
    style H fill:#FF9800,color:#fff
```

---

> [!NOTE]
> These diagrams use Mermaid syntax and render natively in most Markdown viewers (GitHub, VS Code with Mermaid extension, etc.). Install a Mermaid preview extension if they appear as raw code.
