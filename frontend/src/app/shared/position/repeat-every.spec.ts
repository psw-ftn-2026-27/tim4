import { EnvironmentInjector, createEnvironmentInjector, runInInjectionContext } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { repeatEvery } from './repeat-every';

describe('repeatEvery', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('calls the callback right away and then on every interval', () => {
    vi.useFakeTimers();
    const page = createEnvironmentInjector([], TestBed.inject(EnvironmentInjector));
    const callback = vi.fn();
    runInInjectionContext(page, () => repeatEvery(10_000, callback));

    vi.advanceTimersByTime(30_000);

    expect(callback).toHaveBeenCalledTimes(4);
    page.destroy();
  });

  it('calls the callback on no further tick once the page calls stop()', () => {
    vi.useFakeTimers();
    const page = createEnvironmentInjector([], TestBed.inject(EnvironmentInjector));
    const callback = vi.fn();
    const check = runInInjectionContext(page, () => repeatEvery(10_000, callback));

    vi.advanceTimersByTime(10_000);
    check.stop();
    vi.advanceTimersByTime(30_000);

    expect(callback).toHaveBeenCalledTimes(2);
    page.destroy();
  });

  it('calls the callback on no further tick once the page is destroyed', () => {
    vi.useFakeTimers();
    // The injector stands in for the page: destroying it is the tourist leaving the page.
    const page = createEnvironmentInjector([], TestBed.inject(EnvironmentInjector));
    const callback = vi.fn();
    runInInjectionContext(page, () => repeatEvery(10_000, callback));

    page.destroy();
    vi.advanceTimersByTime(30_000);

    expect(callback).toHaveBeenCalledTimes(1);
  });
});
