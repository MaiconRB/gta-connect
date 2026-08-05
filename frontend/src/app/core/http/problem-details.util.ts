import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetailsBody {
  title?: string;
  errors?: Record<string, string[]>;
}

// A Api sempre responde erros no formato ProblemDetails (ver GlobalExceptionHandler no backend).
// Esta função extrai uma mensagem exibível ao usuário a partir desse formato.
export function extractErrorMessage(error: HttpErrorResponse): string {
  const body = error.error as ProblemDetailsBody | undefined;

  const firstFieldError = body?.errors && Object.values(body.errors)[0]?.[0];
  if (firstFieldError) {
    return firstFieldError;
  }

  if (body?.title) {
    return body.title;
  }

  return 'Não foi possível completar a operação. Tente novamente.';
}
