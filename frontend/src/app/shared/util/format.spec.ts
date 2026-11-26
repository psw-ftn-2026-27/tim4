import {
  formatDate,
  formatDateTime,
  formatKm,
  formatMinutes,
  formatNumber,
  formatProgress,
} from './format';

describe('formatDate and formatDateTime', () => {
  it('write the local date as "dd.mm.yyyy." with leading zeros', () => {
    const iso = new Date(2026, 0, 5, 9, 7).toISOString();

    expect(formatDate(iso)).toBe('05.01.2026.');
    expect(formatDateTime(iso)).toBe('05.01.2026. 09:07');
  });
});

describe('formatNumber', () => {
  it('writes at most one decimal, with a comma, and none for a whole number', () => {
    expect(formatNumber(37.54)).toBe('37,5');
    expect(formatNumber(40)).toBe('40');
    expect(formatNumber(0)).toBe('0');
  });
});

describe('formatKm', () => {
  it('writes under one kilometre in metres', () => {
    expect(formatKm(0)).toBe('0 m');
    expect(formatKm(0.85)).toBe('850 m');
  });

  it('writes one kilometre and more in kilometres, with one decimal', () => {
    expect(formatKm(1)).toBe('1,0 km');
    expect(formatKm(1.44)).toBe('1,4 km');
  });

  it('never writes "1000 m" for a distance that rounds up to a kilometre', () => {
    expect(formatKm(0.9996)).toBe('1,0 km');
    expect(formatKm(0.9994)).toBe('999 m');
  });
});

describe('formatMinutes', () => {
  it('writes minutes, hours, or both', () => {
    expect(formatMinutes(0)).toBe('0 min');
    expect(formatMinutes(45)).toBe('45 min');
    expect(formatMinutes(120)).toBe('2 h');
    expect(formatMinutes(90)).toBe('1 h 30 min');
  });

  it('rounds to whole minutes first', () => {
    expect(formatMinutes(90.5)).toBe('1 h 31 min');
    expect(formatMinutes(59.6)).toBe('1 h');
  });
});

describe('formatProgress', () => {
  it('writes the share done as a whole percentage', () => {
    expect(formatProgress(0, 4)).toBe('0%');
    expect(formatProgress(1, 3)).toBe('33%');
    expect(formatProgress(4, 4)).toBe('100%');
  });

  it('is "0%", not "NaN%", when there is nothing to do', () => {
    expect(formatProgress(0, 0)).toBe('0%');
  });

  it('never goes over 100%', () => {
    expect(formatProgress(5, 4)).toBe('100%');
  });
});
