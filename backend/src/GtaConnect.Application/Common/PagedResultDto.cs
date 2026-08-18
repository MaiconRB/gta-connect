namespace GtaConnect.Application.Common;

// Genérico e reutilizável — qualquer listagem paginada (busca de jogadores, histórico de
// mensagens, feed no futuro) usa o mesmo shape. Por isso vive em Common, não numa feature.
public record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
