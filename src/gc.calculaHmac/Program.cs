// See https://aka.ms/new-console-template for more information


using System.Security.Cryptography;
using System.Text;

string template = "id:@id;request-id:@req;ts:@ts;";

string id = "76436050231";
string req = "d7b5e44e-2048-4edb-b74d-c728e739bcee";
string ts = "1713533795";
string v1 = "72641038abd29e98a2872120281ec4f94312c4badff3641b11f76fa28a55a6e4";
string key = "25e4f367871406970020007038c93fe0873699df6bc664c7674b664be3706b1a";

template = template.Replace("@id", id).Replace("@req", req).Replace("@ts", ts);
 
string hmac1 = ObtenerHMACtoHex(template, key);
string hmac2 = ObtenerHMACtoHex(template, key,false);
//string hmac3 = ObtenerHMACtoHexV2(template, key);
//string hmac4 = ObtenerHMACtoHexV2(template, key,false);

Console.WriteLine(template);
Console.WriteLine(v1);
Console.WriteLine($"HMACSHA256   - UTF8:  {hmac1} => Comparación: {hmac1.Equals(v1)}");
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
