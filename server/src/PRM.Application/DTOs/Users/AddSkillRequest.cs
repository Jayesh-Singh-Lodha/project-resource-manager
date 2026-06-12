namespace PRM.Application.DTOs.Users;

public record AddSkillRequest(
    string SkillName,
    string Category,
    string ProficiencyLevel
);
