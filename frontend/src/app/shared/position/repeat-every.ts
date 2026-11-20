import { DestroyRef, inject } from '@angular/core';

// Calls the callback right away and then on every interval, until the page calls stop() or
// is destroyed. Call it in a field initializer of the page, like httpResource, because it
// needs the page's DestroyRef to stop by itself when the tourist leaves the page.
export function repeatEvery(intervalMs: number, callback: () => void): { stop(): void } {
  // inject first: outside a field initializer it throws before any timer is started.
  const destroyRef = inject(DestroyRef);
  const timer = setInterval(callback, intervalMs);
  const stop = () => clearInterval(timer);
  destroyRef.onDestroy(stop);
  callback();
  return { stop };
}
