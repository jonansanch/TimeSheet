using System.Runtime.CompilerServices;
using QuestPDF.Infrastructure;

namespace KPG.Timesheet.Infrastructure.IntegrationTests;

/// <summary>
/// Fija la licencia de QuestPDF una sola vez al cargar el ensamblado de tests.
///
/// <para>
/// En la app real esto lo hace <c>Program.cs</c>, pero los tests no lo ejecutan. Sin esto,
/// los tests que generan PDF (<see cref="Reportes.TimesheetDocumentBuilderTests"/>) solo
/// pasaban si otro test que si tocaba el host de la Api corria antes en el mismo proceso —
/// dependian del orden de ejecucion.
/// </para>
/// </summary>
internal static class QuestPdfTestSetup
{
    [ModuleInitializer]
    public static void FijarLicencia() => QuestPDF.Settings.License = LicenseType.Community;
}
