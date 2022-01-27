namespace Operador
{
    public class Base64
    {
        public static string Encriptar(string txt)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(txt);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string DesEncriptar(string txt)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(txt);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
