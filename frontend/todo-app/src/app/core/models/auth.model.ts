/** Credentials posted to the Identity login endpoint. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Token payload returned by ASP.NET Core Identity's /login endpoint. */
export interface AccessTokenResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}
