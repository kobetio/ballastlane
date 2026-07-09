export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

/** Shape returned by both `POST /api/auth/register` and `POST /api/auth/login`. */
export interface AuthResponse {
  userId: string;
  name: string;
  email: string;
  token: string;
  expiresAtUtc: string;
}

export interface CurrentUser {
  id: string;
  name: string;
  email: string;
}
