using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc
{
    public static class Constants
    {
        public static string Message
        {
            get { return "Message"; }
        }

        public static string ErrorOccurred
        {
            get { return "ErrorOccurred"; }
        }

        public static PdfReport CreateBasicReport(string naslov)
        {
            var pdf = new PdfReport();
            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "RPPP09",
                    Application = "KoronavirusMvc",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
            .MainTablePreferences(table => {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                //table.NumberOfDataRowsPerPage(20);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = true,
                    ShowOneGroupPerPage = true
                });
                table.SpacingAfter(4f);
            });
            return pdf;
        }
    }
}
