namespace Poderosa.Library
{
	public class Strings
    {
		public static string GetString(string id)
		{
			return id;
		}
	}

	public class StringResource
	{
		public static StringResource Instance { get; } = new StringResource();

		public string GetString(string id)
        {
			return Strings.GetString(id);
        }
	}
}