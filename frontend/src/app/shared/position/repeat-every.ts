import { DestroyRef, inject } from '@angular/core';

// Calls the callback right away and then on every interval, until the page calls stop() or
// is destroyed. Call it in a field initializer of the page, like httpResource, because it
// needs the page's DestroyRef to stop by itself when the tourist leaves the page.
export function repeatEvery(intervalMs: number, callback: () => void): { stop(): void } {
  const timer = setInterval(callback, intervalMs);
  const stop = () => clearInterval(timer);
  inject(DestroyRef).onDestroy(stop);
  callback();
  return { stop };
}
