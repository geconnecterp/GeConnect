namespace gc.infraestructura.Dtos.Cajas;

// Fachadas de compatibilidad: Clover conserva su contrato HTTP y su opción QR.
public sealed class CloverOpcionDto : PagoIntegradoOpcionDto { }
public sealed class CloverPreparacionDto : PagoIntegradoPreparacionDto
{
    public bool qr { get; set; }
}
public sealed class CloverRespuestaSpDto : PagoIntegradoRespuestaSpDto { }
