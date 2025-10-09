using Newtonsoft.Json;

namespace gc.sitio.Helpers
{
	public static class Clonador
	{
		public static List<T> ClonarLista<T>(this List<T> source)
		{
			var json = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<List<T>>(json);
		}

		public static T ClonarObjeto<T>(this T source)
		{
			var json = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
