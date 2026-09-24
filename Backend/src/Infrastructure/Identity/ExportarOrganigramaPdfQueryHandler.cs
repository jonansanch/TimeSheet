using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.ExportarOrganigramaPdf;
using KPG.Timesheet.Application.Features.Users.Queries.GetOrganigrama;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KPG.Timesheet.Infrastructure.Identity;

public class ExportarOrganigramaPdfQueryHandler(IIdentityService identityService)
    : IRequestHandler<ExportarOrganigramaPdfQuery, ExportarOrganigramaPdfResult>
{
    public async Task<ExportarOrganigramaPdfResult> Handle(
        ExportarOrganigramaPdfQuery request,
        CancellationToken cancellationToken)
    {
        var nodos = await identityService.GetOrganigramaAsync(cancellationToken);
        var filas = ConstruirFilas(nodos);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.2f, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor("#16324F"));

                page.Header().Column(column =>
                {
                    column.Item().Text("KPG Timesheet").FontSize(10).SemiBold().FontColor("#0B577D");
                    column.Item().Text("Organigrama").FontSize(20).Bold().FontColor("#082F49");
                    column.Item().PaddingTop(2).Text($"{nodos.Count} colaboradores activos · Generado {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor("#52677A");
                });

                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3.2f);
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(2.2f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Colaborador");
                        HeaderCell(header.Cell(), "Puesto");
                        HeaderCell(header.Cell(), "Rol");
                        HeaderCell(header.Cell(), "Reporta a");
                    });

                    foreach (var fila in filas)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor("#D8E2EA").PaddingVertical(6)
                            .PaddingLeft(fila.Nivel * 14).Text($"{(fila.Nivel > 0 ? "↳ " : string.Empty)}{fila.Nodo.Nombre}")
                            .SemiBold();
                        BodyCell(table.Cell(), fila.Nodo.PuestoNombre ?? "Sin puesto");
                        BodyCell(table.Cell(), fila.Nodo.Rol);
                        BodyCell(table.Cell(), fila.SupervisorNombre ?? "—");
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        });

        return new ExportarOrganigramaPdfResult(
            documento.GeneratePdf(),
            "application/pdf",
            $"organigrama-kpg-{DateTime.Now:yyyyMMdd}.pdf");
    }

    private static IReadOnlyList<FilaOrganigrama> ConstruirFilas(
        IReadOnlyList<OrganigramaNodoDto> nodos)
    {
        var porId = nodos.ToDictionary(n => n.UserId, StringComparer.Ordinal);
        var hijos = nodos
            .Where(n => n.SupervisorUserId is not null
                     && n.SupervisorUserId != n.UserId
                     && porId.ContainsKey(n.SupervisorUserId))
            .GroupBy(n => n.SupervisorUserId!)
            .ToDictionary(g => g.Key, g => g.OrderBy(n => n.Nombre).ToList(), StringComparer.Ordinal);

        var raices = nodos
            .Where(n => n.SupervisorUserId is null
                     || n.SupervisorUserId == n.UserId
                     || !porId.ContainsKey(n.SupervisorUserId))
            .OrderBy(n => n.Nombre);

        var resultado = new List<FilaOrganigrama>();
        foreach (var raiz in raices)
            Agregar(raiz, 0, null, hijos, resultado, []);

        return resultado;
    }

    private static void Agregar(
        OrganigramaNodoDto nodo,
        int nivel,
        string? supervisorNombre,
        IReadOnlyDictionary<string, List<OrganigramaNodoDto>> hijos,
        List<FilaOrganigrama> resultado,
        HashSet<string> visitados)
    {
        if (!visitados.Add(nodo.UserId)) return;
        resultado.Add(new FilaOrganigrama(nodo, nivel, supervisorNombre));

        if (!hijos.TryGetValue(nodo.UserId, out var subordinados)) return;
        foreach (var subordinado in subordinados)
            Agregar(subordinado, nivel + 1, nodo.Nombre, hijos, resultado,
                new HashSet<string>(visitados, StringComparer.Ordinal));
    }

    private static void HeaderCell(IContainer cell, string texto) =>
        cell.Background("#0B577D").Padding(6).Text(texto).SemiBold().FontColor(Colors.White);

    private static void BodyCell(IContainer cell, string texto) =>
        cell.BorderBottom(0.5f).BorderColor("#D8E2EA").PaddingVertical(6).Text(texto);

    private sealed record FilaOrganigrama(
        OrganigramaNodoDto Nodo,
        int Nivel,
        string? SupervisorNombre);
}
