// See https://aka.ms/new-console-template for more information


using System.Security.Cryptography;
using System.Text;

string template = "id:@id;request-id:@req;ts:@ts;";

string id = "0002-123490";
string req = "beae84ef-11ad-4a02-9f13-2d1dce26d9e0";
string ts = "1713879440";
string v1 = "2a44eed56cc370af2d8a021c083602b3dd60b769387a98e3998c7e455b2ae391";
string key = "257ae2b219bdf1533a8df0f02c936a7561fdde5a7852f201f7a89a2d8d18c180";

template = template.Replace("@id", id).Replace("@req", req).Replace("@ts", ts);
 
string hmac1 = ObtenerHMACtoHex(template, key).ToLower();
string hmac2 = ObtenerHMACtoHex(template, key,false).ToLower();
//string hmac3 = ObtenerHMACtoHexV2(template, key);
//string hmac4 = ObtenerHMACtoHexV2(template, key,false);

Console.WriteLine(template);
Console.WriteLine($"                       {v1}");
Console.WriteLine($"HMACSHA256   - UTF8:   {hmac1} => Comparación: {hmac1.Equals(v1)}");
Console.WriteLine($"HMACSHA256   - ASCII:  {hmac2} => Comparación: {hmac2.Equals(v1)}");
//Console.WriteLine($"HMACSHA3_256 - UTF8<: {hmac3} => Comparación: {hmac3.Equals(v1)}");
//Console.WriteLine($"HMACSHA3_256 - ASCII: {hmac4} => Comparación: {hmac4.Equals(v1)}");


Console.ReadKey();  

string ObtenerHMACtoHex(string text, string key,bool useUTF8=true)
{
    key ??= ""; //si es null se le asigna ""
    if (useUTF8)
    {
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash);
        }
    }
    else
    {
        using (var hmacsha256 = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
        {
            var hash = hmacsha256.ComputeHash(Encoding.ASCII.GetBytes(text));
            return Convert.ToHexString(hash);
        }
    }
}

//string ObtenerHMACtoHexV2(string text, string key, bool useUTF8 = true)
//{
//    key ??= ""; //si es null se le asigna ""
//    if (useUTF8)
//    {
//        using (var hmacsha256 = new HMACSHA3_256(Encoding.UTF8.GetBytes(key)))
//        {
//            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
//            return Convert.ToHexString(hash);
//        }
//    }
//    else
//    {
//        using (var hmacsha256 = new HMACSHA3_256(Encoding.ASCII.GetBytes(key)))
//        {
//            var hash = hmacsha256.ComputeHash(Encoding.ASCII.GetBytes(text));
//            return Convert.ToHexString(hash);
//        }
//    }
//}


//public string ObtenerHMACtoHex(string text, string key)
//{
//    key ??= ""; //si es null se le asigna ""

//    using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
//    {
//        var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
//        return Convert.ToHexString(hash);
//    }

//}
