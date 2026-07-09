/**
 * Shape of the JSON body written by the API's `ExceptionHandlingMiddleware`
 * (RFC 7807 `ProblemDetails`, with an optional `errors` dictionary for
 * field-level validation failures).
 */
export interface ApiProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
