namespace FindPet.Domain.Exceptions;

public class BusinessRuleException : BaseException
{
    public BusinessRuleException(string message)
        : base(message, 400, "BUSINESS_RULE_VIOLATION")
    {
    }

    public BusinessRuleException(string rule, string message)
        : base(message, 400, "BUSINESS_RULE_VIOLATION", new { Rule = rule })
    {
    }
}