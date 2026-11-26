import { safeReturnUrl } from './safe-return-url';

describe('safeReturnUrl', () => {
  it('keeps an address of this app, with its query', () => {
    expect(safeReturnUrl('/payment/cart')).toBe('/payment/cart');
    expect(safeReturnUrl('/exploration?page=2')).toBe('/exploration?page=2');
  });

  it('goes to the start page when there is no address', () => {
    expect(safeReturnUrl(undefined)).toBe('/');
    expect(safeReturnUrl('')).toBe('/');
  });

  it('never leads to another site', () => {
    expect(safeReturnUrl('https://example.com')).toBe('/');
    expect(safeReturnUrl('//example.com/path')).toBe('/');
    expect(safeReturnUrl('javascript:alert(1)')).toBe('/');
  });
});
