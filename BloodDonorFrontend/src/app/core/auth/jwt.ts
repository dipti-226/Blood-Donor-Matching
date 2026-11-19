export interface JwtPayload {
  exp?: number;
  [claim: string]: unknown;
}

// Reads the payload of a JWT. This only decodes it. The API validates tokens, never the browser.
export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const part = token.split('.')[1];
    if (!part) {
      return null;
    }
    const base64 = part.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
    const bytes = Uint8Array.from(atob(padded), (c) => c.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes)) as JwtPayload;
  } catch {
    return null;
  }
}

export function getTokenExpiryMs(token: string): number | null {
  const payload = decodeJwtPayload(token);
  return typeof payload?.exp === 'number' ? payload.exp * 1000 : null;
}