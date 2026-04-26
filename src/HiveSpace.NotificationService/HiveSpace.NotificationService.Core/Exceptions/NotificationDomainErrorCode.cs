using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.NotificationService.Core.Exceptions;

public class NotificationDomainErrorCode : DomainErrorCode
{
    private NotificationDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    public static readonly NotificationDomainErrorCode NotificationNotFound = new(4001, "NotificationNotFound", "NTF4001");
    public static readonly NotificationDomainErrorCode NotNotificationOwner = new(4002, "NotNotificationOwner", "NTF4002");
    public static readonly NotificationDomainErrorCode InvalidConfiguration  = new(4003, "InvalidConfiguration",  "NTF4003");
    public static readonly NotificationDomainErrorCode InvalidEventGroup     = new(4004, "InvalidEventGroup",     "NTF4004");
}
