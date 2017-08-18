namespace UniECS
{
	public class SequentialIdentityGenerator : IIdentityGenerator
	{
		static private SequentialIdentityGenerator instance;

		static public SequentialIdentityGenerator Instance {
			get {
				if (instance == null) {
					instance = new SequentialIdentityGenerator ();
				}
				return instance;
			}
		}

		private int lastIdentifier = 0;

		public int GenerateId ()
		{
			return ++lastIdentifier;
		}
	}
}