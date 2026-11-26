// Where to go after signing in: the page that sent the user here (requireRole or an expired
// session), but only an address of this app, never one that leads elsewhere.
export function safeReturnUrl(returnUrl: string | undefined): string {
  return returnUrl !== undefined && returnUrl.startsWith('/') && !returnUrl.startsWith('//')
    ? returnUrl
    : '/';
}
