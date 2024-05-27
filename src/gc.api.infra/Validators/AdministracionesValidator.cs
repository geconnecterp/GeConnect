namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos.Administracion;

    public class AdministracionesValidator : AbstractValidator<AdministracionDto>
    {
        public AdministracionesValidator()
        {
            RuleFor(p => p.Adm_id).NotNull().Length(4, 4);
            RuleFor(p => p.Adm_nombre).NotNull().Length(4, 40);
            RuleFor(p => p.Adm_direccion).NotNull().Length(4, 150);
            RuleFor(p => p.Usu_id_encargado).NotNull().Length(4, 10);
            RuleFor(p => p.Adm_nro_lote).NotNull();
            RuleFor(p => p.Adm_nro_lote_central).NotNull();
            RuleFor(p => p.Adm_central).NotNull();
            RuleFor(p => p.Cx_existe).NotNull();
            RuleFor(p => p.Cx_profile).Length(4, 100);
            RuleFor(p => p.Cx_base).Length(4, 60);
            RuleFor(p => p.Cx_login).Length(4, 60);
            RuleFor(p => p.Cx_pass).Length(4, 60);
            RuleFor(p => p.Adm_activa).NotNull();
            RuleFor(p => p.Adm_oc_limite).NotNull();

        }
    }
}
