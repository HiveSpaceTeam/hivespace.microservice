using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.UserService.Api.Helpers;

public static class ValidationRuleGenerator
{
    public static ValidationResult GenerateClientRules<TModel>(Expression<Func<TModel, object>> propertyExpression)
    {
        var memberExpression = GetMemberExpression(propertyExpression);
        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new DomainException(400, DomainErrorCode.InvalidExpression, "Expression must reference a property");
            
        var attributes = property.GetCustomAttributes<ValidationAttribute>();
        
        var rules = new Dictionary<string, object>();
        var messages = new Dictionary<string, string>();
        
        foreach (var attr in attributes)
        {
            switch (attr)
            {
                case RequiredAttribute req:
                    rules["required"] = true;
                    messages["required"] = req.ErrorMessage ?? "This field is required";
                    break;
                    
                case StringLengthAttribute len:
                    if (len.MinimumLength > 0)
                    {
                        rules["minLength"] = len.MinimumLength;
                        messages["minLength"] = $"Minimum {len.MinimumLength} characters required";
                    }
                    if (len.MaximumLength > 0)
                    {
                        rules["maxLength"] = len.MaximumLength;
                        messages["maxLength"] = $"Maximum {len.MaximumLength} characters allowed";
                    }
                    break;
                    
                case EmailAddressAttribute email:
                    rules["email"] = true;
                    messages["email"] = email.ErrorMessage ?? "Please enter a valid email address";
                    break;
                    
                case RegularExpressionAttribute regex:
                    // Detect password strength pattern specifically
                    if (regex.Pattern.Contains("(?=.*[a-z])") && regex.Pattern.Contains("(?=.*[A-Z])") && regex.Pattern.Contains("(?=.*\\d)"))
                    {
                        rules["passwordStrength"] = true;
                        messages["passwordStrength"] = regex.ErrorMessage ?? "Password must contain uppercase, lowercase, number, and special character";
                    }
                    else
                    {
                        rules["pattern"] = regex.Pattern;
                        messages["pattern"] = regex.ErrorMessage ?? "Invalid format";
                    }
                    break;
                    
                case CompareAttribute compare:
                    rules["compare"] = compare.OtherProperty;
                    messages["compare"] = compare.ErrorMessage ?? "Fields do not match";
                    break;
            }
        }
        
        return new ValidationResult(rules, messages);
    }
    
    private static MemberExpression GetMemberExpression<TModel>(Expression<Func<TModel, object>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member;
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            return unaryMember;
        throw new DomainException(400, DomainErrorCode.InvalidExpression, "Expression must be a member expression");
    }
    
    public class ValidationResult
    {
        public Dictionary<string, object> Rules { get; }
        public Dictionary<string, string> Messages { get; }
        
        public ValidationResult(Dictionary<string, object> rules, Dictionary<string, string> messages)
        {
            Rules = rules;
            Messages = messages;
        }
    }
}