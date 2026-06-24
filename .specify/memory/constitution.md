<!--
SYNC IMPACT REPORT
- Version change: [TEMPLATE] -> 1.0.0
- List of modified principles:
  - [PRINCIPLE_1_NAME] -> I. Segregation of Concerns (Clean Architecture)
  - [PRINCIPLE_2_NAME] -> II. Spec-Driven Development (SDD)
  - [PRINCIPLE_3_NAME] -> III. API Design First
  - [PRINCIPLE_4_NAME] -> IV. Domain-Driven Design (DDD)
  - [PRINCIPLE_5_NAME] -> V. Automated Testing
- Added sections:
  - Additional Constraints
  - Development Workflow
- Removed sections: None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
- Follow-up TODOs: None
-->

# ExecutiveBriefing Constitution

## Core Principles

### I. Segregation of Concerns (Clean Architecture)
The project follows clean architecture principles. The solution is split into API, ApplicationServices, Infrastructure, and Domain layers. Dependencies flow inwards: API and Infrastructure depend on ApplicationServices and Domain; Domain has no outer dependencies.

### II. Spec-Driven Development (SDD)
All development must proceed through specification (`spec.md`), technical planning (`plan.md`), and structured task breakdown (`tasks.md`) before any implementation code is written.

### III. API Design First
API contracts must be defined and documented using OpenAPI standards. Routes, requests, and response schemas must be designed before implementing controllers.

### IV. Domain-Driven Design (DDD)
The domain layer must enforce strict DDD patterns:
- **Entities**: Must have a unique identity, mutable state managed strictly through behavior-rich methods (no public property setters), and internal validation invariants.
- **Value Objects**: Must be immutable, have no identity, and define equality based on their properties.
- **Aggregate Roots**: Must serve as the entry point and consistency boundary for any state modifications. Access to internal entities must be marshaled through the aggregate root.
- **Anemic Domain Models**: Strictly forbidden. Entities containing only getters/setters without behavior-rich methods are not allowed.
- **Repositories**: Must define interfaces in the Domain layer and be implemented in the Infrastructure layer.

### V. Test-Driven Development (TDD)
The project strictly enforces TDD practices:
- **Red-Green-Refactor Cycle**: All code changes must start by writing unit tests that fail (Red). Then, the minimal code to pass the tests is written (Green). Finally, the code is refactored for cleanliness and performance (Refactor).
- **Test Completeness**: Unit tests must cover all business logic, validation rules, and domain entity constraints. No business logic can be written without a corresponding pre-existing test.
- **Integration Tests**: Must verify API controllers, endpoint routing, database adapters, and external integrations after unit tests pass.

### VI. Simplicity and Evolutionary Architecture (YAGNI)
The architecture must remain simple, lightweight, and evolutive:
- **YAGNI (You Aren't Gonna Need It)**: Do not over-engineer. Solve current requirements with the simplest possible design.
- **No Advanced Design Patterns by Default**: Do not apply complex/advanced patterns (e.g., CQRS, Event Sourcing, MediatR, Outbox pattern) without explicit justification.
- **Collaborative Gate**: Any introduction of advanced patterns or architectural complexity must first be analyzed and discussed with the user to verify if the trade-off makes sense.

### VII. Clean Code & Clean Architecture Pillars
To ensure maintainability, the codebase must adhere to clean design standards:
- **Single Responsibility Principle (SRP)**: Every module, class, and method must have one, and only one, reason to change.
- **Scout Rule**: Always leave the code cleaner than you found it. When modifying a file, clean up surrounding dead code, poor naming, or formatting issues.
- **Explicit Architecture Boundaries**: Layer boundaries are non-negotiable. API and Infrastructure can only talk to Application Services. The Domain layer must be pure C# and have zero dependencies on other layers or external libraries.
- **Refactoring Discipline**: Refactoring is not a separate phase. Refactor for readability, DRY (Don't Repeat Yourself), and architecture alignment immediately after tests pass.



## Additional Constraints
- The backend must target .NET 10.
- Follow C# coding standards and naming conventions (PascalCase for classes/methods, camelCase for local variables).
- Minimize third-party dependencies.

## Development Workflow
1. Update/create Spec file (`spec.md`).
2. Update/create Plan file (`plan.md`).
3. Update/create Tasks checklist (`tasks.md`).
4. Write failing tests for the task chunk (Red).
5. Implement minimal code to make the tests pass (Green).
6. Refactor the code for cleanliness and compliance with DDD (Refactor).
7. Verify code compilation and run all tests.

## Governance
The Constitution is the ultimate source of truth for architectural style and development standards. Any deviations must be explicitly documented and approved in the implementation plan.

**Version**: 1.0.0 | **Ratified**: 2026-06-24 | **Last Amended**: 2026-06-24
