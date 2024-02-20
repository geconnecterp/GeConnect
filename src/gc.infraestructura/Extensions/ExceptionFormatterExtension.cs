namespace gc.infraestructura.Core.Extensions
{
    using System;
    using System.Text;


    public class ExceptionFormatterExtension
    {
        private readonly Exception ex;

        public ExceptionFormatterExtension(Exception ex)
        {
            this.ex = ex;
        }

        public string GetValue()
        {
            var builder = new StringBuilder();

            builder.Append("EXCEPTION TYPE: ");
            builder.AppendLine($" <{ex.GetType().Name}> ");


            builder.AppendLine("EXCEPTION MESSAGE ");
            builder.AppendLine(ex.Message);
            //builder.AppendLine();

            builder.AppendLine("EXCEPTION SOURCE ");
            builder.AppendLine(ex.Source);
            builder.AppendLine();

            builder.AppendLine("STACK TRACE ");
            builder.AppendLine(ex.StackTrace);
            builder.AppendLine();

            if (ex.InnerException != null)
            {
                builder.AppendLine("INNER-EXCEPTION: ");
                builder.AppendLine(new ExceptionFormatterExtension(ex.InnerException).GetValue());
            }           

            return builder.ToString();
        }
    }
}
