
namespace UniversalHybridTemplate
{
	public partial class App : Application
	{
		protected override Window CreateWindow(IActivationState activationState) {
			return new Window(new MainPage());
		}

		public App() {
			InitializeComponent();

			if (Windows.Count == 0) {
				var unused = CreateWindow(null);
				return;
			}
			Windows[0].Page = new MainPage();
		}
	}
}
