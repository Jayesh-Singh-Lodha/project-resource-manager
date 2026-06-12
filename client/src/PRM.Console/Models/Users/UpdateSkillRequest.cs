namespace PRM.Console.Models.Users;

public record UpdateSkillRequest(
    string SkillName,
    string ProficiencyLevel
);
