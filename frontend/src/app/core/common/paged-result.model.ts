// Genérico e reutilizável — qualquer listagem paginada (busca de jogadores, histórico
// de mensagens, feed no futuro) usa o mesmo shape. Espelha PagedResultDto<T> do backend.
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
