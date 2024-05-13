namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos.Billeteras;

    public class BilleteraOrdenValidator : AbstractValidator<BilleteraOrdenDto>
    {
        public BilleteraOrdenValidator()
        {
            RuleFor(p => p.Bo_Id).NotNull().Length(0, 20);
            //RuleFor(p => p.Rb_Compte).NotNull().Length(4, 20);
            //RuleFor(p => p.Adm_Id).NotNull().Length(4, 4);
            //RuleFor(p => p.Caja_Id).NotNull().Length(4, 4);
            //RuleFor(p => p.Bill_Id).Length(2, 2);
            //RuleFor(p => p.Boe_Id).NotNull().Length(2, 2);
            //RuleFor(p => p.Cuit).NotNull().Length(4, 15);
            //RuleFor(p => p.Tco_Id).Length(3, 3);
            //RuleFor(p => p.Cm_Compte).Length(4, 15);
            //RuleFor(p => p.Bo_Importe).NotNull();
            //RuleFor(p => p.Bo_Carga).NotNull();
            //RuleFor(p => p.Bo_Clave).Length(4, 500);
            //RuleFor(p => p.Bo_Id_Ext).Length(4, 20);
            //RuleFor(p => p.Bo_Notificado_Desc).Length(4, 100);
            //RuleFor(p => p.Ip).Length(4, 15);

        }
    }
}
