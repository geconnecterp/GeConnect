namespace gc.sitio.Models.Components;

/// <summary>
/// Paginador opt-in: presentación solamente, sin consultas ni estado de sesión.
/// Uso: incluir Views/Shared/Components/_PaginadorNumerado.cshtml con este modelo;
/// cargar css/components/gc-pager.css y js/components/gc-pager.js una vez en la vista.
/// El contenedor del módulo atiende gc:page (detail.page) y realiza su propia consulta.
/// Durante la carga debe establecer aria-busy="true" y deshabilitar botones/inputs.
/// Reemplazar la parcial tras éxito y restaurar foco en [aria-current="page"].
/// No reemplaza GridCoreSmart, no decide tamaños de página ni interviene en exportaciones.
/// </summary>
public sealed class PaginadorNumerado
{
    public int Pagina { get; }
    public int Paginas { get; }
    public string Etiqueta { get; }

    public PaginadorNumerado(int pagina, int paginas, string etiqueta = "Páginas de resultados")
    {
        Paginas = Math.Max(1, paginas);
        Pagina = Math.Clamp(pagina, 1, Paginas);
        Etiqueta = etiqueta;
    }

    // Hasta siete números, incluso con millones de páginas. null representa un salto.
    public IReadOnlyList<int?> Numeros()
    {
        var desde = Math.Max(1L, Math.Min((long)Pagina - 2, (long)Paginas - 4));
        var hasta = Math.Min(Paginas, desde + 4);
        var numeros = new SortedSet<int> { 1, Paginas };
        for (var n = desde; n <= hasta; n++) numeros.Add((int)n);
        var resultado = new List<int?>();
        var anterior = 0;
        foreach (var n in numeros)
        {
            if (anterior > 0 && n - anterior > 1) resultado.Add(null);
            resultado.Add(n);
            anterior = n;
        }
        return resultado;
    }
}
