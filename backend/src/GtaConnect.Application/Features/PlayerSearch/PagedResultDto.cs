namespace GtaConnect.Application.Features.PlayerSearch;

// Genérico e reutilizável — qualquer listagem paginada futura (feed etc) pode usar o mesmo shape.
public record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
