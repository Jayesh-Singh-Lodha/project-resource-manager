namespace PRM.Application.DTOs.Users;

public record UpdateSkillRequest(
    string SkillName,
    string ProficiencyLevel
);
