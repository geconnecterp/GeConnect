using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.EntidadesComunes.Options
{
    public class ApiSettingOptions
    {
        public const string SectionName = "ApiSetting";

        [Range(1,600)]
        public int TimeoutInSeconds { get; set; } = 210;

        public bool PermitirCertificadosNoValidos { get; set; } = false;
    }
}
