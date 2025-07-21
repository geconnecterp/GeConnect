namespace gc.infraestructura.Helpers
{
    public static class HelperContable
    {
        public static decimal CalcularPCosto(
            decimal p_plista,
            decimal p_dto1,
            decimal p_dto2,
            decimal p_dto3,
            decimal p_dto4,
            decimal p_dto_pa,
            string p_boni,
            decimal p_porc_flete
            )
        {
            decimal costo, boni;
            costo = 0;
            boni = 1;

            if (!string.IsNullOrEmpty(p_boni))
            {
                var boniArr = p_boni.Split("/").Select(x => x.ToDecimal()).ToArray();
                if (boniArr.Length == 2)
                {
                    if (boniArr[0] < boniArr[1] && boniArr[0] > 0 && boniArr[1] > 0)
                    {
                        boni = boniArr[0] / boniArr[1];
                    }
                }
            }

            costo = p_plista * ((100 - p_dto1) / 100) *
                ((100 - p_dto2) / 100) * ((100 - p_dto3) / 100) *
                ((100 - p_dto4) / 100) * ((100 - p_dto_pa) / 100) *
                boni * ((100 + p_porc_flete) / 100);

            return costo;
        }
    }
}
