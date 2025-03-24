namespace UniversalSystemCalls;

public struct SystemNotification
{
	public SystemNotification() {
	}

	public struct Button
	{
		public string Title { get; set; }
		public Action ClickAction { get; set; }

		public Button() { }

		public Button(string title, Action clickAction) {
			Title = title;
			ClickAction = clickAction;
		}
	}

	public string Title { get; set; }

	public string Body { get; set; }

	public string BodyImagePath { get; set; }

	public string BodyImageAltText { get; set; } = "Image";

	public Action NotificationRemmoved { get; set; }

	public List<Button> Buttons { get; } = [];
}
