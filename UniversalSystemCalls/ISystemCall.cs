namespace UniversalSystemCalls;

public interface ISystemCall
{
	string PlatformName { get; }

}


public interface ISystemCall_GetUserAccentColor : ISystemCall
{
	Task<SystemColor> GetUserAccentColor();
}

public interface ISystemCall_Notification : ISystemCall
{
	Task Notifcation(SystemNotification systemNotification);
}

