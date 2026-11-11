import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models';

/** First useful message of an API error (ProblemDetails detail or first validation error). */
export function requestErrorMessage(error: unknown, fallback: string): string {
  const problem = (error as HttpErrorResponse | null)?.error as ProblemDetails | null;
  const first = problem?.errors ? Object.values(problem.errors)[0]?.[0] : undefined;
  return problem?.detail ?? first ?? fallback;
}
