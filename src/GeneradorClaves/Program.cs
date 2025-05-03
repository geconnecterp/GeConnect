using GeneradorClaves.Extensions;
using GeneradorClaves.Modelo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

void InicializaPantalla()
{
    Console.OutputEncoding = Encoding.Default;
    Console.WindowHeight = 40;
    Console.BackgroundColor = ConsoleColor.Blue;
    Console.ForegroundColor = ConsoleColor.White;

    Console.Title = "Generador de Clave Pública y Privada.";
    Console.Clear();
    Console.SetCursorPosition(0, 0);
}

try
{
    InicializaPantalla();

    IConfiguration configuration = new ConfigurationBuilder()

        .SetBasePath(AppContext.BaseDirectory)

        .AddJsonFile("appSettings.json", false, false).Build();

    var serviceColeccion = new ServiceCollection();

    serviceColeccion.AddServicios(configuration);

    serviceColeccion.Configure<AppSettings>(configuration.GetSection("AppSettings"));

    var serviceProvider = serviceColeccion.BuildServiceProvider();

    Console.Clear();

    var proc = serviceProvider.GetService<IGeneradorClave>();
    if (proc != null)
    {
        Thread thProcCud = new Thread(new ThreadStart(proc.TestDeRSA));
        thProcCud.Start();
        thProcCud.Join();
    }
    else
    {
        Console.WriteLine("No se pudo obtener el servicio de generador de claves.");
        return;
    }


    Console.WriteLine("Claves Generadas");
}
catch (Exception)
{

    throw;
}

