namespace PRM.Console.Models.Users;

public record AddSkillRequest(
    string SkillName,
    string Category,
    string ProficiencyLevel
);
