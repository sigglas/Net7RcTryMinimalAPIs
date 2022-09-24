namespace TryMinimalAPIs.Models
{
    public class ApiDescModel
    {
        public static string GetContent()
        {
            var tFile = $"TryMinimalAPIs.Content.ApiDescriptionV1.html";
            System.Reflection.Assembly _assembly;
            System.IO.StreamReader _textStreamReader;
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();
            _textStreamReader = new System.IO.StreamReader(_assembly.GetManifestResourceStream(tFile));
            var resultTxt = _textStreamReader.ReadToEnd();

            return resultTxt;
        }
    }
}
