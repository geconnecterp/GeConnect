namespace gc.infraestructura.Enumeraciones
{
    public enum AppModulos
    {
        CCUENTAS, //CONSULTA DE CUENTAS COMERCIALES
        ASTEMP, //ASIENTOS TEMPORALES
        ASDEF, //ASIENTOS DEFINITIVOS
        LMAYOR, //LIBRO MAYOR,
        BSS, //BALANCE SUMA SALDOS
        LDIARIO, //LIBRO DIARIO
        BGR, //BALANCE GENERAL
    }

    public enum AppReportes
    {
        CCUENTAS_CUENTA_CORRIENTE = 1,
        CCUENTAS_VENCIMIENTO_COMPROBANTES = 2,
        CCUENTAS_COMPROBANTES = 3,
        CCUENTAS_ORDEN_DE_PAGO = 4,
        CCUENTAS_RECEPCION_PROVEEDORES = 5,
        CCUENTAS_COMPROBANTES_DETALLE = 6,
        CCUENTAS_ORDEN_DE_PAGO_DETALLE = 7,
        CCUENTAS_RECEPCION_PROVEEDORES_DETALLE = 8,
    }

    public enum DocumentType
    {
        Original,
        Duplicate,
        Certificate,
        Receipt,
        Report,
        Statement
    }

    public enum ExportType
    {
        PDF,
        Excel,
        TXT
    }

    public enum MessagePriority
    {
        Low,
        Normal,
        High
    }
}
