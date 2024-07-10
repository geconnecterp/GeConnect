namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos.Cajas;

    public class CajaValidator : AbstractValidator<CajaDto>
    {
        public CajaValidator()
        {
            RuleFor(p => p.Caja_Id).NotNull().Length(4, 4);
            RuleFor(p => p.Adm_Id).NotNull().Length(4, 4);
            RuleFor(p => p.Depo_Id).NotNull().Length(2, 2);
            RuleFor(p => p.Dia_Movi).Length(4, 15);
            RuleFor(p => p.Usu_Id).Length(4, 10);
            RuleFor(p => p.Caja_Nombre).NotNull().Length(4, 30);
            RuleFor(p => p.Caja_Estado).NotNull();
            RuleFor(p => p.Caja_Habilitadas).NotNull();
            RuleFor(p => p.Caja_Modalidad).NotNull().Length(2, 2);
            RuleFor(p => p.Caja_Maquina).Length(4, 30);
            RuleFor(p => p.Caja_Nro_Proceso).Length(4, 15);
            RuleFor(p => p.Caja_Nro_Operacion).NotNull();
            RuleFor(p => p.Caja_Activa).NotNull();
            RuleFor(p => p.Caja_Manual).NotNull();
            RuleFor(p => p.Caja_Actu).NotNull();

        }
    }
}
