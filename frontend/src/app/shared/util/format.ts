// How the app writes dates, amounts and lengths, so every module writes them the same way
// (frontend/AGENTS.md, UI rules, "Dates and numbers"). Plain functions: a page calls them from a
// computed() or from the template through a protected member. The server sends a date as an ISO
// string and a decimal as a number; nothing else is ever shown raw.

const pad = (value: number) => String(value).padStart(2, '0');

// "21.11.2026."
export function formatDate(iso: string): string {
  const date = new Date(iso);
  return `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()}.`;
}

// "21.11.2026. 14:05"
export function formatDateTime(iso: string): string {
  const date = new Date(iso);
  return `${formatDate(iso)} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

// "37,5": at most one decimal, with a decimal comma, and none when it is whole ("40").
export function formatNumber(value: number): string {
  return String(Math.round(value * 10) / 10).replace('.', ',');
}

// "1,4 km" from kilometres; under one kilometre in metres, "850 m". The choice is made after
// rounding to metres, so 0.9996 km is "1,0 km", not "1000 m".
export function formatKm(km: number): string {
  const meters = Math.round(km * 1000);
  return meters < 1000 ? `${meters} m` : `${km.toFixed(1).replace('.', ',')} km`;
}

// "45 min", "1 h 30 min", "2 h", in whole minutes: 90.5 minutes is "1 h 31 min".
export function formatMinutes(minutes: number): string {
  const whole = Math.round(minutes);
  const hours = Math.floor(whole / 60);
  const rest = whole % 60;
  if (hours === 0) {
    return `${rest} min`;
  }
  return rest === 0 ? `${hours} h` : `${hours} h ${rest} min`;
}

// "40%": how much of a whole is done, as a CSS width or a label. Nothing to do yet (a tour
// without key points) is "0%", never "NaN%".
export function formatProgress(done: number, total: number): string {
  return total <= 0 ? '0%' : `${Math.round((Math.min(done, total) / total) * 100)}%`;
}
